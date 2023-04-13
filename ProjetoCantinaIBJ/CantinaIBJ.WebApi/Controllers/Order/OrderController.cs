using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Helpers;
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
    readonly ICustomerPersonRepository _customerPersonRepository;
    readonly OrderHelper _orderHelper;
    readonly Mappers _mappers;
    readonly IMapper _mapper;
    readonly HttpUserContext _userContext;
    readonly ILogger<OrderController> _logger;

    public OrderController(
        IMapper mapper,
        ILogger<OrderController> logger,
        IOrderRepository orderRepository,
        Mappers mappers,
        HttpUserContext userContext,
        ICustomerPersonRepository customerPersonRepository,
        OrderHelper orderHelper)
    {
        _mapper = mapper;
        _logger = logger;
        _orderRepository = orderRepository;
        _mappers = mappers;
        _userContext = userContext;
        _customerPersonRepository = customerPersonRepository;
        _orderHelper = orderHelper;
    }

    /// <summary>
    /// Lista todos os pedidos
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy.User)]
    public async Task<IActionResult> ListAsync([FromQuery] int page = 0, [FromQuery] int size = 10,
        [FromQuery] string? customerName = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            ListDataPagination<Order> listData = await _orderRepository.GetListOrders(contextUser, customerName, page, size);

            var newData = new ListDataPagination<OrderReadModel>
            {
                Data = listData.Data.Select(c => _mapper.Map<OrderReadModel>(c)).ToList(),
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

            CustomerPerson? customerPerson = null;

            //validação para ver se foi preenchido id de um cliente pré-cadastrado, ou se preencheu o nome do cliente, ou um ou outro, dar exceção se nao preencher nenhum
            if (model.CustomerPersonId == null && string.IsNullOrEmpty(model.CustomerName))
                BadRequest("Informar um cliente Pré-Cadastrado, Se não cadastro, informar somente o nome do cliente");

            var order = _mapper.Map<Order>(model);

            if (order.CustomerPersonId != null && order.CustomerPersonId > 0)
                customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, order.CustomerPersonId.Value);

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

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);

            CustomerPerson? customerPerson = null;

            if (order.CustomerPersonId != null && order.CustomerPersonId > 0)
                customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, order.CustomerPersonId.Value);

            if (updateModel.Status == OrderStatus.Finished && updateModel.PaymentValue == null && updateModel.PaymentOfType == null)
                BadRequest("Se for finalizar o pedido, é obrigatório informar o valor do pagamento e ");

            _mapper.Map(updateModel, order);

            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var product in order.Products)
            {
                var totalPriceProduct = product.Quantity * product.Price;
                productsValues += totalPriceProduct;
            }

            order.TotalValue = productsValues;

            //Helper para verificar status e forma de pagamento, para fazer os cálculos devidos
            await _orderHelper.UpdateCalculatePaymentsOrder(contextUser, order, customerPerson);
            
            return NoContent();
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
            order.Status = OrderStatus.Excluded;
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