using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model.Product;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Update.Product;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Produces("application/json")]
public class ProductController : CoreController
{
    readonly IProductRepository _productRepository;
    readonly IProductHistoricRepository _productHistoricRepository;
    readonly Mappers _mappers;
    readonly IMapper _mapper;
    readonly ILogger<ProductController> _logger;

    public ProductController(
        IMapper mapper,
        ILogger<ProductController> logger,
        IProductRepository productRepository,
        IProductHistoricRepository productHistoricRepository,
        Mappers mappers)
    {
        _mapper = mapper;
        _logger = logger;
        _productRepository = productRepository;
        _productHistoricRepository = productHistoricRepository;
        _mappers = mappers;
    }

    /// <summary>
    /// Lista todos os produtos
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IAsyncEnumerable<ProductReadModel>>> ListAsync()
    {
        try
        {
            var products = await _productRepository.GetProducts();

            return Ok(products);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    /// <summary>
    /// Acessa um registro de produto por Id(Código)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductReadModel), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
                return NotFound("Produto não encontrado");

            var readProduct = _mapper.Map<ProductReadModel>(product);

            return Ok(readProduct);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cria um novo registro de um produto
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] ProductCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var product = _mapper.Map<Product>(model);
            await _productRepository.AddProductAsync(product);

            return Ok(product.Id);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza um registro de um produto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            _mapper.Map(updateModel, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.SaveChangesAsync();

            await _mappers.ProductToProductHistoric(product);

            return StatusCode(201, product);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Exclui um registro de um produto
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
                return NotFound("Produto não encontrado");

            await _productRepository.DeleteAsync(product);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    private async Task<bool> PersonExists(int id)
    {
        try
        {
            var product = await _productRepository.ListAsync();
            var hasProduct = product.Any(x => x.Id == id);

            return hasProduct;
        }
        catch (Exception e)
        {
            throw e;
        }

    }
}