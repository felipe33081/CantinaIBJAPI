using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Interfaces;
using CantinaIBJ.WebApi.Models;
using CantinaIBJ.WebApi.Models.Read.Order;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CantinaIBJ.WebApi.Controllers.Auth;

[Route("v1/[controller]")]
[Produces("application/json")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly HttpUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public AuthController(IJwtService jwtService, 
        IUserRepository userRepository, 
        HttpUserContext userContext)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    /// <summary>
    /// Realiza o login do usuário
    /// </summary>
    /// <param name="request">Modelo de dados de entrada</param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> Login([FromBody] UserRequestModel request)
    {
        var user = await _userRepository.GetUserByUsernameAsync(request.Username);
        List<Claim> claims = new();

        if (user == null || !_userRepository.VerifyPassword(user, request.Password))
        {
            return Unauthorized("Login e/ou Senha incorreto(a)s, tente novamente.");
        }
            
        switch (user.Group)
        {
            case UserGroups.Admin:
                claims = new List<Claim>()
                {
                    new Claim("userid", user.Id.ToString()),
                    new Claim("emailaddress", user.Email),
                    new Claim("admin", "true"),
                    new Claim("user", "true")
                };
                break;
            case UserGroups.User:
                claims = new List<Claim>()
                {
                    new Claim("userid", user.Id.ToString()),
                    new Claim("emailaddress", user.Email),
                    new Claim("user", "true")
                };
                break;
            default:
                return Unauthorized();
        }

        var token = _jwtService.GenerateToken(user.Name, claims);

        return Ok(new { token });
    }

    /// <summary>
    /// Valida um token Jwt
    /// </summary>
    /// <param name="token"></param>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
#if RELEASE
    [ApiExplorerSettings(IgnoreApi = true)]
#endif
    [HttpPost("validateTokenJwt")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult ValidateTokenJwt(string token)
    {
        try
        {
            var validToken = _jwtService.ValidateToken(token);
            if (validToken.Identities.Count() == 0)
                return BadRequest("Token inválido");
            return Ok(validToken);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Teste para ver o retorno da autorização pelo token
    /// </summary>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
#if RELEASE
    [ApiExplorerSettings(IgnoreApi = true)]
#endif
    [Authorize(Policy.Admin)]
    [HttpGet("isAdmin")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult Me()
    {
        var contextUser = _userContext.GetContextUser();
        var principal = HttpContext.User;
        var claim = HttpContext.User?.Identities?.FirstOrDefault()?.Claims;
        var username = claim?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
        var isAdmin = principal.HasClaim("admin", "true");
        return Ok(new { username, isAdmin });
    }
}