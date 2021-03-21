using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace quizter_be.Repository
{
    public class DBQuestionStorage : IQuestionStorage
    {
        private readonly string _connectionString;
        private readonly string _questionDirectoryPath;
        public DBQuestionStorage(string questionDirectoryPath, IConfiguration configuration)
        {
            _questionDirectoryPath = questionDirectoryPath;
            _connectionString = configuration.GetConnectionString("ProdConnection");
            // _connectionString = configuration.GetConnectionString("DevConnection");
        }

        public async Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions)
        {
            var lines = await File.ReadAllLinesAsync(_questionDirectoryPath + $"{category}");
            Random rnd = new Random();
            var questions = ParseQuestions(lines).Where(x => x.Answers.Count <= 4)
                                                 .OrderBy(x => rnd.Next())
                                                 .ToList();
            await SaveQuestions(questions, gameName, totalNumberOfQuestions);
            return true;
        }
        public async Task<AnswerResponse> CheckAnswer(string gameName, string playerName, int answerId)
        {
            var data = new AnswerResponse();
            var questionId = await GetCurrentQuestionId(gameName);
            var answers = await GetAllAnswers(gameName, questionId - 1);
            data.isCorrect = answers[answerId].isCorrect;
            data.correctAnswerId = answers.FindIndex(x => x.isCorrect);
            return data;
        }


        public async Task<Question> GetQuestion(string gameName, int? questionId = null)
        {
            questionId = await GetCurrentQuestionId(gameName);
            await IncrementQuestionId(gameName, questionId);
            var question = new Question();
            question.Answers = new List<Answer>();
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT questions.body AS q_body, answers.body AS a_body, answers.is_correct
                                                    FROM answers
                                                    LEFT JOIN questions ON questions.id = answers.question_id 
                                                    WHERE questions.id = @questionId;", connection))
                {
                    cmd.Parameters.AddWithValue("questionId", questionId);
                    using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (dr.Read())
                        {
                            var answer = new Answer();
                            question.Body = dr.GetFieldValue<string>("q_body");
                            answer.Body = dr.GetFieldValue<string>("a_body");
                            answer.isCorrect = dr.GetFieldValue<bool>("is_correct");
                            question.Answers.Add(answer);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return question;
        }

        private async Task<int> GetCurrentQuestionId(string gameName)
        {
            int questionId = 0;
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT MIN(questions.id) + MIN(games.question_id) - 1
                                                    FROM games
                                                    JOIN questions ON questions.game_id = games.id
                                                    WHERE game_name = @gameName; ", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    questionId = (int)await cmd.ExecuteScalarAsync();

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return questionId;
        }

        private async Task IncrementQuestionId(string gameName, int? questionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"UPDATE games
                                                    SET question_id = question_id + 1
                                                    WHERE game_name = @gameName;", connection))
                {
                    cmd.Parameters.AddWithValue("questionId", questionId + 1);
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private List<Question> ParseQuestions(string[] lines)
        {
            var correctAnswer = string.Empty;
            var currentQuestion = new Question();
            var questions = new List<Question>();

            try
            {
                foreach (var line in lines)
                {
                    if (line == string.Empty)
                    {
                        if (currentQuestion.Answers != null)
                            questions.Add(currentQuestion);

                        currentQuestion = new Question();
                        currentQuestion.Answers = new List<Answer>();
                    }
                    else
                    {
                        var prefix = line.Length < 3 ? string.Empty : line.Substring(0, 2);
                        switch (prefix)
                        {
                            case "#Q":
                                currentQuestion.Body = line.Substring(2);
                                break;

                            case "^ ":
                                correctAnswer = line.Substring(2);
                                break;

                            case "A ":
                            case "B ":
                            case "C ":
                            case "D ":
                                if (line.Contains(correctAnswer))
                                    currentQuestion.Answers.Add(new Answer { Body = line.Substring(2), isCorrect = true });
                                else
                                    currentQuestion.Answers.Add(new Answer { Body = line.Substring(2), isCorrect = false });
                                break;

                            default:
                                currentQuestion.Body += " " + line;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return questions;
        }
        private async Task SaveQuestions(List<Question> questions, string gameName, int totalNumberOfQuestions)
        {
            try
            {

                for (var i = 0; i < totalNumberOfQuestions; i++)
                {
                    var questionId = await SaveQuestion(gameName, questions[i]);
                    await SaveAnswers(gameName, questions[i], questionId);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task<int> SaveQuestion(string gameName, Question question)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand(@"INSERT INTO questions (game_id, body)
                                                    SELECT game.id, @body
                                                    FROM
                                                    (SELECT id FROM games WHERE game_name = @gameName) game
                                                    RETURNING id;", connection))
            {
                cmd.Parameters.AddWithValue("gameName", gameName);
                cmd.Parameters.AddWithValue("body", question.Body);
                return (int)await cmd.ExecuteScalarAsync();
            }
        }
        private async Task SaveAnswers(string gameName, Question question, int questionId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            foreach (var answer in question.Answers)
            {
                using (var cmd = new NpgsqlCommand(@"INSERT INTO answers (question_id, body, is_correct)
                                                                VALUES(@questionId, @body, @isCorrect);", connection))
                {
                    cmd.Parameters.AddWithValue("questionId", questionId);
                    cmd.Parameters.AddWithValue("body", answer.Body);
                    cmd.Parameters.AddWithValue("isCorrect", answer.isCorrect);
                    var result = await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Answer>> GetAllAnswers(string gameName, int questionId)
        {
            var answers = new List<Answer>();
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT answers.body, answers.is_correct
                                                     FROM answers
                                                     JOIN questions AS q ON q.id = answers.question_id
                                                     JOIN games AS g ON g.id = q.game_id
                                                     WHERE game_name = @gameName AND answers.question_id = @questionId;", connection))
                {
                    cmd.Parameters.AddWithValue("gameName", gameName);
                    cmd.Parameters.AddWithValue("questionId", questionId);
                    using (NpgsqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (dr.Read())
                        {
                            var answer = new Answer();
                            answer.Body = dr.GetFieldValue<string>("body");
                            answer.isCorrect = dr.GetFieldValue<bool>("is_correct");
                            answers.Add(answer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return answers;

        }
        public Task<IEnumerable<Question>> GetAllQuestions(string category)
        {
            throw new NotImplementedException();
        }
    }
}