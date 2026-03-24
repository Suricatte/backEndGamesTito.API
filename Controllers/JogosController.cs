using backEndGamesTito.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        [HttpGet("{jogoId:int}")] 
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