﻿using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models.Create.Product;
using CantinaIBJ.WebApi.Models.Read.Order;
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
    /// <response code="200">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpGet]
    [Authorize(Policy.User)]
    public async Task<IActionResult> ListAsync([FromQuery] int page = 0, [FromQuery] int size = 10,
        [FromQuery] string? searchString = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            ListDataPagination<Product> listData = await _productRepository.GetListProducts(contextUser, searchString, page, size);

            var newData = new ListDataPagination<ProductReadModel>
            {
                Data = listData.Data.Select(c => _mapper.Map<ProductReadModel>(c)).ToList(),
                Page = page,
                TotalItems = listData.TotalItems,
                TotalPages = listData.TotalPages
            };

            return Ok(newData);
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
    /// <response code="200">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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
    /// <response code="200">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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
            product.UpdatedBy = contextUser.GetCurrentUser();

            await _productRepository.UpdateAsync(product);

            await _mappers.ProductToProductHistoric(contextUser, product);

            return NoContent();
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
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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

            await _productRepository.UpdateAsync(product);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}