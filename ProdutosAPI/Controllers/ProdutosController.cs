using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Migrations;
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

        /// <summary>Buscar Produto pelo Código</summary>
        /// <remarks>GET api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="200">Produto retornardo com sucesso</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Dados do produto com base no código passado: <paramref name="id"/></returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProdutosDTO>> GetProdutosById(int id)
        {
            var produto = await _produtos.GetProdutosById(p => p.Id == id);

           // if (produto is null) return NotFound("Produto não encontrado");

            return Ok(produto);
        }

        ///<summary>Buscar todos os produtos</summary>
        ///<remarks>GET api/produtos</remarks>
        /// <response code="200">Todos os produtos retonardos</response>
        /// <response code="404">Nenhum produto foi encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Dados de todos os produtos cadastrados</returns>
        [HttpGet]
        public async Task<ActionResult<ProdutosDTO>> GetProdutos()
        {
            var produtos = await _produtos.GetProdutos();

           //  if (produtos is null) return NotFound("Nenhum produto foi encontrado");

            return Ok(produtos);
        }

        ///<summary>Cadastrar um novo produto</summary>
        ///<remarks>POST api/produtos</remarks>
        /// <response code="201">Produto cadastrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Retorna os dados produto cadastrado</returns>
        [HttpPost]
        public async Task<ActionResult<Produtos>> PostProdutos(ProdutosDTO produto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var produtoNovo = await _produtos.PostProdutos(produto);

            return CreatedAtAction(nameof(GetProdutosById), new { id = produtoNovo.Id }, produto);
        }

        ///<summary>Atualizar os dados do produto</summary>
        /// <remarks>PUT api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="204">Produto atualizado</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Produto atualizado com sucesso</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProdutos(int id, ProdutosDTO produto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _produtos.PutProdutos(id, produto);

            return NoContent();
        }
        ///<summary>Deletar o produto</summary>
        /// <remarks>DELETE api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="204">Produto deletado</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Produto deletado com sucesso</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProdutos(int id)
        {
            var produtoDeletado = await _produtos.DeleteProdutos(id);

            return NoContent();
        }
    }
}