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
            _connectionString = configuration.GetConnectionString("ProdConnection");
        }

        public async Task<string> CreateGame(string gameName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"INSERT INTO games (game_name, question_id) VALUES (@gameName, 1);", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
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
                using (var cmd = new NpgsqlCommand(@"INSERT INTO players (player_name, avatar, game_id, correct_answers, wrong_answers, last_answer_is_correct, is_ready)
                                                    SELECT @playerName, @avatar, game.id, 0, 0, false, true
                                                    FROM
                                                    (SELECT id FROM games WHERE game_name = @gameName) game
                                                    RETURNING id;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("playerName", player.Username);
                    cmd.Parameters.AddWithValue("avatar", player.Avatar);
                    playerId = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return playerId;
        }

        public async Task SetPlayerReadyState(string gameName, string username, bool state)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"UPDATE players AS p
                                                    SET is_ready = @state
                                                     FROM games AS g 
                                                     WHERE g.id = p.game_id
                                                     AND p.player_name = @username AND g.game_name = @gameName;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("state", state);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }
        public async Task<bool> AllPlayersReady(string gameName)
        {
            bool areReady = false;
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"SELECT is_ready 
                                                 FROM players 
                                                 JOIN games ON games.id = players.game_id
                                                 WHERE game_name = @gameName;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        areReady = dr.GetFieldValue<bool>("is_ready");
                    }
                }
            }
            return areReady;
        }


        public async Task<string> SetCategory(string gameName, string category)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"UPDATE games SET category = @category WHERE game_name = @gameName;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                cmd.Parameters.AddWithValue("category", category);
                await cmd.ExecuteNonQueryAsync();
            }
            return category;
        }

        public async Task SetSettings(Game game, Settings settings)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"INSERT INTO settings (game_id, number_of_questions, time_per_question)
                                                    SELECT game.id, @numberOfQuestions, @timePerQuestion FROM
                                                    (SELECT id FROM games WHERE game_name = @gameName) game;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", game.GameName);
                    cmd.Parameters.AddWithValue("numberOfQuestions", settings.TotalNumberOfQuestions);
                    cmd.Parameters.AddWithValue("timePerQuestion", settings.TimePerQuestion);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }

        public async Task<Game> GetGame(string gameName)
        {
            var game = new Game();
            game.GameSettings = new Settings();
            game.GameName = gameName;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"SELECT category, number_of_questions, time_per_question 
                                                FROM games g
                                                JOIN settings s ON s.game_id = g.id 
                                                WHERE game_name = @gameName;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        game.GameCategory = dr.GetFieldValue<string>("category");
                        game.GameSettings.TotalNumberOfQuestions = dr.GetFieldValue<int>("number_of_questions");
                        game.GameSettings.TimePerQuestion = dr.GetFieldValue<int>("time_per_question");
                    }
                }
            }
            return game;
        }


        public async Task<List<Player>> GetPlayers(string gameName)
        {
            var players = new List<Player>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"SELECT player_name, avatar, correct_answers, wrong_answers, is_ready 
                                                FROM games g
                                                JOIN players p ON p.game_id = g.id 
                                                WHERE game_name = @gameName;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        var player = new Player();
                        player.Username = dr.GetFieldValue<string>("player_name");
                        player.Avatar = dr.GetFieldValue<string>("avatar");
                        player.CorrectAnswers = dr.GetFieldValue<int>("correct_answers");
                        player.WrongAnswers = dr.GetFieldValue<int>("wrong_answers");
                        player.IsReady = dr.GetFieldValue<bool>("is_ready");
                        players.Add(player);
                    }
                }
            }
            return players;
        }

        public async Task ResetAllPlayersReadyState(string gameName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"UPDATE players AS p
                                                    SET is_ready = @state
                                                    FROM games AS g
                                                    WHERE p.game_id = g.id
                                                    AND g.game_name = @gameName;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("state", false);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }



        public async Task<Player> SetPlayerScore(string gameName, string username, bool isCorrect)
        {
            int playerId = 0;
            var incrementCorrect = isCorrect ? 1 : 0;
            var incrementWrong = isCorrect ? 0 : 1;
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"UPDATE players AS p 
                                                     SET correct_answers = correct_answers + @incrementCorrect, wrong_answers = wrong_answers + @incrementWrong, last_answer_is_correct = @isCorrect   
                                                     FROM games AS g
                                                     WHERE g.id = p.game_id
                                                     AND game_name = @gameName AND player_name = @username RETURNING p.id;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("incrementCorrect", incrementCorrect);
                    cmd.Parameters.AddWithValue("incrementWrong", incrementWrong);
                    cmd.Parameters.AddWithValue("isCorrect", isCorrect);

                    playerId = (int)await cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return await GetPlayerDetails(playerId);
        }

        private async Task<Player> GetPlayerDetails(int playerId)
        {
            var player = new Player();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"SELECT * FROM players WHERE id = @playerId;", connection))
            {
                cmd.Parameters.AddWithValue("playerId", playerId);
                await cmd.ExecuteNonQueryAsync();
                using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        player.Username = dr.GetFieldValue<string>("player_name");
                        player.Avatar = dr.GetFieldValue<string>("avatar");
                        player.CorrectAnswers = dr.GetFieldValue<int>("correct_answers");
                        player.WrongAnswers = dr.GetFieldValue<int>("wrong_answers");
                        player.IsReady = dr.GetFieldValue<bool>("is_ready");
                        player.LastAnswerIsCorrect = dr.GetFieldValue<bool>("last_answer_is_correct");

                    }
                }
            }

            return player;
        }

        public Task<IEnumerable<Game>> GetAllGames()
        {
            throw new System.NotImplementedException();
        }

        public Task<Game> GetGame(int id)
        {
            throw new System.NotImplementedException();
        }

    }
}