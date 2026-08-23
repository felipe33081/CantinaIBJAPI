using CantinaIBJ.Integration.WhatsGW.Models.Response;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace CantinaIBJ.Integration.WhatsGW;

public class WhatsGWCommunication : IWhatsGWService
{
    private readonly WhatsGWSettings _settings;
    public WhatsGWCommunication(IOptions<WhatsGWSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<WhatsGWSendMessageResponse?> WhatsSendMessage(string toNumber, string message)
    {
        // Modo offline: sem configuracao de WhatsApp, apenas ignora silenciosamente.
        // Notificacao e opcional e NUNCA deve quebrar uma venda no retiro sem internet.
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(_settings.ApiKey))
            return null;

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var url = _settings.BaseUrl;

            var data = new
            {
                apikey = _settings.ApiKey,
                phone_number = _settings.FromNumber,
                contact_phone_number = toNumber,
                message_type = "text",
                message_body = message
            };

            var jsonContent = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WhatsGWSendMessageResponse>(res);
            }

            return null;
        }
        catch (Exception ex)
        {
            // Sem internet / servico indisponivel: registra e segue. Nao propaga.
            Console.WriteLine($"[WhatsGW] Notificacao ignorada (offline?): {ex.Message}");
            return null;
        }
    }

    public static string FileToBase64(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string file = Convert.ToBase64String(bytes);
        return file;
    }
}