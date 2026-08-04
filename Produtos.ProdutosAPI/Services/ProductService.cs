using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using Contracts.Events;
using System.Linq.Expressions;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using Bogus;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProductService(IProductRepository productRepository
                              , IDistributedCache cache
                              , ILogger<ProductService> logger
                              , ISendEndpointProvider publishEndpoint
                              , IConfiguration configuration) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<ProductService> _logger = logger;
        private readonly ISendEndpointProvider _publishEndpoint = publishEndpoint;
        private readonly IConfiguration _configuration = configuration;

        private bool CacheEnabled => _configuration.GetValue<bool>("Cache:Enabled");
        private const string CacheKey = "List:Products";

        // Busca um produto por um critério específico
        public async Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct = default)
        {
            var produto = await _productRepository.GetByFindAsync(predicate, ct);

            if (produto is null) throw new KeyNotFoundException("Produto não encontrado");

            return produto;
        }

        // Busca todos os produtos
        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            if(!CacheEnabled)
            {
                return await _productRepository.GetAllAsync(ct);
            }

            try
            {
                //Tenta buscar o json do produtos
                string? productsJson = await _cache.GetStringAsync(CacheKey, ct);
                

                if (!string.IsNullOrEmpty(productsJson))
                {
                    var productCache = JsonSerializer.Deserialize<IEnumerable<Product>>(productsJson);

                    return productCache;
                }
            }catch(Exception ex)
            {
               _logger.LogWarning(ex, "Redis fora do ar!");
            }

            var produtos = await _productRepository.GetAllAsync(ct);

            try
            {

                var options = new DistributedCacheEntryOptions
                {
                    // Expira em 2 minutos (TTL - Time To Live)
                    // Depois disso, o Redis apaga sozinho e obriga a buscar no banco de novo.
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };

                string jsonForSave = JsonSerializer.Serialize<IEnumerable<Product>>(produtos);
                await _cache.SetStringAsync(CacheKey, jsonForSave, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível realizar o cache dos produtos");
            }
            
            return produtos;
        }

        public async Task<PagedResult<Product>> ProductPaginationDtoAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            if(!CacheEnabled)
            {
                return await _productRepository.ProductPaginationDtoAsync(pageNumber, pageSize, ct);
            }

            string? cacheKey = null;

            try
            {
                var version = await _cache.GetStringAsync("products:version", ct) ?? "0";
                cacheKey = $"v{version}:Page:{pageNumber}:Size:{pageSize}";

                var productsJson = await _cache.GetStringAsync(cacheKey, ct);
                if (!string.IsNullOrEmpty(productsJson))
                {
                    var productCache = JsonSerializer.Deserialize<PagedResult<Product>>(productsJson);

                    if(productCache is not null)
                        return productCache;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis fora do ar!");
            }

            var pagedResult = await _productRepository.ProductPaginationDtoAsync(pageNumber, pageSize, ct);


            if(cacheKey is not null) {
                try
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                    };

                    string jsonForSave = JsonSerializer.Serialize(pagedResult);
                    await _cache.SetStringAsync(cacheKey, jsonForSave, options);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível realizar o cache dos produtos");
                }
            }
            
            return pagedResult;
        }

        // Adiciona um novo produto
        public async Task<Product> CreateAsync(ProductDTO product, CancellationToken ct = default)
        {
            var productId = await _productRepository.GetByFindAsync(p => p.Name == product.Name, ct);

            if (productId is not null)
                throw new ArgumentException("Já existe um produto com esse nome cadastrado");

            var newProduct = new Product
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
            };

            if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Nome é obrigatório");
            if (product.Price <= 0) throw new ArgumentException("O preço precisa ser maior que zero");

            await _productRepository.CreateAsync(newProduct, ct);

            await InvalidateProductCacheAsync(ct);

            var evento = new ProductCreatedEvent(newProduct.Id, newProduct.Name, newProduct.Price, DateTime.UtcNow);

            try
            {
                var endpoint  = await _publishEndpoint.GetSendEndpoint(new Uri("queue:product-created"));
                await endpoint.Send(evento, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar evento para produto {Id}", newProduct.Id);
            }

            return newProduct;
        }

        // Atualiza um produto existente
        public async Task<bool> UpdateAsync(int id,ProductDTO produto, CancellationToken ct = default)
        {
            var produtoAtualizar = await _productRepository.GetByFindAsync(p => p.Id == id, ct);

            if (produtoAtualizar is null) throw new KeyNotFoundException("Produto não encontrado");

            if (produto is null) throw new ArgumentException("O produto não pode ser nulo");

            produtoAtualizar.Name = produto.Name;
            produtoAtualizar.Price = produto.Price;
            produtoAtualizar.Quantity = produto.Quantity;
            
            await _productRepository.UpdateAsync(id, produtoAtualizar, ct);

            await InvalidateProductCacheAsync(ct);

            var evento = new ProductCreatedEvent(produtoAtualizar.Id, produtoAtualizar.Name, produtoAtualizar.Price, DateTime.UtcNow);

            try
            {
                var endpoint  = await _publishEndpoint.GetSendEndpoint(new Uri("queue:product-created"));
                await endpoint.Send(evento, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar evento para produto {Id}", produtoAtualizar.Id);
            }

            return true;
        }

        // Deleta um produto por ID
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var produtoDeletar =  await _productRepository.GetByFindAsync(p => p.Id == id, ct);

            if (produtoDeletar is null) throw new KeyNotFoundException("Produto não encontrado");

            await _productRepository.DeleteAsync(id, ct);

            await InvalidateProductCacheAsync(ct);

            var evento = new ProductCreatedEvent(produtoDeletar.Id, produtoDeletar.Name, produtoDeletar.Price, DateTime.UtcNow);

            try
            {
                var endpoint  = await _publishEndpoint.GetSendEndpoint(new Uri("queue:product-created"));
                await endpoint.Send(evento, ct);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar evento para produto {Id}", produtoDeletar.Id);
            }
            return true;
        }

        public async Task SeedAsync(int count = 50, CancellationToken ct = default)
        {
            await _productRepository.SeedAsync(count, ct);
        } 

        public async Task InvalidateProductCacheAsync(CancellationToken ct = default)
        {
            await _cache.RemoveAsync(CacheKey, ct);
            await _cache.SetStringAsync("products:version", DateTime.UtcNow.Ticks.ToString(), ct);
        }
    }
}
