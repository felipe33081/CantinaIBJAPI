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
        try
        {
            using var client = new HttpClient();
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
            else
            {
                throw new Exception("Erro ao enviar mensagem de notificação via Whatsapp");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public static string FileToBase64(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string file = Convert.ToBase64String(bytes);
        return file;
    }
}