// --- Models/LoginRequestModel.cs

using System.ComponentModel.DataAnnotations;

namespace backEndGamesTito.Api.Models
{
    public class LoginRequestModel
    {
        // Mudamos de 'Email' para 'Identifier' e removemos a validação de [EmailAddress]
        [Required(ErrorMessage = "O campo e-mail ou celular é obrigatório!")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo senha é obrigatório!")]
        public string PassWordHash { get; set; } = string.Empty;
    }
}