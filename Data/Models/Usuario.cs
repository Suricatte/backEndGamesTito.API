
// --- Data/Models/Usuario.cs
using System;

namespace backEndGamesTito.API.Data.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Celular { get; set; } // <--- NOVO CAMPO
        public string PassWordHash { get; set; } = string.Empty;
        public string HashPass { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? TokenRecoveryExpiry { get; set; }
        public int StatusId { get; set; }

        public string? TelegramUsername { get; set; }
        public string? TelegramChatId { get; set; }
    }
}