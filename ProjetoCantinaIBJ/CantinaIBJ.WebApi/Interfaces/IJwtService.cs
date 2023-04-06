using System.Security.Claims;

namespace CantinaIBJ.WebApi.Interfaces;

public interface IJwtService
{
    string GenerateToken(string subject, IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateToken(string token);
}