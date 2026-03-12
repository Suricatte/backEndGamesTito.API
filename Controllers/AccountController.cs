// --- Controllers/AccountController.cs
using backEndGamesTito.Api.Models; // Para LoginRequestModel
using backEndGamesTito.API.Models; // Para RegisterRequestModel, ForgotPasswordRequestModel, ResetPasswordRequestModel
using backEndGamesTito.API.Repositories;
using backEndGamesTito.API.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
// Alias para evitar conflito com a classe de mesmo nome em Models
using DbUsuario = backEndGamesTito.API.Data.Models.Usuario;

namespace backEndGamesTito.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        // Dependências do Controller
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmailService _emailService;
        private readonly WhatsAppService _whatsAppService;
        private readonly TelegramService _telegramService;

        // --- CONSTRUTOR COM INJEÇÃO DE DEPENDÊNCIA ---
        // Recebe todos os serviços configurados no Program.cs
        public AccountController(
            UsuarioRepository usuarioRepository,
            EmailService emailService,
            WhatsAppService whatsAppService,
            TelegramService telegramService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _telegramService = telegramService;
        }

        // --- 1. ROTA DE LOGIN (Universal) ---
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            // Busca o usuário pelo Identificador (pode ser Email, Celular ou User do Telegram)
            var user = await _usuarioRepository.GetUserByIdentifierAsync(model.Identifier);

            if (user == null)
            {
                return Unauthorized(new { erro = true, message = "Usuário ou senha inválidos." });
            }

            // --- Validação de Senha ---
            string ApiKey = "mangaPara_todos_ComLeite_kkk";

            // Criptografa a senha digitada
            string PassSHA256 = ComputeSha256Hash(model.PassWordHash);

            // IMPORTANTE: Para validar, usamos sempre o E-MAIL original do banco como parte do segredo,
            // independentemente de como o usuário logou (celular ou telegram).
            string EmailSHA256 = ComputeSha256Hash(user.Email);

            string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

            bool isPasswordValid = false;
            try
            {
                // Verifica o hash BCrypt
                isPasswordValid = BCrypt.Net.BCrypt.Verify(PassCrip, user.PassWordHash);
            }
            catch (Exception)
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new { erro = true, message = "Usuário ou senha inválidos" });
            }

            // Retorna sucesso e dados básicos
            return Ok(new
            {
                usuario = new
                {
                    email = user.Email,
                    celular = user.Celular,
                    telegram = user.TelegramUsername,
                    NomeCompleto = user.NomeCompleto
                },
                erro = false,
                message = "Login realizado com sucesso!"
            });
        }

        // --- 2. ROTA DE ESQUECI A SENHA (Inteligente) ---
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            // 1. Busca o usuário
            var user = await _usuarioRepository.GetUserByIdentifierAsync(model.Identifier);

            // Por segurança, não avisamos se o usuário não existe
            if (user == null)
            {
                return Ok(new { erro = false, message = "Se o usuário existir, as instruções foram enviadas." });
            }

            // 2. Gera o Token e define validade
            string rawToken = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string tokenHashToSave = ComputeSha256Hash(rawToken);
            DateTime expiry = DateTime.Now.AddMinutes(30);

            // 3. Salva no banco (vinculado ao e-mail principal)
            await _usuarioRepository.SaveRecoveryTokenAsync(user.Email, tokenHashToSave, expiry);

            // --- LÓGICA DE DECISÃO DE CANAL ---

            // Verifica se o input é igual ao E-mail cadastrado
            bool isInputEmail = user.Email.Equals(model.Identifier, StringComparison.OrdinalIgnoreCase);

            // Verifica se o input é igual ao Telegram Username cadastrado
            bool isInputTelegram = !string.IsNullOrEmpty(user.TelegramUsername) &&
                                   user.TelegramUsername.Equals(model.Identifier, StringComparison.OrdinalIgnoreCase);

            // -> Cenário A: Usuário digitou E-mail
            if (isInputEmail)
            {
                try
                {
                    string emailBody = $"<h3>Brilho Eterno de uma Mente sem lembranças: <strong>{rawToken}</strong></h3>";
                    await _emailService.SendEmailAsync(user.Email, "Recuperação de Senha", emailBody);
                    return Ok(new { erro = false, message = "Código enviado ao E-mail." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { erro = true, message = "Erro ao enviar e-mail: " + ex.Message });
                }
            }
            // -> Cenário B: Usuário digitou Telegram
            else if (isInputTelegram)
            {
                if (!string.IsNullOrEmpty(user.TelegramChatId))
                {
                    try
                    {
                        string msg = $"🔑 *GamesTito Recuperação*\n\nSeu código é: `{rawToken}`";
                        await _telegramService.SendMessageAsync(user.TelegramChatId, msg);
                        return Ok(new { erro = false, message = "Código enviado ao Telegram." });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { erro = true, message = "Erro ao enviar Telegram: " + ex.Message });
                    }
                }
                else
                {
                    return BadRequest(new { erro = true, message = "Usuário tem Telegram, mas não iniciou o Bot (ChatId ausente)." });
                }
            }
            // -> Cenário C: Assume-se que é Celular (WhatsApp)
            else
            {
                if (!string.IsNullOrEmpty(user.Celular))
                {
                    try
                    {
                        string zapMsg = $"GamesTito: Seu código de recuperação é {rawToken}";
                        await _whatsAppService.SendMessageAsync(user.Celular, zapMsg);
                        return Ok(new { erro = false, message = "Código enviado ao WhatsApp." });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { erro = true, message = "Erro ao enviar WhatsApp: " + ex.Message });
                    }
                }
                else
                {
                    return BadRequest(new { erro = true, message = "Número de celular não cadastrado para este usuário." });
                }
            }
        }

        // --- 3. ROTA DE RESETAR SENHA ---
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            var user = await _usuarioRepository.GetUserByIdentifierAsync(model.Identifier);

            // Validações de Token e Expiração
            if (user == null || user.TokenRecoveryExpiry == null)
            {
                return BadRequest(new { erro = true, message = "Solicitação inválida." });
            }

            if (DateTime.Now > user.TokenRecoveryExpiry)
            {
                return BadRequest(new { erro = true, message = "O token expirou. Solicite novamente." });
            }

            // Compara o hash do token enviado com o do banco
            if (user.HashPass != ComputeSha256Hash(model.Token))
            {
                return BadRequest(new { erro = true, message = "Token inválido." });
            }

            // --- Geração da Nova Senha ---
            string apiKey = "mangaPara_todos_ComLeite_kkk";
            string dataString = DateTime.Now.ToString();

            string passSHA256 = ComputeSha256Hash(model.NewPassword);
            string emailSHA256 = ComputeSha256Hash(user.Email); // Sempre usa o e-mail como base

            string passCrip = passSHA256 + emailSHA256 + apiKey;
            string passBCrypt = BCrypt.Net.BCrypt.HashPassword(passCrip);

            // Rotação do HashPass (Segurança extra para invalidar o token usado)
            string hashCrip = emailSHA256 + passSHA256 + dataString + apiKey;
            string hashBCrypt = BCrypt.Net.BCrypt.HashPassword(hashCrip);

            await _usuarioRepository.UpdatePasswordAsync(user.Email, passBCrypt, hashBCrypt);

            return Ok(new { erro = false, message = "Senha atualizada com sucesso!" });
        }

        // --- 4. ROTA DE REGISTRO ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                DateTime agora = DateTime.Now;
                string dataString = agora.ToString();
                string ApiKey = "mangaPara_todos_ComLeite_kkk";

                string PassSHA256 = ComputeSha256Hash(model.PassWordHash);
                string EmailSHA256 = ComputeSha256Hash(model.Email);

                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;
                string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

                string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
                string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

                var novoUsuario = new DbUsuario
                {
                    NomeCompleto = model.NomeCompleto,
                    Email = model.Email,
                    Celular = model.Celular, // Salva celular
                    TelegramUsername = model.TelegramUsername, // Salva user Telegram
                    TelegramChatId = model.TelegramChatId, // Salva ChatId (se vier do front)

                    PassWordHash = PassBCrypt,
                    HashPass = HashBCrypt,
                    DataAtualizacao = DateTime.Now,
                    StatusId = 2
                };

                await _usuarioRepository.CreateUserAsync(novoUsuario);

                return Ok(new
                {
                    erro = false,
                    message = "Usuário cadastrado com sucesso!",
                    usuario = new
                    {
                        model.NomeCompleto,
                        model.Email
                    }
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict(new { erro = true, message = "Este email (ou usuário) já está em uso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = true, message = "Erro no sistema.", codErro = ex.Message });
            }
        }

        // --- Método Auxiliar Privado ---
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }
        }
    }
}