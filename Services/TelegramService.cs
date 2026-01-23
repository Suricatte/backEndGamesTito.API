using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types; // Importante para o tipo ChatId se necessário

namespace backEndGamesTito.API.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient _botClient;

        public TelegramService(IConfiguration configuration)
        {
            var token = configuration["TelegramSettings:BotToken"];

            // Verifica se o token existe antes de tentar conectar
            if (!string.IsNullOrEmpty(token))
            {
                _botClient = new TelegramBotClient(token);
            }
        }

        public async Task SendMessageAsync(string chatId, string message)
        {
            // Se o bot não foi iniciado (sem token), sai do método sem erro
            if (_botClient == null) return;

            // O Telegram exige que o ChatId seja numérico (long)
            if (long.TryParse(chatId, out long numericChatId))
            {
                try
                {
                    // CORREÇÃO: Nas versões atuais do Telegram.Bot, usa-se 'SendMessage'
                    await _botClient.SendMessage(
                        chatId: numericChatId,
                        text: message
                    );
                }
                catch (Exception ex)
                {
                    // Opcional: Logar o erro no console para debug
                    Console.WriteLine($"[Telegram Error] Falha ao enviar para {chatId}: {ex.Message}");
                    throw; // Repassa o erro para ser tratado no Controller
                }
            }
        }
    }
}