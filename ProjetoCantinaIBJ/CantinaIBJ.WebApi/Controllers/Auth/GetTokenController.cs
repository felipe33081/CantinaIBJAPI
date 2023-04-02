using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CantinaIBJ.WebApi.Controllers.Auth
{
    [Route("v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class GetTokenController : CoreController
    {
        private IConfiguration _configuration;

        public GetTokenController(IConfiguration configuration)
        {

            _configuration = configuration;

        }

        private Users AuthenticationUser(Users user)
        {
            Users _user = null;
            //@ToDo: criar tabela user para criar os usuários no banco de dados

            if (user.Username == "admin" && user.Password == "12345")
            {
                _user = new Users { Username = "Felipe Nogueira" };
            }

            return _user;
        }

        private string GenerateToken(Users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], null,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetToken(Users user)
        {
            IActionResult response = Unauthorized();
            var user_ = AuthenticationUser(user);
            if (user_ != null)
            {
                var token = GenerateToken(user_);
                response = Ok( new { token = token});
            }
            return response;
        }
    }
}
