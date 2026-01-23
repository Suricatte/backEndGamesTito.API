using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace backEndGamesTito.API.Services
{
    public class WhatsAppService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public WhatsAppService(IConfiguration configuration)
        {
            _accountSid = configuration["TwilioSettings:AccountSid"]
                          ?? throw new ArgumentNullException("Twilio AccountSid não configurado.");
            _authToken = configuration["TwilioSettings:AuthToken"]
                         ?? throw new ArgumentNullException("Twilio AuthToken não configurado.");
            _fromPhoneNumber = configuration["TwilioSettings:FromPhoneNumber"]
                               ?? "whatsapp:+14155238886";

            // Inicializa o cliente do Twilio
            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendMessageAsync(string toPhoneNumber, string messageBody)
        {
            // O WhatsApp exige que o número tenha o prefixo "whatsapp:"
            // E o número de destino deve estar no formato E.164 (ex: +5511999999999)

            // Tratamento simples para garantir o prefixo se o usuário não mandar
            var to = toPhoneNumber.StartsWith("whatsapp:") ? toPhoneNumber : $"whatsapp:{toPhoneNumber}";
            var from = _fromPhoneNumber.StartsWith("whatsapp:") ? _fromPhoneNumber : $"whatsapp:{_fromPhoneNumber}";

            var message = await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(from),
                to: new PhoneNumber(to)
            );

            // Opcional: Logar o status
            // Console.WriteLine($"Mensagem enviada! SID: {message.Sid}");
        }
    }
}