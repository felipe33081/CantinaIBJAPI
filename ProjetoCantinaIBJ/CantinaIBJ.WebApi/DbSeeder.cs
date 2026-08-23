using CantinaIBJ.Data.Context;
using CantinaIBJ.Model.Auth;
using CantinaIBJ.WebApi.Helpers;

namespace CantinaIBJ.WebApi;

/// <summary>
/// Semeadura inicial do banco SQLite. Roda no startup, apos EnsureCreated.
/// Cria um PIN padrao na primeira execucao (o usuario troca depois pelo app).
/// </summary>
public static class DbSeeder
{
    /// <summary>PIN inicial de fabrica. Deve ser trocado no primeiro uso.</summary>
    public const string DefaultPin = "1234";

    public static void Seed(PostgreSqlContext context)
    {
        if (!context.AppSetting.Any())
        {
            var (hash, salt) = PinHasher.Hash(DefaultPin);
            context.AppSetting.Add(new AppSetting { PinHash = hash, PinSalt = salt });
            context.SaveChanges();
        }
    }
}
