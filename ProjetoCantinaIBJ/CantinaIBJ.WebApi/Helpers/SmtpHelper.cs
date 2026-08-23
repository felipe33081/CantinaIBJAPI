using CantinaIBJ.Model.AppSettings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CantinaIBJ.WebApi.Helpers
{
    public class SmtpHelper
    {
        private readonly SendEmailSettings _sendEmailSettings;
        public SmtpHelper(IOptions<SendEmailSettings> sendEmailSettings)
        {
            _sendEmailSettings = sendEmailSettings.Value;
        }

        public async Task SendTokenConfirmationEmail(string name, string email, string randomToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_sendEmailSettings.EmailName, _sendEmailSettings.EmailAddress));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = "Token de cadastro";
            message.Body = new TextPart("plain")
            {
                Text = $"Olá {name},\n\n" +
                $"Por favor, confirme seu email clicando no link:\n\n" +
                $"{_sendEmailSettings.UrlConfirmation}{randomToken}"
            };

            await SendSafe(message);
        }

        public async Task SendPasswordReseted(string name, string email, string randomPassword)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_sendEmailSettings.EmailName, _sendEmailSettings.EmailAddress));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = "Recuperação de senha";
            message.Body = new TextPart("plain")
            {
                Text = $"Olá {name},\n\n" +
                $"Segue sua nova senha para acesso ao sistema 'Cantina IBJ' conforme solicitado:\n\n" +
                $"{randomPassword}"
            };

            await SendSafe(message);
        }

        // Envio offline-safe: sem SMTP configurado ou sem internet, apenas ignora.
        // O e-mail e opcional e nunca deve quebrar um fluxo no retiro sem rede.
        private async Task SendSafe(MimeMessage message)
        {
            if (string.IsNullOrWhiteSpace(_sendEmailSettings.HostDomain))
                return;

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_sendEmailSettings.HostDomain, 587, false);
                await client.AuthenticateAsync(_sendEmailSettings.EmailAddress, _sendEmailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP] E-mail ignorado (offline?): {ex.Message}");
            }
        }
    }
}
