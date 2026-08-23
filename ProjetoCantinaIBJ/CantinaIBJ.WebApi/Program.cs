using CantinaIBJ.WebApi;

// Ponto de entrada da API. A construcao do host fica em AppHost.Build para ser
// reutilizada pelo host desktop (CantinaIBJ.Desktop), que sobe o mesmo servidor
// in-process e o exibe numa janela WebView2.
var app = AppHost.Build(args);
app.Run();
