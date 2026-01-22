using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosAPI.DTOs;
using ProdutosAPI.Services.Interfaces;

namespace ProdutosAPI.Controllers
{
    [Authorize]
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productsService;

        // Injeção de dependência do serviço de produtos
        public ProductController(IProductService products)
        {
            _productsService = products;
        }

        /// <summary>Buscar Produto pelo Código</summary>
        /// <remarks>GET api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="200">Produto retornardo com sucesso</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Dados do produto com base no código passado: <paramref name="id"/></returns>
        [HttpGet("{id:int}")]
        [Authorize("Admin")]
        public async Task<ActionResult<ProductDTO>> GetByIdAsync(int id)
        {
            var product = await _productsService.GetByFindAsync(p => p.Id == id);
            return Ok(product);
        }

        ///<summary>Buscar todos os produtos</summary>
        ///<remarks>GET api/produtos</remarks>
        /// <response code="200">Todos os produtos retonardos</response>
        /// <response code="404">Nenhum produto foi encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Dados de todos os produtos cadastrados</returns>
        [Authorize("Admin")]
        [HttpGet]
        public async Task<ActionResult<ProductDTO>> GetAllAsync()
        {
            var products = await _productsService.GetAllAsync();
            return Ok(products);
        }

        ///<summary>Cadastrar um novo produto</summary>
        ///<remarks>POST api/produtos</remarks>
        /// <response code="201">Produto cadastrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Retorna os dados produto cadastrado</returns>
        [Authorize("Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostAsync(ProductDTO product)
        {
            var newProduct = await _productsService.CreateAsync(product);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = newProduct.Id }, product);
        }

        ///<summary>Atualizar os dados do produto</summary>
        /// <remarks>PUT api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="204">Produto atualizado</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Produto atualizado com sucesso</returns>
        [Authorize("Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAsync(int id, ProductDTO product)
        {
            await _productsService.UpdateAsync(id, product);

            return NoContent();
        }
        ///<summary>Deletar o produto</summary>
        /// <remarks>DELETE api/produtos</remarks>
        /// <param name="id">Código do Produto</param>
        /// <response code="204">Produto deletado</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno na aplicação</response>
        /// <returns>Produto deletado com sucesso</returns>
        [Authorize("Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _productsService.DeleteAsync(id);

            return NoContent();
        }
    }
}