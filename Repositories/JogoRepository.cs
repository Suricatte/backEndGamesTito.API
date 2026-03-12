// --- Repositories/JogoRepository.cs

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using backEndGamesTito.API.Data.Models;

namespace backEndGamesTito.API.Repositories
{
    public class JogoRepository
    {
        private readonly string _connectionString;

        public JogoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("String de conexão 'DefaultConnection' não encontrada!");
        }

        public async Task<List<Jogo>> ListarTodosAsync()
        {
            var jogos = new List<Jogo>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var commandText = "SELECT JogoId, Titulo, Descricao, Genero, DataLancamento, Preco, Imagem FROM Jogos";
                var cmd = new SqlCommand(commandText, connection);

                await connection.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        jogos.Add(MapearJogo(reader));
                    }
                }
            }
            return jogos;
        }

        public async Task<Jogo?> ObterPorIdAsync(int jogoId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT JogoId, Titulo, Descricao, Genero, DataLancamento, Preco, Imagem FROM Jogos WHERE JogoId = @jogoId";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@jogoId", jogoId);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapearJogo(reader);
                    }
                }
            }
            return null;
        }

        private Jogo MapearJogo(SqlDataReader reader)
        {
            return new Jogo
            {
                JogoId = reader.GetInt32(reader.GetOrdinal("JogoId")),
                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                Descricao = reader.IsDBNull(reader.GetOrdinal("Descricao")) ? string.Empty : reader.GetString(reader.GetOrdinal("Descricao")),
                Genero = reader.GetString(reader.GetOrdinal("Genero")),
                DataLancamento = reader.GetDateTime(reader.GetOrdinal("DataLancamento")),
                
                // CORREÇÃO DO CAST: Lemos como object e convertemos para decimal com segurança
                Preco = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Preco"))),
                
                Imagem = reader.IsDBNull(reader.GetOrdinal("Imagem")) ? string.Empty : reader.GetString(reader.GetOrdinal("Imagem"))
            };
        }
    }
}