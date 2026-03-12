namespace backEndGamesTito.API.Data.Models
{
    public class Jogo
    {
        public int JogoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime DataLancamento { get; set; }
        public decimal Preco { get; set; }
        public string Imagem { get; set; } = string.Empty;
    }
}