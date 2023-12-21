using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Update.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CantinaIBJ.WebApi.Common.Constants;

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
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="searchString"></param>
    /// <param name="isDeleted"></param>
    /// <param name="orderBy"></param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpGet]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(ListDataPagination<ProductReadModel>), 200)]
    public async Task<IActionResult> ListAsync([FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? description = null,
        [FromQuery] string? searchString = null,
        [FromQuery] bool isDeleted = false,
        [FromQuery] string? orderBy = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var products = await _productRepository.GetListProducts(contextUser, page, size, name, description, searchString, isDeleted, orderBy);

            var newData = new ListDataPagination<ProductReadModel>()
            {
                Data = products.Data.Select(c => _mapper.Map<ProductReadModel>(c)).ToList(),
                Page = page,
                TotalItems = products.TotalItems,
                TotalPages = products.TotalPages
            };
            Response.Headers.Add("X-Total-Count", products.TotalItems.ToString());

            return Ok(newData);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Acessa um registro de produto por Id(Código)
    /// </summary>
    /// <param name="id">Id do produto</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpGet("{id}")]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(ProductReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
            if (product == null)
                return NotFound(new { errors = "Produto não encontrado" });

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
    /// <param name="model">Modelo de dados de entrada</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] ProductCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = _mapper.Map<Product>(model);
            
            if (product.Quantity > 0)
                product.Disponibility = true;

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
    /// <param name="id">Id do produto</param>
    /// <param name="updateModel">Modelo de dados de entrada</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
            if (product is null)
                return NotFound(new { errors = "Produto não encontrado" });

            _mapper.Map(updateModel, product);

            product.UpdatedAt = DateTimeOffset.UtcNow;
            product.UpdatedBy = contextUser.GetCurrentUser();

            await _productRepository.UpdateAsync(product);

            await _mappers.ProductToProductHistoric(contextUser, product);

            return Ok(product);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Exclui um registro de um produto
    /// </summary>
    /// <param name="id">Id do produto</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpDelete("{id}")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var product = await _productRepository.GetProductByIdAsync(contextUser, id);
            if (product == null)
                return NotFound(new { errors = "Produto não encontrado" });

            product.IsDeleted = true;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            product.UpdatedBy = contextUser.GetCurrentUser();

            await _productRepository.UpdateAsync(product);

            return Ok(product);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}