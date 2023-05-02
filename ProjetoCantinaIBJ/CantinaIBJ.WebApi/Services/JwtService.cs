using CantinaIBJ.WebApi.Interfaces;
using CantinaIBJ.Model.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CantinaIBJ.WebApi.Models;
using IdentityModel;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;

namespace CantinaIBJ.WebApi.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(
        IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(string subject, IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, subject) }.Union(claims)),
            //Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_jwtSettings.ExpiresInHours)),
            //SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key)), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            //Audience = _jwtSettings.Audience,
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key)),
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = _jwtSettings.Issuer,
                //ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                {
                    var json = new WebClient().DownloadString($"https://cognito-idp.us-east-2.amazonaws.com/us-east-2_CT2bI5jE6/.well-known/jwks.json");
                    var keys = JsonConvert.DeserializeObject<Jwks>(json).Keys;

                    var selectedKey = keys.FirstOrDefault(k => k.KeyId == identifier);
                    if (selectedKey == null)
                    {
                        throw new SecurityTokenSignatureKeyNotFoundException("Matching key not found.");
                    }

                    var rsa = new RSACng();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Exponent = Base64Url.Decode(selectedKey.Exponent),
                        Modulus = Base64Url.Decode(selectedKey.Modulus)
                    });

                    return new List<SecurityKey> { new RsaSecurityKey(rsa) };
                }
            }, out var validatedToken);

            return principal;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}