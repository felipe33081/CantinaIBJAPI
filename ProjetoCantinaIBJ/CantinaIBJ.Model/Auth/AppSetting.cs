using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Model.Auth;

/// <summary>
/// Configuracao local do aplicativo (linha unica). Guarda o PIN de acesso do
/// caixa (hash + salt) e a impressora selecionada. Fica no SQLite, sem nuvem.
/// </summary>
public class AppSetting
{
    [Key]
    public int Id { get; set; }

    public string PinHash { get; set; } = "";

    public string PinSalt { get; set; } = "";

    /// <summary>Nome da impressora do Windows escolhida para os cupons (ex.: "POS58").</summary>
    public string? PrinterName { get; set; }
}
