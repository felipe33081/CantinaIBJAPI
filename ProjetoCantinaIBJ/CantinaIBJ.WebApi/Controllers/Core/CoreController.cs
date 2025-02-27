using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Controllers.Core;

[ApiController]
[Route("v1/[controller]")]
[Produces("application/json")]
public class CoreController : Controller
{
    protected BadRequestObjectResult LoggerBadRequest(Exception e, ILogger logger)
    {
        logger.LogWarning(e.ToString());
        return new BadRequestObjectResult(new { errors = RandomHelpers.GetInnerException(e).Message });
    }

    protected BadRequestObjectResult InvalidBusinessRules(IEnumerable<string> errors, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            message = "Erro em regra de negócio";

        return BadRequest(new { errors = errors, Code = "invalid_business_rules", Message = message });
    }

    protected BadRequestObjectResult InvalidBusinessRules(string errors, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            message = "Erro em regra de negócio";

        return InvalidBusinessRules(new string[] { errors }, message);
    }

    protected BadRequestObjectResult InvalidBusinessRules(IEnumerable<ValidationResult> errorResult, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            message = "Erro em regra de negócio";

        return InvalidBusinessRules(errorResult.Select(e => e.ErrorMessage), message);
    }

    protected BadRequestObjectResult InvalidData(string errors, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            message = "Não foi possível validar os dados informados";

        return BadRequest(new { errors = errors, Code = "invalid_parameters", Message = message });
    }

    protected IEnumerable<ValidationResult> Validate(object model)
    {
        var errorResults = new List<ValidationResult>();
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var errorMessage = string.Empty;

        Validator.TryValidateObject(model, context, errorResults);

        return errorResults;
    }
}