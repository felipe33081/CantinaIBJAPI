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
        public IActionResult Login([FromBody] User request)
        {
            if (request.Username == "admin" && request.Password == "1234")
            {
                var token = _jwtService.GenerateToken("admin", new[] { new Claim("admin", "true") }, DateTime.UtcNow.AddHours(1));
                return Ok(new { token });
            }
            return Unauthorized();
        }

        [HttpPost("ValidateTokenJwt")]
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
        [Authorize]
        [HttpGet("isAdmin")]
        public IActionResult Me()
        {
            var principal = HttpContext.User;
            var username = principal.Identity.Name;
            var isAdmin = principal.HasClaim("admin", "true");
            return Ok(new { username, isAdmin });
        }
    }
}
