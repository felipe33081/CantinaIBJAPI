using CantinaIBJ.Model.User;

namespace CantinaIBJ.WebApi.Common;

public class HttpUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User _user;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User GetContextUser()
    {
        try
        {
            if (_user != null)
            {
                return _user;
            }
            
            HttpContext context = _httpContextAccessor.HttpContext;

            var claim = context?.User?.Identities?.FirstOrDefault()?.Claims;

            User user = new();
#pragma warning disable CS8601 // Possible null reference assignment.
            user.Name = claim?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
            user.Group = claim?.FirstOrDefault(c => c.Type.Contains("Group"))?.Value;
            user.Aud = claim?.FirstOrDefault(c => c.Type == "aud")?.Value;
            user.PhoneNumber = claim?.FirstOrDefault(c => c.Type == "phonenumber")?.Value;

            user.TokenCreatedIn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claim?.FirstOrDefault(c => c.Type == "iat")?.Value));
            user.TokenExpiresIn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claim?.FirstOrDefault(c => c.Type == "exp")?.Value));
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