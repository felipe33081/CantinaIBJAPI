using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Read.Order;
using CantinaIBJ.WebApi.Models.Update.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CantinaIBJ.WebApi.Common.Constants;

namespace CantinaIBJ.WebApi.Controllers.Customer;

public class CustomerPersonController : CoreController
{
    readonly IWhatsGWService _whatsGWService;
    readonly ICustomerPersonRepository _customerPersonRepository;
    readonly IMapper _mapper;
    readonly ILogger<CustomerPersonController> _logger;
    readonly HttpUserContext _userContext;
    readonly IOrderRepository _orderRepository;

    public CustomerPersonController(
        IMapper mapper,
        ILogger<CustomerPersonController> logger,
        ICustomerPersonRepository customerPersonRepository,
        HttpUserContext userContext,
        IOrderRepository orderRepository,
        IWhatsGWService whatsGWService)
    {
        _mapper = mapper;
        _logger = logger;
        _customerPersonRepository = customerPersonRepository;
        _userContext = userContext;
        _orderRepository = orderRepository;
        _whatsGWService = whatsGWService;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="name"></param>
    /// <param name="phone"></param>
    /// <param name="searchString"></param>
    /// <param name="isDeleted"></param>
    /// <param name="orderBy"></param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpGet]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(ListDataPagination<CustomerPersonReadModel>), 200)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? phone = null,
        [FromQuery] string? searchString = null,
        [FromQuery] bool isDeleted = false,
        [FromQuery] string? orderBy = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var nameLowed = string.Empty;
            var phoneLowed = string.Empty;
            if (!string.IsNullOrEmpty(name)) { nameLowed = name?.ToLower(); }
            if (!string.IsNullOrEmpty(phone)) { phoneLowed = phone?.ToLower(); }

            var customers = await _customerPersonRepository.GetListCustomerPersons(contextUser, page, size, nameLowed, phone, searchString, isDeleted, orderBy);

            // Para cada cliente, obtenha os pedidos associados
            var customerWithOrders = new List<CustomerPersonReadModel>();
            foreach (var customer in customers.Data)
            {
                var orders = await _orderRepository.GetAllByCustomerId(customer.Id); // Busca os pedidos relacionados ao cliente

                // Mapeia o cliente e adiciona os pedidos
                var customerReadModel = _mapper.Map<CustomerPersonReadModel>(customer);
                customerReadModel.Orders = _mapper.Map<List<OrderReadModel>>(orders);

                customerWithOrders.Add(customerReadModel);
            }

            // Crie o objeto de paginação
            var newData = new ListDataPagination<CustomerPersonReadModel>()
            {
                Data = customerWithOrders,
                Page = page,
                TotalItems = customers.TotalItems,
                TotalPages = customers.TotalPages
            };

            Response.Headers.Add("X-Total-Count", customers.TotalItems.ToString());

            return Ok(newData);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Acessa um registro de cliente por Id(Código)
    /// </summary>
    /// <param name="id">Id do cliente</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpGet("{id}")]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(CustomerPersonReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson == null)
                return NotFound(new { errors = "Cliente não encontrado" });

            var orders = await _orderRepository.GetAllByCustomerId(customerPerson.Id); // Busca os pedidos relacionados ao cliente

            var customerReadModel = _mapper.Map<CustomerPersonReadModel>(customerPerson);
            customerReadModel.Orders = _mapper.Map<List<OrderReadModel>>(orders);

            return Ok(customerReadModel);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cria um novo registro de um cliente
    /// </summary>
    /// <param name="model">Modelo de dados de entrada</param>#e0e1dd
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpPost]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> Create([FromBody] CustomerPersonCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            CustomerPerson customer;

            if (model.Phone != null)
            {
                string cleanedPhone = System.Text.RegularExpressions.Regex.Replace(model.Phone.ToString(), @"[()\s-]", "");

                string pattern = @"^[1-9]{2}9[0-9]{8}$";

                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedPhone, pattern))
                {
                    throw new Exception("Por favor, insira um número de telefone válido com 11 dígitos (DDD + número).");
                }

                model.Phone = cleanedPhone;
            }

            var nameLowed = model.Name.ToLower();
            customer = await _customerPersonRepository.GetCustomerPersonByNameAsync(contextUser, nameLowed);
            if (customer != null)
            {
                if (customer.IsDeleted == false)
                    return StatusCode(400, new { errors = "Não foi possível criar usuário, já existe um com o mesmo nome" });

                customer.Phone = model.Phone;
                customer.Balance = 0;
                customer.UpdatedAt = null;
                customer.UpdatedBy = null;
                customer.CreatedAt = DateTime.UtcNow;
                customer.CreatedBy = contextUser.Name;
                customer.IsDeleted = false;
                customer.Email = string.Empty;
                if (!string.IsNullOrEmpty(model.Email))
                    customer.Email = model.Email;

                await _customerPersonRepository.SaveChangesAsync();
            }
            else
            {
                customer = _mapper.Map<CustomerPerson>(model);
                await _customerPersonRepository.AddCustomerPersonAsync(contextUser, customer);
            }

            try
            {
                string message = $"*Cantina IBJ*\n\nOlá, *{customer.Name}*.\nSua conta foi aberta no retiro IBJ 2026!";

                if (customer.Phone != null)
                {
                    //await _whatsGWService.WhatsSendMessage("55" + customer.Phone, message);
                }
            }
            catch { }

            return Ok(customer.Id);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza um registro de um cliente
    /// </summary>
    /// <param name="id">Id do cliente</param>
    /// <param name="updateModel">Modelo de dados de entrada</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerPersonUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            if (updateModel.Phone != null)
            {
                string cleanedPhone = System.Text.RegularExpressions.Regex.Replace(updateModel.Phone.ToString(), @"[()\s-]", "");

                string pattern = @"^[1-9]{2}9[0-9]{8}$";

                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedPhone, pattern))
                {
                    throw new Exception("Por favor, insira um número de telefone válido com 11 dígitos (DDD + número).");
                }

                updateModel.Phone = cleanedPhone;
            }

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson is null)
                return NotFound(new { errors = "Cliente não encontrado" });

            _mapper.Map(updateModel, customerPerson);

            customerPerson.UpdatedAt = DateTime.UtcNow;
            customerPerson.UpdatedBy = contextUser.GetCurrentUser();

            await _customerPersonRepository.UpdateAsync(customerPerson);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Zera a conta de um cliente
    /// </summary>
    /// <param name="id">Id do cliente</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}/resetAccount")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ZeraConta([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson is null)
                return NotFound(new { errors = "Cliente não encontrado" });

            customerPerson.Balance = 0;
            customerPerson.UpdatedAt = DateTime.UtcNow;
            customerPerson.UpdatedBy = contextUser.GetCurrentUser();

            await _customerPersonRepository.UpdateAsync(customerPerson);

            try
            {
                string message = $"*Cantina IBJ*\n\nOlá, *{customerPerson.Name}*.\nSua conta no retiro IBJ 2026 foi paga.\nConta encerrada.";

                if (customerPerson.Phone != null)
                    await _whatsGWService.WhatsSendMessage("55" + customerPerson.Phone, message);
            }
            catch { }

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza o saldo de um cliente
    /// </summary>
    /// <param name="id">Id do cliente</param>
    /// <param name="balance">Novo saldo</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}/updateBalance")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ZeraConta([FromRoute] int id, [FromBody] decimal balance)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson is null)
                return NotFound(new { errors = "Cliente não encontrado" });

            customerPerson.Balance = balance;
            customerPerson.UpdatedAt = DateTime.UtcNow;
            customerPerson.UpdatedBy = contextUser.GetCurrentUser();

            await _customerPersonRepository.UpdateAsync(customerPerson);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Exclui um registro de um cliente
    /// </summary>
    /// <param name="id">Id do cliente</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpDelete("{id}")]
    [Authorize(Policy.ADMIN)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson == null)
                return NotFound(new { errors = "Cliente não encontrado" });

            var orders = await _orderRepository.GetAllByCustomerId(id);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.Status == OrderStatus.InProgress && order.IsDeleted == false)
                        return StatusCode(400, new { errors = "Não é possível excluir cliente com pedidos em andamento" });
                }
            }

            if (customerPerson.Balance != 0)
                return StatusCode(400, new { errors = "Não é possível excluir cliente que tenha pendências no saldo" });

            customerPerson.IsDeleted = true;
            customerPerson.UpdatedAt = DateTimeOffset.UtcNow;
            customerPerson.UpdatedBy = contextUser.GetCurrentUser();

            await _customerPersonRepository.UpdateAsync(customerPerson);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}