using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CantinaIBJ.Desktop;

/// <summary>
/// Janela principal: um WebView2 em tela cheia apontando para o servidor local.
/// </summary>
public class MainForm : Form
{
    private readonly WebView2 _webView;
    private readonly string _url;

    public MainForm(string url)
    {
        _url = url;

        Text = "Cantina IBJ";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 700);

        _webView = new WebView2 { Dock = DockStyle.Fill };
        Controls.Add(_webView);

        Load += OnLoad;
    }

    private async void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            // Cache/perfil do WebView2 em LOCALAPPDATA (evita gravar ao lado do .exe,
            // que pode estar em Program Files, sem permissao de escrita).
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CantinaIBJ", "WebView2");
            Directory.CreateDirectory(userDataFolder);

            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await _webView.EnsureCoreWebView2Async(env);

            // Sem menu de contexto/DevTools para dar cara de aplicativo nativo.
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

            _webView.CoreWebView2.Navigate(_url);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível iniciar o componente de exibição (WebView2).\n\n" +
                "Verifique se o 'Microsoft Edge WebView2 Runtime' está instalado.\n\n" +
                ex.Message,
                "Cantina IBJ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
