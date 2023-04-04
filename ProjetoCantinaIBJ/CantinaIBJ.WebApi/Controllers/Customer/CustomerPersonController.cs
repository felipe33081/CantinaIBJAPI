using AutoMapper;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Update.Customer;
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
    [Authorize]
    public async Task<ActionResult<IAsyncEnumerable<CustomerPersonReadModel>>> CustomerPersontListAsync()
    {
        try
        {
            var user = _userContext.GetContextUser();

            var customerPersons = await _customerPersonRepository.GetCustomerPersons(user);

            return Ok(customerPersons);
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
    [Authorize]
    [ProducesResponseType(typeof(CustomerPersonReadModel), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(user, id);
            if (customerPerson == null)
                return NotFound("Produto não encontrado");

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
    [Authorize]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] CustomerPersonCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var user = _userContext.GetContextUser();

            var customerPerson = _mapper.Map<CustomerPerson>(model);
            await _customerPersonRepository.AddCustomerPersonAsync(user, customerPerson);

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
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerPersonUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var user = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(user, id);
            _mapper.Map(updateModel, customerPerson);

            customerPerson.UpdatedAt = DateTime.Now;
            customerPerson.UpdatedBy = user.GetCurrentUser();

            await _customerPersonRepository.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }

        return NoContent();
    }

    /// <summary>
    /// Exclui um registro de um cliente
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = _userContext.GetContextUser();

            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(user, id);
            if (customerPerson == null)
                return NotFound("Produto não encontrado");

            customerPerson.IsDeleted = true;
            customerPerson.UpdatedAt = DateTime.Now;
            customerPerson.UpdatedBy = user.GetCurrentUser();

            await _customerPersonRepository.SaveChangesAsync();

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
            var customerPerson = await _customerPersonRepository.ListAsync();
            var hasCustomerPerson = customerPerson.Any(x => x.Id == id);

            return hasCustomerPerson;
        }
        catch (Exception e)
        {
            throw e;
        }

    }
}