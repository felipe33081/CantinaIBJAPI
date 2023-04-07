using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Update.Product;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
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
    readonly HttpUserContext _userContext;
    readonly ILogger<ProductController> _logger;

    public ProductController(
        IMapper mapper,
        ILogger<ProductController> logger,
        IProductRepository productRepository,
        IProductHistoricRepository productHistoricRepository,
        Mappers mappers,
        HttpUserContext userContext)
    {
        _mapper = mapper;
        _logger = logger;
        _productRepository = productRepository;
        _productHistoricRepository = productHistoricRepository;
        _mappers = mappers;
        _userContext = userContext;
    }

    /// <summary>
    /// Lista todos os produtos
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy.User)]
    public async Task<ActionResult<IAsyncEnumerable<ProductReadModel>>> ListAsync()
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var products = await _productRepository.GetProducts(contextUser);

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
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(ProductReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
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
    [Authorize(Policy.Admin)]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] ProductCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = _mapper.Map<Product>(model);
            await _productRepository.AddProductAsync(contextUser, product);

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
    [Authorize(Policy.Admin)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
            _mapper.Map(updateModel, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.AddProductAsync(contextUser, product);

            await _mappers.ProductToProductHistoric(contextUser, product);

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
    [Authorize(Policy.Admin)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
            if (product == null)
                return NotFound("Produto não encontrado");

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = contextUser.GetCurrentUser();

            await _productRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}