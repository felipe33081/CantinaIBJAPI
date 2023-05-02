namespace CantinaIBJ.Model.AppSettings;
#nullable disable
public class JwtSettings
{
    public string Issuer { get; set; }
    public int ExpiresInHours { get; set; }
}