using CantinaIBJ.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace CantinaIBJ.Desktop;

internal static class Program
{
    /// <summary>
    /// Ponto de entrada do app desktop. Sobe o servidor ASP.NET (API + SPA React +
    /// SQLite embutido) in-process em localhost e o exibe numa janela WebView2.
    /// Tudo offline: nenhuma dependencia de internet ou banco externo.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Instancia unica: se o app ja estiver aberto, evita subir um segundo servidor
        // (que colidiria na porta/banco) e apenas avisa o usuario.
        using var singleInstance = new Mutex(true, @"Global\CantinaIBJ_SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "O Cantina IBJ já está aberto.",
                "Cantina IBJ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        WebApplication? app = null;
        try
        {
            app = AppHost.Build(args);
            app.StartAsync().GetAwaiter().GetResult();

            ApplicationConfiguration.Initialize();
            using var form = new MainForm(AppHost.DefaultUrl);
            Application.Run(form);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível iniciar o Cantina IBJ.\n\n{ex.Message}",
                "Cantina IBJ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            try { app?.StopAsync().GetAwaiter().GetResult(); } catch { /* encerrando */ }
        }
    }
}
