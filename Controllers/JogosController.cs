using backEndGamesTito.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
// Remova as outras referências de Models que estão sobrando e use apenas a correta:
using backEndGamesTito.API.Data.Models; 

namespace backEndGamesTito.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JogosController : ControllerBase
    {
        private readonly JogoRepository _jogoRepository;

        public JogosController(JogoRepository jogoRepository)
        {
            _jogoRepository = jogoRepository;
        }

        [HttpGet("GamesList")]
        public async Task<IActionResult> ListJogos()
        {
            try 
            {
                var jogos = await _jogoRepository.ListarTodosAsync();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro ao listar jogos.", detalhes = ex.Message });
            }
        }

        // Alterei o nome da rota aqui para evitar qualquer conflito de detecção
        [HttpGet("FindById/{jogoId:int}")] 
        public async Task<IActionResult> GetById(int jogoId)
        {
            try
            {
                var jogo = await _jogoRepository.ObterPorIdAsync(jogoId);
                if (jogo == null)
                    return NotFound(new { mensagem = "Jogo não encontrado." });

                return Ok(jogo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro ao buscar jogo.", detalhes = ex.Message });
            }
        }
    }
}