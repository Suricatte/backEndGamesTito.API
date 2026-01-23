using System.ComponentModel.DataAnnotations;

namespace backEndGamesTito.API.Models
{
    public class ResetPasswordRequestModel
    {
        [Required]
        public string Identifier { get; set; } = string.Empty; // Email ou Celular

        [Required(ErrorMessage = "O token é obrigatório")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        public string NewPassword { get; set; } = string.Empty;
    }
}