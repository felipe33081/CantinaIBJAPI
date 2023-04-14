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
using CantinaIBJ.WebApi.Models;
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
    readonly IProductRepository _productRepository;
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
        OrderHelper orderHelper,
        IProductRepository productRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _orderRepository = orderRepository;
        _mappers = mappers;
        _userContext = userContext;
        _customerPersonRepository = customerPersonRepository;
        _orderHelper = orderHelper;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Lista todos os pedidos
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="customerName"></param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpGet]
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(ListDataPagination<OrderReadModel>), 200)]
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
    /// <param name="id">Id do pedido</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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
    /// <param name="model">Modelo de dados de entrada</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
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

            //validação para ver se foi preenchido id de um cliente pré-cadastrado, ou se preencheu o nome do cliente, ou um ou outro, dar exceção se nao preencher nenhum
            if (model.CustomerPersonId == null && string.IsNullOrEmpty(model.CustomerName))
                return BadRequest("Informar um cliente Pré-Cadastrado, Se não cadastro, informar somente o nome do cliente");

            var order = _mapper.Map<Order>(model);

            if (model.CustomerPersonId != null && model.CustomerPersonId > 0)
            {
                var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, model.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound("Cliente não encontrado");
            }

            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var orderProduct in order.Products)
            {
                var product = await _productRepository.GetProductByIdAsync(contextUser, orderProduct.ProductId);
                orderProduct.Price = product.Price;
                var totalPriceProduct = orderProduct.Quantity * product.Price;
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
    /// <param name="id">Id do pedido</param>
    /// <param name="updateModel">Modelo de dados de entrada</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy.User)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] OrderUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            if (order == null)
                return NotFound("Pedido não encontrado");

            if (order.Status == OrderStatus.Canceled)
                return BadRequest("O pedido está cancelado, não é possível atualizar");

            if (updateModel.CustomerPersonId != null && updateModel.CustomerPersonId > 0)
            {
                var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, updateModel.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound("Cliente não encontrado");
            }
            
            _mapper.Map(updateModel, order);

            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var orderProduct in order.Products)
            {
                var product = await _productRepository.GetProductByIdAsync(contextUser, orderProduct.ProductId);
                orderProduct.Price = product.Price;
                var totalPriceProduct = orderProduct.Quantity * product.Price;
                productsValues += totalPriceProduct;
            }

            order.TotalValue = productsValues;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = contextUser.GetCurrentUser();
            await _orderRepository.UpdateAsync(order);
            
            //TODO: Fazer novo endpoint, que vai servir pro "botão" finalizar (pedido), para que esse outro endpoint possa fazer todos esses calculos do helper anterior

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Finaliza o pedido
    /// </summary>
    /// <param name="id">Id do pedido</param>
    /// <param name="requestModel">Modelo de dados de entrada</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost("{id}/finalize")]
    [Authorize(Policy.User)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> FinalizeOrder([FromRoute] int id, [FromBody] FinalizeOrderRequestModel requestModel)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            if (order == null)
                return NotFound("Pedido não encontrado");

            if (requestModel.PaymentOfType == PaymentOfType.Debitor && requestModel.PaymentValue > order.TotalValue)
                return BadRequest("Valor do pagamento não pode ser maior do que o valor do pedido, para o tipo de pagamento escolhido");

            if (requestModel.PaymentOfType == PaymentOfType.ExtraMoney && requestModel.PaymentValue < order.TotalValue)
                return BadRequest("Valor do pagamento não pode ser menor do que o valor do pedido, para o tipo de pagamento escolhido");

            if (order.Status != OrderStatus.InProgress)
                return BadRequest("Só é possível finalizar um pedido em andamento");

            CustomerPerson? customerPerson = null;
            if (order.CustomerPersonId != null && order.CustomerPersonId > 0)
            {
                customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, order.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound("Cliente não encontrado");
            }

            //Helper para verificar status e forma de pagamento, para fazer os cálculos devidos
            await _orderHelper.UpdateCalculatePaymentsOrder(contextUser, order, requestModel, customerPerson);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost("{id}/cancel")]
    [Authorize(Policy.User)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CancelOrder([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var order = await _orderRepository.GetOrderByIdAsync(contextUser, id);
            if (order is null)
                return NotFound("Pedido não encontrado");

            if (order.Status != OrderStatus.InProgress)
                return BadRequest("Só é possível cancelar um pedido em andamento");

            order.Status = OrderStatus.Canceled;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = contextUser.GetCurrentUser();
            await _orderRepository.UpdateAsync(order);
        }
        catch (Exception e)
        {
            LoggerBadRequest(e, _logger);
        }
        return NoContent();
    }

    /// <summary>
    /// Exclui um registro de um pedido
    /// </summary>
    /// <remarks>Deleta um pedido</remarks>
    /// <param name="id">Id do pedido</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpDelete("{id}")]
    [Authorize(Policy.User)]
    [ProducesResponseType(204)]
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