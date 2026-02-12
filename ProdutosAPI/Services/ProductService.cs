using Microsoft.EntityFrameworkCore;
using ProdutosAPI.DTOs;
using ProdutosAPI.Migrations;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IDistributedCache _cache;

        private const string cacheKey = "List_Products";

        public ProductService(IProductRepository productRepository, IDistributedCache cache)
        {
            _productRepository = productRepository;
            _cache = cache;
        }

        // Busca um produto por um critério específico
        public async Task<Product> GetByFindAsync(Expression<Func<Product, bool>> predicate)
        {
            var produto = await _productRepository.GetByFindAsync(predicate);

            if (produto is null) throw new KeyNotFoundException("Produto não encontrado");

            return produto;
        }

        // Busca todos os produtos
        public async Task<IEnumerable<Product>> GetAllAsync()
        {

            try
            {
                //Tenta buscar o json do produtos
                string? productsJson = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(productsJson))
                {
                    var productCache = JsonSerializer.Deserialize<IEnumerable<Product>>(productsJson);

                    return productCache;
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Redis fora do ar: {ex}");
            }

            var produtos = await _productRepository.GetAllAsync();

            if (!produtos.Any()) return Enumerable.Empty<Product>();

            try
            {

                var options = new DistributedCacheEntryOptions
                {
                    // Expira em 2 minutos (TTL - Time To Live)
                    // Depois disso, o Redis apaga sozinho e obriga a buscar no banco de novo.
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };

                string jsonForSave = JsonSerializer.Serialize<IEnumerable<Product>>(produtos);
                await _cache.SetStringAsync(cacheKey, jsonForSave, options);
            }
            catch (Exception ex)
            {

            }
            
            return produtos;
        }

        // Adiciona um novo produto
        public async Task<Product> CreateAsync(ProductDTO product)
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

            if (newProduct is null) throw new ArgumentNullException("O produto não pode ser nulo");


            try 
            {
                await _productRepository.CreateAsync(newProduct);

                // Para limpa a lista assim no próximo get irá preencher com o nome produto
                await _cache.RemoveAsync(cacheKey);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Erro ao salvar o produto: " + ex.Message);
            }
            
            return newProduct;
        }

        // Atualiza um produto existente
        public async Task<bool> UpdateAsync(int id,ProductDTO produto)
        {
            var produtoAtualizar = await _productRepository.GetByFindAsync(p => p.Id == id);

            if (produtoAtualizar is null) throw new KeyNotFoundException("Produto não encontrado");

            produtoAtualizar.Name = produto.Name;
            produtoAtualizar.Price = produto.Price;
            produtoAtualizar.Quantity = produto.Quantity;

            try 
            {
                await _productRepository.UpdateAsync(id, produtoAtualizar);

                await _cache.RemoveAsync(cacheKey);
            }
            catch(DbUpdateException ex)
            {
                throw new Exception("Erro ao tentar atualizar o produto: " + ex.Message);
            }

            return true;
        }

        // Deleta um produto por ID
        public async Task<bool> DeleteAsync(int id)
        {
            var produtoDeletar =  await _productRepository.GetByFindAsync(p => p.Id == id);

            if (produtoDeletar is null) throw new KeyNotFoundException("Produto não encontrado");

            try
            {
                await _productRepository.DeleteAsync(id);

                await _cache.RemoveAsync(cacheKey);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Erro ao tentar deletar o produto" + ex.Message);
            }

            return true;
        }
    }
}
