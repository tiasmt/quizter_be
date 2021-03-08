using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Data;

namespace quizter_be.Repository
{
    public class DBGameStorage : IGameStorage
    {
        private readonly string _connectionString;
        public DBGameStorage(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DevConnection");
        }

        public async Task<string> CreateGame(string gameName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"INSERT INTO games (game_name) VALUES (@gameName);", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                //log exception
            }

            return gameName;
        }
        public async Task<int> CreatePlayer(Player player, string gameName)
        {
            int playerId = 0;
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT game_id INTO gameId FROM games WHERE game_name = @gameName;
                                                     INSERT INTO players (player_name, avatar, game_id, correct_answers, wrong_answers, last_answer_is_correct, is_ready)
                                                     VALUES (@playerName, @avatar, gameId, 0, 0, 0, 1) RETURNING id;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("playerName", player.Username);
                    cmd.Parameters.AddWithValue("avatar", player.Avatar);
                    playerId = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                //log exception
            }

            return playerId;
        }

        public async Task SetPlayerReadyState(string gameName, string username, bool state)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT game_id INTO gameId FROM games WHERE game_name = @gameName;
                                                     UPDATE players SET is_ready = @state WHERE player_name = @username AND game_id = gameId;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("state", state);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                //log exception
            }

        }
        public async Task<bool> AllPlayersReady(string gameName)
        {
            bool areReady = false;
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"SELECT game_id INTO gameId FROM games WHERE game_name = @gameName;
                                                     SELECT is_ready FROM players WHERE game_id = gameId;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        areReady = dr.GetFieldValue<bool>("is_ready");;
                    }
                }
            }
            return areReady;
        }



        public Task<IEnumerable<Game>> GetAllGames()
        {
            throw new System.NotImplementedException();
        }

        public Task<Game> GetGame(string gameName)
        {
            throw new System.NotImplementedException();
        }

        public Task<Game> GetGame(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Player>> GetPlayers(string gameName)
        {
            throw new System.NotImplementedException();
        }

        public Task ResetAllPlayersReadyState(string gameName)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SetCategory(string gameName, string category)
        {
            throw new System.NotImplementedException();
        }


        public Task<Player> SetPlayerScore(string gameName, string username, bool isCorrect)
        {
            throw new System.NotImplementedException();
        }

        public Task SetSettings(Game game, Settings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}