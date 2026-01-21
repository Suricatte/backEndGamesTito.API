// --- Controllers/AccountController.cs
using backEndGamesTito.Api.Models;
using backEndGamesTito.API.Models;
using backEndGamesTito.API.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
// --- ADICIONAR ELEMENTOS PARA CRIPTOGRAFIA ---
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
// Usar o banco de dados com o DbUsuario e os atributos da classe Usuario
using DbUsuario = backEndGamesTito.API.Data.Models.Usuario;

namespace backEndGamesTito.API.Controllers
{
    // criando as rotas para o controller de conta
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UsuarioRepository _usuarioRepository;
        public AccountController(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }
        // Rota para registrar um novo usuário
        [HttpPost("register")]

        /* 
            ********** -- MÉTODO DE REGISTRO DE 'NOVO' USUARIO -- **********
        */
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                // *** Criando a criptografia ***

                DateTime agora = DateTime.Now;
                // Converte a data em string
                string dataString = agora.ToString();
                // Palavra passe
                string ApiKey = "mangaPara_todos_ComLeite_kkk";

                // Cria a senha e email aplicando SHA256
                string PassSHA256 = ComputeSha256Hash(model.PassWordHash);
                string EmailSHA256 = ComputeSha256Hash(model.Email);

                // Criando a string para a criptografia da senha e hash(para recuperar senha)
                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

                string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

                // Aplicando o BCrypt
                string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
                string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

                // Criando o 'array' com todos os dados do usuário para depois ser gravado
                var novoUsuario = new DbUsuario
                {
                    NomeCompleto = model.NomeCompleto,
                    Email = model.Email,
                    PassWordHash = PassBCrypt,
                    HashPass = HashBCrypt,
                    DataAtualizacao = DateTime.Now, //  OU -- DataAtualizacao = agora,
                    StatusId = 2
                };

                await _usuarioRepository.CreateUserAsync(novoUsuario);

                return Ok(new
                {
                    erro = false, // success = true,
                    message = "Usuário cadastrado com sucesso!",
                    usuario = new
                    {
                        model.NomeCompleto,
                        model.Email,
                        model.PassWordHash
                    }
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Erro de email duplicado pois o valor 'UNIQUE' está no campo no banco de dados
                return Conflict(new
                {
                    erro = true, // success = false,
                    message = "Este email já está em uso!"
                });
            }
            catch (Exception ex)
            {
                //return StatusCode(500, new { message = $"Erro: {ex.Message}" });
                return StatusCode(500, new
                {
                    erro = true, // success = false,
                    message = "Sistema indisponivel no momento tente mais tarde!",
                    codErro = $"Erro: {ex.Message}"
                });
            }
        }

        // ***** Cria uma instância de SHA256 || MÉTODO DE HASHING DO SHA256 *****
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Computa o hash do dado de entrada 'string'
                // e retorna o resultado como um 'array' de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Converte o 'array' de bytes em uma string hexadecimal
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        // Criando uma rota para login

        [HttpPost("Login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)

        {

            // 1. Busca o usuário no banco

            var user = await _usuarioRepository.GetUserByEmailAsync(model.Email);

            if (user == null)

            {

                return Unauthorized(new

                {

                    erro = true,

                    message = "Usuário ou senha inválidos."

                });

            }

            // 2. Recria o hash de login exatamente como no resgistro

            // *** Criando a criptografia ***

            // Palavra passe

            string ApiKey = "mangaPara_todos_ComLeite_kkk";

            // Cria a senha e email aplicando SHA256

            string PassSHA256 = ComputeSha256Hash(model.PassWordHash);

            string EmailSHA256 = ComputeSha256Hash(model.Email);

            // Criando a string para a criptografia da senha e hash (para recuperar senha)

            string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

            // 3. Verifica o hash usando BCrypyt

            // Compara o hash recém criado com o hash salvo no banco (user.PassWordHash)

            bool isPasswordValid;

            try

            {

                isPasswordValid = BCrypt.Net.BCrypt.Verify(PassCrip, user.PassWordHash);

            }

            catch (Exception)

            {

                isPasswordValid = false;

            }

            if (!isPasswordValid)

            {

                return Unauthorized(new

                {

                    erro = true,

                    message = "Usuário ou senha inválidos"

                });

            }

            // 4. SUCESSO +! (NO FUTURO GERA UM 'JWT')

            return Ok(new

            {

                usuario = new

                {

                    email = user.Email,

                    passWordHash = user.PassWordHash,

                    NomeCompleto = user.NomeCompleto

                },

                erro = false,

                message = "Login realizado com sucesso!"

            });

        }

    }
}
