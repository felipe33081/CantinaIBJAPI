using AutoMapper;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.Customer;
using CantinaIBJ.WebApi.Models.Read.Customer;
using CantinaIBJ.WebApi.Models.Update.Customer;
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

    public CustomerPersonController(
        IMapper mapper,
        ILogger<CustomerPersonController> logger,
        ICustomerPersonRepository customerPersonRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _customerPersonRepository = customerPersonRepository;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IAsyncEnumerable<CustomerPersonReadModel>>> CustomerPersontListAsync()
    {
        try
        {
            var customerPersons = await _customerPersonRepository.GetCustomerPersons();

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
    [ProducesResponseType(typeof(CustomerPersonReadModel), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(id);
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
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<IActionResult> Create([FromBody] CustomerPersonCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var customerPerson = _mapper.Map<CustomerPerson>(model);
            await _customerPersonRepository.AddCustomerPersonAsync(customerPerson);

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
    public async Task<IActionResult> Update(int id, [FromBody] CustomerPersonUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound("Modelo não é válido");

        try
        {
            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(id);
            _mapper.Map(updateModel, customerPerson);

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
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(id);
            if (customerPerson == null)
                return NotFound("Produto não encontrado");

            await _customerPersonRepository.DeleteAsync(customerPerson);

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