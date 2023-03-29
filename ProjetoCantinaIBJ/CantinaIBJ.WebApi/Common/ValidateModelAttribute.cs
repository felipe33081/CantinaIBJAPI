using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Common;

/// <summary>
/// Validation do model antes de entrar na action 
/// </summary>
public class ValidateModelAttribute : IActionFilter
{
    private ILogger _logger;

    public ValidateModelAttribute(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ValidateModelAttribute>();
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {

    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        context.Result = new BadRequestObjectResult(context.ModelState.ToString());
        string errorMessage = "Não foi possível validar os dados informados";
        var errors = new List<DetailedError>();
        foreach (var state in context.ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                var detailedError = new DetailedError();
                var key = string.IsNullOrEmpty(state.Key) ? "" : "[" + state.Key + "] - ";
                var message = key + error.ErrorMessage;

                detailedError.Message = error.ErrorMessage;
                detailedError.MessageIntegration = message;

                if (message.Contains("Could not convert string to DateTimeOffset") == true)
                {
                    string msgError = "Data inválida";
                    detailedError.Message = msgError;
                    detailedError.MessageIntegration = msgError;
                }
                errors.Add(detailedError);
            }
        }
        context.Result = new BadRequestObjectResult(new { errors = errors, Code = "invalid_parameters", Message = errorMessage });
    }
}

public class DetailedError
{
    public string? Message { get; set; }
    public string? MessageIntegration { get; set; }
}