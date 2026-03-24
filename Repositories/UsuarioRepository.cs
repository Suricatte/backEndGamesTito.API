// --- Repositories/UsuarioRepository.cs

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using backEndGamesTito.API.Data.Models;

namespace backEndGamesTito.API.Repositories
{
    public class UsuarioRepository
    {
        private readonly string _connectionString = string.Empty;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("String de conexão 'DefaultConnection' não enconrada!");
        }

        public async Task CreateUserAsync(Usuario user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = @"
                    INSERT INTO dbo.Usuario
                        (NomeCompleto, Email, Celular, TelegramUsername, TelegramChatId, PassWordHash, HashPass, DataAtualizacao, StatusId)
                    VALUES
                        (@NomeCompleto, @Email, @Celular, @TelegramUsername, @TelegramChatId, @PassWordHash, @HashPass, @DataAtualizacao, @StatusId)
                ";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@NomeCompleto", user.NomeCompleto);
                    command.Parameters.AddWithValue("@Email", user.Email);

                    // Tratamento para Celular (opcional)
                    command.Parameters.AddWithValue("@Celular", (object)user.Celular ?? DBNull.Value);

                    // Tratamento para Telegram (opcional)
                    command.Parameters.AddWithValue("@TelegramUsername", (object)user.TelegramUsername ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TelegramChatId", (object)user.TelegramChatId ?? DBNull.Value);

                    command.Parameters.AddWithValue("@PassWordHash", user.PassWordHash);
                    command.Parameters.AddWithValue("@HashPass", user.HashPass);
                    command.Parameters.AddWithValue("@DataAtualizacao", (object)user.DataAtualizacao ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StatusId", user.StatusId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Atualizado para buscar por Email OU Celular OU Telegram
        public async Task<Usuario?> GetUserByIdentifierAsync(string identifier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // SQL atualizado para usar OR nas 3 colunas
                var commandText = @"
                    SELECT TOP 1 * FROM dbo.Usuario 
                    WHERE Email = @Identifier 
                       OR Celular = @Identifier
                       OR TelegramUsername = @Identifier
                ";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Identifier", identifier);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Usuario
                            {
                                UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                NomeCompleto = reader.GetString(reader.GetOrdinal("NomeCompleto")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),

                                // Leitura do Celular
                                Celular = reader.IsDBNull(reader.GetOrdinal("Celular"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("Celular")),

                                // Leitura do Telegram Username
                                TelegramUsername = reader.IsDBNull(reader.GetOrdinal("TelegramUsername"))
                                                   ? null
                                                   : reader.GetString(reader.GetOrdinal("TelegramUsername")),

                                // Leitura do Telegram ChatId
                                TelegramChatId = reader.IsDBNull(reader.GetOrdinal("TelegramChatId"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("TelegramChatId")),

                                PassWordHash = reader.GetString(reader.GetOrdinal("PassWordHash")),
                                HashPass = reader.GetString(reader.GetOrdinal("HashPass")),
                                DataCriacao = reader.GetDateTime(reader.GetOrdinal("DataCriacao")),

                                DataAtualizacao = reader.IsDBNull(reader.GetOrdinal("DataAtualizacao"))
                                                    ? null
                                                    : reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),

                                StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),

                                TokenRecoveryExpiry = reader.IsDBNull(reader.GetOrdinal("TokenRecoveryExpiry"))
                                                    ? null
                                                    : reader.GetDateTime(reader.GetOrdinal("TokenRecoveryExpiry"))
                            };
                        }
                    }
                }
                return null;
            }
        }

        public async Task SaveRecoveryTokenAsync(string email, string token, DateTime expiry)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Mantemos a atualização baseada no Email (chave estável)
                var commandText = @"
                    UPDATE dbo.Usuario 
                    SET HashPass = @Token, 
                        TokenRecoveryExpiry = @Expiry,
                        DataAtualizacao = GETDATE()
                    WHERE Email = @Email";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@Expiry", expiry);
                    command.Parameters.AddWithValue("@Email", email);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdatePasswordAsync(string email, string newPasswordHash, string newHashPass)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = @"
                    UPDATE dbo.Usuario 
                    SET PassWordHash = @PassWordHash, 
                        HashPass = @HashPass,
                        TokenRecoveryExpiry = NULL, 
                        DataAtualizacao = GETDATE()
                    WHERE Email = @Email";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@PassWordHash", newPasswordHash);
                    command.Parameters.AddWithValue("@HashPass", newHashPass);
                    command.Parameters.AddWithValue("@Email", email);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}