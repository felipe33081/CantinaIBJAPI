using AutoMapper;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Read.Product;
using CantinaIBJ.WebApi.Models.Update.Customer;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public CustomerPersonController(
        IMapper mapper,
        ILogger<CustomerPersonController> logger,
        ICustomerPersonRepository customerPersonRepository,
        HttpUserContext userContext)
    {
        _mapper = mapper;
        _logger = logger;
        _customerPersonRepository = customerPersonRepository;
        _userContext = userContext;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy.User)]
    public async Task<IActionResult> ListAsync([FromQuery] int page = 0, [FromQuery] int size = 10,
        [FromQuery] string? searchString = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            ListDataPagination<CustomerPerson> listData = await _customerPersonRepository.GetListCustomerPersons(contextUser, searchString, page, size);

            var newData = new ListDataPagination<CustomerPersonReadModel>
            {
                Data = listData.Data.Select(c => _mapper.Map<CustomerPersonReadModel>(c)).ToList(),
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
    /// Acessa um registro de cliente por Id(Código)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(CustomerPersonReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson == null)
                return NotFound("Cliente não encontrado");

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
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy.User)]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] CustomerPersonCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = _mapper.Map<CustomerPerson>(model);
            await _customerPersonRepository.AddCustomerPersonAsync(contextUser, customerPerson);

            return Ok(customerPerson.Id);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza um registro de um cliente
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(Policy.User)]
    public async Task<IActionResult> Update([FromRoute] int id, 
                                            [FromBody] CustomerPersonUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);

            _mapper.Map(updateModel, customerPerson);

            customerPerson.UpdatedAt = DateTime.Now;
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
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy.Admin)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(contextUser, id);
            if (customerPerson == null)
                return NotFound("Cliente não encontrado");

            customerPerson.IsDeleted = true;
            customerPerson.UpdatedAt = DateTime.Now;
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