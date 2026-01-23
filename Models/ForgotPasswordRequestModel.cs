using System.ComponentModel.DataAnnotations;

namespace backEndGamesTito.API.Models
{
    public class ForgotPasswordRequestModel
    {
        [Required(ErrorMessage = "Informe o e-mail ou celular")]
        public string Identifier { get; set; } = string.Empty;
    }
}