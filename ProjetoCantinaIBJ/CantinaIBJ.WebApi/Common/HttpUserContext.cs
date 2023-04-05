using CantinaIBJ.Model;
using System.Linq;

namespace CantinaIBJ.WebApi.Common;

public class HttpUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private UserContext _user;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserContext GetContextUser()
    {
        try
        {
            if (_user != null)
            {
                return _user;
            }
            
            HttpContext context = _httpContextAccessor.HttpContext;

            var claim = context?.User?.Identities?.FirstOrDefault()?.Claims;

            UserContext user = new();
#pragma warning disable CS8601 // Possible null reference assignment.
            user.UserId = claim?.FirstOrDefault(x => x.Type.Contains("role"))?.Value;
            user.Name = claim?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
            user.Email = claim?.FirstOrDefault(c => c.Type == "emailaddress")?.Value;
            user.Group = claim?.FirstOrDefault(c => c.Type.EndsWith("admin") && c.Value == "true") != null ? "admin" :
              claim?.FirstOrDefault(c => c.Type.EndsWith("user") && c.Value == "true") != null ? "user" :
              null;
            user.Aud = claim?.FirstOrDefault(c => c.Type == "aud")?.Value;

            user.TokenCreatedIn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claim?.FirstOrDefault(c => c.Type == "iat")?.Value));
            user.TokenExpiresIn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claim?.FirstOrDefault(c => c.Type == "exp")?.Value));
            if (user.TokenExpiresIn < DateTimeOffset.UtcNow)
                throw new Exception("Token expirado");
#pragma warning restore CS8601 // Possible null reference assignment.

            _user = user;

            return user;
        }
        catch
        {
            throw new Exception("Usuário não autenticado");
        }
    }
}