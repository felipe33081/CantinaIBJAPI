using CantinaIBJ.WebApi.Interfaces;
using CantinaIBJ.WebApi.Models;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CantinaIBJ.WebApi.Controllers.Auth
{
    [Route("v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("getToken")]
        public IActionResult Login([FromBody] UserRequestModel request)
        {
            if (user == null || !_userRepository.VerifyPassword(user, password))
            {
                return Unauthorized("Senha incorreta");
            }
            
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Group, user.Group)
            };

            var token = _jwtService.GenerateToken(user.Id.ToString(), claims, DateTime.UtcNow.AddHours(3));
            return Ok(new { token });
        }

        [HttpPost("ValidateTokenJwt")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ValidateTokenJwt(string token)
        {
            try
            {
                var validToken = _jwtService.ValidateToken(token);

                return Ok(validToken);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Teste para ver o retorno da autorização pelo token
        [Authorize(Policy.Admin)]
        [HttpGet("isAdmin")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult Me()
        {
            var principal = HttpContext.User;
            var claim = HttpContext.User?.Identities?.FirstOrDefault()?.Claims;
            var username = claim?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
            var isAdmin = principal.HasClaim("admin", "true");
            return Ok(new { username, isAdmin });
        }
    }
}
