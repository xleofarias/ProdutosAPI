using Microsoft.EntityFrameworkCore;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Repositories.Interfaces;
using ProdutosAPI.Services.Interfaces;
using System.Linq.Expressions;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProductService : IProductService
    {
        public readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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
            var produtos = await _productRepository.GetAllAsync();

            if (!produtos.Any()) throw new Exception("Nenhum produto encontrado");

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
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Erro ao tentar deletar o produto" + ex.Message);
            }

            return true;
        }
    }
}
