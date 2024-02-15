using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Update.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CantinaIBJ.WebApi.Common.Constants;

namespace CantinaIBJ.WebApi.Controllers.Customer;

[ApiController]
[Route("v1/[controller]")]
[Produces("application/json")]
public class CustomerPersonController : CoreController
{
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
        IOrderRepository orderRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _customerPersonRepository = customerPersonRepository;
        _userContext = userContext;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="searchString"></param>
    /// <param name="isDeleted"></param>
    /// <param name="orderBy"></param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpGet]
    [Authorize(Policy.USER)]
    [ProducesResponseType(typeof(ListDataPagination<CustomerPersonReadModel>), 200)]
    public async Task<IActionResult> ListAsync([FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? email = null,
        [FromQuery] string? searchString = null,
        [FromQuery] bool isDeleted = false,
        [FromQuery] string? orderBy = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var nameLowed = string.Empty; 
            var emailLowed = string.Empty;
            if (!string.IsNullOrEmpty(name)) { nameLowed = name?.ToLower(); }
            if (!string.IsNullOrEmpty(email)) { emailLowed = email?.ToLower(); }

            var customers = await _customerPersonRepository.GetListCustomerPersons(contextUser, page, size, nameLowed, emailLowed, searchString, isDeleted, orderBy);

            var newData = new ListDataPagination<CustomerPersonReadModel>()
            {
                Data = customers.Data.Select(c => _mapper.Map<CustomerPersonReadModel>(c)).ToList(),
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

            var readCustomerPerson = _mapper.Map<CustomerPersonReadModel>(customerPerson);

            return Ok(readCustomerPerson);
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
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] CustomerPersonCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            CustomerPerson customer;

            string phoneNumber = model.Phone.ToString();

            // Defina sua expressão regular para validar números de telefone
            string pattern = @"^(\([1-9]{2}\)\s?9[0-9]{4}-?[0-9]{4})$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern))
            {
                throw new Exception("Por favor, insira um número de telefone válido.");
            }

            model.Phone = System.Text.RegularExpressions.Regex.Replace(model.Phone, @"[()\s-]", "");

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
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerPersonUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var contextUser = _userContext.GetContextUser();

            string phoneNumber = updateModel.Phone.ToString();

            // Defina sua expressão regular para validar números de telefone
            string pattern = @"^(\([1-9]{2}\)\s?9[0-9]{4}-?[0-9]{4})$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern))
            {
                throw new Exception("Por favor, insira um número de telefone válido.");
            }

            updateModel.Phone = System.Text.RegularExpressions.Regex.Replace(updateModel.Phone, @"[()\s-]", "");

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
    /// Exclui um registro de um cliente
    /// </summary>
    /// <param name="id">Id do cliente</param>
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

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson == null)
                return NotFound(new { errors = "Cliente não encontrado" });

            if (_orderRepository.GetAllByCustomerId(id).Any(x => x.Status == OrderStatus.InProgress && x.IsDeleted == false))
                return StatusCode(400, new { errors = "Não é possível excluir cliente com pedidos em andamento" });

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