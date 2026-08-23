using CantinaIBJ.Data.Context;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Controllers.Auth;

/// <summary>
/// Autenticacao local por PIN (offline, sem nuvem). Substitui o antigo Cognito.
/// Cenario: caixa unico da cantina no retiro. E apenas um "portao" de tela.
/// </summary>
public class AuthController : CoreController
{
    private readonly PostgreSqlContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(PostgreSqlContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Valida o PIN de acesso. Retorna um token local simples quando correto.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var setting = _context.AppSetting.FirstOrDefault();
            if (setting == null || string.IsNullOrEmpty(request?.Pin) ||
                !PinHasher.Verify(request.Pin, setting.PinHash, setting.PinSalt))
            {
                return Unauthorized(new { message = "PIN incorreto." });
            }

            return Ok(new
            {
                token = Guid.NewGuid().ToString("N"),
                isDefaultPin = PinHasher.Verify(DbSeeder.DefaultPin, setting.PinHash, setting.PinSalt)
            });
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Troca o PIN de acesso. Exige o PIN atual.
    /// </summary>
    [HttpPost("change-pin")]
    public IActionResult ChangePin([FromBody] ChangePinRequest request)
    {
        try
        {
            var setting = _context.AppSetting.FirstOrDefault();
            if (setting == null || string.IsNullOrEmpty(request?.CurrentPin) ||
                !PinHasher.Verify(request.CurrentPin, setting.PinHash, setting.PinSalt))
            {
                return Unauthorized(new { message = "PIN atual incorreto." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPin) || request.NewPin.Length < 4)
                return InvalidData("O novo PIN deve ter ao menos 4 digitos.");

            var (hash, salt) = PinHasher.Hash(request.NewPin);
            setting.PinHash = hash;
            setting.PinSalt = salt;
            _context.SaveChanges();

            return Ok(new { message = "PIN atualizado." });
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    public class LoginRequest
    {
        public string Pin { get; set; } = "";
    }

    public class ChangePinRequest
    {
        public string CurrentPin { get; set; } = "";
        public string NewPin { get; set; } = "";
    }
}
