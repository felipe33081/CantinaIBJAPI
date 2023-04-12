using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models.Create.Order;
using CantinaIBJ.WebApi.Models.Read.Order;
using CantinaIBJ.WebApi.Models.Update.Order;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Produces("application/json")]
public class OrderController : CoreController
{
    readonly IOrderRepository _orderRepository;
    readonly Mappers _mappers;
    readonly IMapper _mapper;
    readonly HttpUserContext _userContext;
    readonly ILogger<OrderController> _logger;

    public OrderController(
        IMapper mapper,
        ILogger<OrderController> logger,
        IOrderRepository orderRepository,
        Mappers mappers,
        HttpUserContext userContext)
    {
        _mapper = mapper;
        _logger = logger;
        _orderRepository = orderRepository;
        _mappers = mappers;
        _userContext = userContext;
    }

    /// <summary>
    /// Lista todos os pedidos
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy.User)]
    public async Task<ActionResult<IAsyncEnumerable<OrderReadModel>>> ListAsync()
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var orders = await _orderRepository.GetOrders(contextUser);

            return Ok(orders);
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    /// <summary>
    /// Acessa um registro de pedido por Id(Código)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(OrderReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            if (order == null)
                return NotFound("Pedido não encontrado");

            var readOrder = _mapper.Map<OrderReadModel>(order);

            return Ok(readOrder);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cria um novo registro de um pedido
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] OrderCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            //TODO: Verificar se foi preenchido o tipo de pagamento já na criação, se sim, validar os passos abaixo:
            //TODO: criar validações pro Status do pedido, caso o usuario/vendedor tenha atualizado o status para "Encerrado"(verificar o tipo de pagamento se foi avista, pix, ou ficou devendo, para
            //atualizar também o debitBalance ou creditBalando do cliente, caso o cliente tenha cadastro) e retornar o endpoint com status code 201
            //Se o Status for Cancelado, cria com status de cancelado

            //validação para ver se foi preenchido id de um cliente pré-cadastrado, ou se preencheu o nome do cliente, ou um ou outro, dar exceção se nao preencher nenhum
            if (model.CustomerPersonId == null && string.IsNullOrEmpty(model.CustomerName))
                BadRequest("Informar um cliente Pré-Cadastrado, Se não cadastro, informar somente o nome do cliente");

            var order = _mapper.Map<Order>(model);
            
            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var product in order.Products)
            {
                var totalPriceProduct = product.Quantity * product.Price;
                productsValues += totalPriceProduct;
            }

            order.TotalValue = productsValues;

            await _orderRepository.AddOrderAsync(contextUser, order);

            return Ok(order.Id);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza um registro de um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(Policy.User)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] OrderUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            //TODO: criar validações pro Status do pedido, caso o usuario/vendedor tenha atualizado o status para "Encerrado"(verificar o tipo de pagamento se foi avista, pix, ou ficou devendo, para
            //atualizar também o debitBalance ou creditBalando do cliente, caso o cliente tenha cadastro) e retornar o endpoint com status code 201
            //Se o Status for Cancelado, exclui


            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            order = _mapper.Map<Order>(updateModel);

            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var product in order.Products)
            {
                var totalPriceProduct = product.Quantity * product.Price;
                productsValues += totalPriceProduct;
            }

            order.TotalValue = productsValues;

            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = contextUser.GetCurrentUser();

            await _orderRepository.UpdateAsync(order);

            return StatusCode(201, order);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Exclui um registro de um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy.User)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            if (order == null)
                return NotFound("Pedido não encontrado");

            order.IsDeleted = true;
            order.UpdatedAt = DateTime.Now;
            order.UpdatedBy = contextUser.GetCurrentUser();

            await _orderRepository.UpdateAsync(order);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}