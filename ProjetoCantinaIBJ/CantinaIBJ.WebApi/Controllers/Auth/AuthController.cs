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
            if (request.Username == "admin" && request.Password == "1234")
            {
                //Temporario @ToDo: AJUSTAr
                List<Claim> claims = new();
                Claim claim = new Claim("admin", "true");
                Claim claim1 = new Claim("name", "felipe");
                Claim claim2 = new Claim("group", "admin");
                claims.Add(claim);
                claims.Add(claim1);
                claims.Add(claim2);

                var token = _jwtService.GenerateToken("Júlio Ramos", claims, DateTime.UtcNow.AddHours(1));
                return Ok(new { token });
            }
            return Unauthorized();
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
        [Authorize]
        [HttpGet("isAdmin")]
        [ApiExplorerSettings(IgnoreApi = true)]
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
