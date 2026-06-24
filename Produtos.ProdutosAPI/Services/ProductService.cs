using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using Contracts.Events;
using System.Linq.Expressions;
using ProdutosAPI.Repositories;
using Bogus;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProductService(ProductRepository productRepository, IDistributedCache cache, ILogger<ProductService> logger, IPublishEndpoint publishEndpoint)
    {
        private readonly ProductRepository _productRepository = productRepository;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<ProductService> _logger = logger;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        private const string CacheKey = "List_Products";

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
            try
            {
                //Tenta buscar o json do produtos
                string? productsJson = await _cache.GetStringAsync(CacheKey, ct);
                

                // Entender isso aqui depois
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
            return await _productRepository.ProductPaginationDtoAsync(pageNumber, pageSize, ct);
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
            // Para limpa a lista assim no próximo get irá preencher com o nome produto
            await _cache.RemoveAsync(CacheKey);

            var evento = new ProductCreatedEvent(newProduct.Id, newProduct.Name, newProduct.Price, DateTime.UtcNow);

            try
            {
                await _publishEndpoint.Publish(evento);
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
            await _cache.RemoveAsync(CacheKey);

            var evento = new ProductCreatedEvent(produtoAtualizar.Id, produtoAtualizar.Name, produtoAtualizar.Price, DateTime.UtcNow);

            try
            {
                await _publishEndpoint.Publish(evento);
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
            await _cache.RemoveAsync(CacheKey);

            var evento = new ProductCreatedEvent(produtoDeletar.Id, produtoDeletar.Name, produtoDeletar.Price, DateTime.UtcNow);

            try
            {
                await _publishEndpoint.Publish(evento);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar evento para produto {Id}", produtoDeletar.Id);
            }
            return true;
        }

        public async Task SeedAsync(int count = 50, CancellationToken ct = default)
        {
            var faker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Finance.Amount(1, 1000).ToString("0.00")))
                .RuleFor(p => p.Quantity, f => f.Random.Number(1, 100));

            var products = faker.Generate(count);

            foreach (var product in products)
            {
                await _productRepository.CreateAsync(product, ct);
            }
        } 
    }
}
