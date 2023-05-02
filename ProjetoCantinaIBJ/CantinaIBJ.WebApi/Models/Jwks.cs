using IdentityModel;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace CantinaIBJ.WebApi.Models;

public class Jwks
{
    [JsonProperty("keys")]
    public List<JwksKey> Keys { get; set; }
}

public class JwksKey
{
    [JsonProperty("kty")]
    public string KeyType { get; set; }

    [JsonProperty("kid")]
    public string KeyId { get; set; }

    [JsonProperty("use")]
    public string Use { get; set; }

    [JsonProperty("alg")]
    public string Algorithm { get; set; }

    [JsonProperty("n")]
    public string Modulus { get; set; }

    [JsonProperty("e")]
    public string Exponent { get; set; }

    [JsonProperty("d")]
    public string D { get; set; }

    [JsonProperty("p")]
    public string P { get; set; }

    [JsonProperty("q")]
    public string Q { get; set; }

    [JsonProperty("dp")]
    public string DP { get; set; }

    [JsonProperty("dq")]
    public string DQ { get; set; }

    [JsonProperty("qi")]
    public string QI { get; set; }

    public RSA ToRsa()
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64Url.Decode(Modulus),
            Exponent = Base64Url.Decode(Exponent),
            D = D != null ? Base64Url.Decode(D) : null,
            P = P != null ? Base64Url.Decode(P) : null,
            Q = Q != null ? Base64Url.Decode(Q) : null,
            DP = DP != null ? Base64Url.Decode(DP) : null,
            DQ = DQ != null ? Base64Url.Decode(DQ) : null,
            InverseQ = QI != null ? Base64Url.Decode(QI) : null
        });

        return rsa;
    }
}