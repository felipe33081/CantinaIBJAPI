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

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_sendEmailSettings.HostDomain, 587, false);
                await client.AuthenticateAsync(_sendEmailSettings.EmailAddress, _sendEmailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
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

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_sendEmailSettings.HostDomain, 587, false);
                await client.AuthenticateAsync(_sendEmailSettings.EmailAddress, _sendEmailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
