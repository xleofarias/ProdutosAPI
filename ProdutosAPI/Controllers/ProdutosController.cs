using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Datas;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;
using ProdutosAPI.Services;

namespace ProdutosAPI.Controllers
{
    [Route("api/produtos")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        public readonly IProdutosService _produtos;

        public ProdutosController(IProdutosService produtos)
        {
            _produtos = produtos;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProdutosDTO>> GetProdutosById(int id)
        {
            var produto = await _produtos.GetProdutosById(p => p.Id == id);

            //if (produto is null) return NotFound("Produto não encontrado");

            return Ok(produto);
        }

        [HttpGet]
        public async Task<ActionResult<ProdutosDTO>> GetProdutos()
        {
            var produtos = await _produtos.GetProdutos();

           // if (produtos is null) return NotFound("Nenhum produto foi encontrado");

            return Ok(produtos);
        }

        [HttpPost]
        public async Task<ActionResult<Produtos>> PostProdutos(ProdutosDTO produto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var produtoNovo = await _produtos.PostProdutos(produto);

            return CreatedAtAction(nameof(GetProdutosById), new { id = produtoNovo.Id }, produto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<bool>> PutProdutos(int id, ProdutosDTO produto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _produtos.PutProdutos(id, produto);

            return Ok("Produto atualizado com sucesso");
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> DeleteProdutos(int id)
        {
            var produtoDeletado = await _produtos.DeleteProdutos(id);

            return Ok("Produto deletado com sucesso");
        }
    }
}
