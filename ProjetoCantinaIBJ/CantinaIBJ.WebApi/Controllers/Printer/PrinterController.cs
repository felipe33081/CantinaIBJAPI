using CantinaIBJ.Data.Context;
using CantinaIBJ.Model.Interfaces;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Controllers.Printer;

/// <summary>
/// Configuracao da impressora POS (offline). Lista as impressoras do Windows,
/// guarda a escolhida no banco local e permite imprimir um cupom de teste.
/// </summary>
public class PrinterController : CoreController
{
    private readonly PostgreSqlContext _context;
    private readonly IPrinterService _printerService;
    private readonly ILogger<PrinterController> _logger;

    public PrinterController(
        PostgreSqlContext context,
        IPrinterService printerService,
        ILogger<PrinterController> logger)
    {
        _context = context;
        _printerService = printerService;
        _logger = logger;
    }

    /// <summary>Lista as impressoras instaladas no Windows.</summary>
    [HttpGet("list")]
    public IActionResult List()
    {
        try
        {
            return Ok(RawPrinterHelper.GetInstalledPrinters());
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>Retorna a impressora atualmente selecionada.</summary>
    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        var setting = _context.AppSetting.FirstOrDefault();
        return Ok(new { printerName = setting?.PrinterName });
    }

    /// <summary>Salva a impressora selecionada.</summary>
    [HttpPost("settings")]
    public IActionResult SaveSettings([FromBody] PrinterSettingsRequest request)
    {
        try
        {
            var setting = _context.AppSetting.FirstOrDefault();
            if (setting == null)
                return InvalidData("Configuracao do aplicativo nao encontrada.");

            setting.PrinterName = string.IsNullOrWhiteSpace(request?.PrinterName) ? null : request.PrinterName;
            _context.SaveChanges();

            return Ok(new { printerName = setting.PrinterName });
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>Imprime um cupom de teste na impressora selecionada.</summary>
    [HttpPost("test")]
    public IActionResult Test()
    {
        try
        {
            _printerService.ImprimirTeste();
            return Ok(new { message = "Teste enviado para a impressora." });
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    public class PrinterSettingsRequest
    {
        public string? PrinterName { get; set; }
    }
}
