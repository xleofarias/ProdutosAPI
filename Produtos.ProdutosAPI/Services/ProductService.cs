using Microsoft.EntityFrameworkCore;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using Contracts.Events;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ProductService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        private const string CacheKey = "List_Products";

        public ProductService(IProductRepository productRepository, IDistributedCache cache, ILogger<ProductService> logger, IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _cache = cache;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        // Busca um produto por um critério específico
        public async Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellation = default)
        {
            var produto = await _productRepository.GetByFindAsync(predicate);

            if (produto is null) throw new KeyNotFoundException("Produto não encontrado");

            return produto;
        }

        // Busca todos os produtos
        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellation = default)
        {
            try
            {
                //Tenta buscar o json do produtos
                string? productsJson = await _cache.GetStringAsync(CacheKey);

                if (!string.IsNullOrEmpty(productsJson))
                {
                    var productCache = JsonSerializer.Deserialize<IEnumerable<Product>>(productsJson);

                    return productCache;
                }
            }catch(Exception ex)
            {
               _logger.LogWarning(ex, "Redis fora do ar!");
            }

            var produtos = await _productRepository.GetAllAsync();

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

        // Adiciona um novo produto
        public async Task<Product> CreateAsync(ProductDTO product, CancellationToken cancellation = default)
        {
            var productId = await _productRepository.GetByFindAsync(p => p.Name == product.Name);

            if (productId is not null)
                throw new ArgumentException("Já existe um produto com esse nome cadastrado");

            var newProduct = new Product
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
            };

            if (product is null) throw new ArgumentNullException("O produto não pode ser nulo");
            if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Nome é obrigatório");
            if (product.Price <= 0) throw new ArgumentException("O preço precisa ser maior que zero");

            await _productRepository.CreateAsync(newProduct);
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
        public async Task<bool> UpdateAsync(int id,ProductDTO produto, CancellationToken cancellation = default)
        {
            var produtoAtualizar = await _productRepository.GetByFindAsync(p => p.Id == id);

            if (produtoAtualizar is null) throw new KeyNotFoundException("Produto não encontrado");

            if (produto is null) throw new ArgumentException("O produto não pode ser nulo");

            produtoAtualizar.Name = produto.Name;
            produtoAtualizar.Price = produto.Price;
            produtoAtualizar.Quantity = produto.Quantity;
            
            await _productRepository.UpdateAsync(id, produtoAtualizar);
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
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellation = default)
        {
            var produtoDeletar =  await _productRepository.GetByFindAsync(p => p.Id == id);

            if (produtoDeletar is null) throw new KeyNotFoundException("Produto não encontrado");

            await _productRepository.DeleteAsync(id);
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
    }
}
