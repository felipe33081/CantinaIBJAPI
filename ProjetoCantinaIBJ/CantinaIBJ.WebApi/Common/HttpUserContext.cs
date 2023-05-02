using CantinaIBJ.Model;
using System.Linq;
using static CantinaIBJ.WebApi.Common.Constants;

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

            var isApiClient = claim.Where(c => c.Value.Contains("apicantina/") && c.Type == "scope").Count() > 0;
            var groupsUserApi = claim.Where(c => c.Value.Contains("apicantina/") &&
                                           c.Type == "scope")
                                           .Select(x => x.Value?.Split('/')?.Last());

            UserContext user = new();
#pragma warning disable CS8601 // Possible null reference assignment.
            user.UserId = claim?.FirstOrDefault(x => x.Type.Contains(Cognito.ID))?.Value;
            user.Name = claim?.LastOrDefault(x => x.Type == Cognito.USERNAME)?.Value;
            user.Email = claim?.LastOrDefault(c => c.Type == Cognito.EMAIL)?.Value;
            user.PhoneNumber = claim?.LastOrDefault(c => c.Type == Cognito.PHONE_NUMBER)?.Value;
            user.Tenant = claim?.FirstOrDefault(x => x.Type == Cognito.ISSUER)?.Value?.Split('/').Last();
            user.Group = claim?.FirstOrDefault(c => c.Type.EndsWith(Cognito.GROUPS) && c.Value == "Admin") != null ? "Admin" :
              claim?.FirstOrDefault(c => c.Type.EndsWith(Cognito.GROUPS) && c.Value == "User") != null ? "User" :
              string.Empty;

            user.JwtId = claim?.FirstOrDefault(c => c.Type == "jti")?.Value;

            user.TokenCreatedIn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claim?.FirstOrDefault(c => c.Type == "auth_time")?.Value));
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