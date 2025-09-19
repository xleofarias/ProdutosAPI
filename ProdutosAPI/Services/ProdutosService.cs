using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using System.Linq.Expressions;

namespace ProdutosAPI.Services
{
    // Implementação do serviço de produtos
    public class ProdutosService : IProdutosService
    {
        public readonly ProdutosDBContext _context;

        public ProdutosService(ProdutosDBContext context)
        {
            _context = context;
        }

        // Busca um produto por um critério específico
        public async Task<Produtos> GetProdutosById(Expression<Func<Produtos, bool>> predicate)
        {
            var produto = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(predicate);

            if (produto is null) throw new KeyNotFoundException("Produto não encontrado");

            return produto;
        }

        // Busca todos os produtos
        public async Task<IEnumerable<Produtos>> GetProdutos()
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();

            return produtos;
        }

        // Adiciona um novo produto
        public async Task<Produtos> PostProdutos(ProdutosDTO produto)
        {
            var novoProduto = new Produtos
            {
                Nome = produto.Nome,
                Preco = produto.Preco,
                Quantidade = produto.Quantidade
            };

            await _context.Produtos.AddAsync(novoProduto);
            await _context.SaveChangesAsync();

            return novoProduto;

        }

        // Atualiza um produto existente
        public async Task<bool> PutProdutos(int id,ProdutosDTO produto)
        {
            var produtoAtualizar = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);

            if (produtoAtualizar is null) throw new KeyNotFoundException("Produto não encontrado");

            produtoAtualizar.Nome = produto.Nome;
            produtoAtualizar.Preco = produto.Preco;
            produtoAtualizar.Quantidade = produto.Quantidade;

            _context.Produtos.Update(produtoAtualizar);
            await _context.SaveChangesAsync();

            return true;
        }

        // Deleta um produto por ID
        public async Task<bool> DeleteProdutos(int id)
        {
            var produtoDeletar =  await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
            
            if (produtoDeletar is null) throw new KeyNotFoundException("Produto não encontrado");
            
            _context.Produtos.Remove(produtoDeletar);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
