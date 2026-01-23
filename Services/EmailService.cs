using Microsoft.Extensions.Configuration; // Necessário para ler o appsettings
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace backEndGamesTito.API.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _senderEmail;
        private readonly string _appPassword;

        // Injetamos a configuração (IConfiguration) para ler do appsettings.json
        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _port = int.Parse(configuration["EmailSettings:Port"] ?? "587");
            _senderEmail = configuration["EmailSettings:SenderEmail"]
                           ?? throw new System.ArgumentNullException("Email não configurado no appsettings.");
            _appPassword = configuration["EmailSettings:AppPassword"]
                           ?? throw new System.ArgumentNullException("Senha não configurada no appsettings.");
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpServer)
            {
                Port = _port,
                Credentials = new NetworkCredential(_senderEmail, _appPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, "Games Tito Support"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(recipientEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}