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
            _connectionString = configuration.GetConnectionString("DevConnection");
        }

        public async Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions)
        {
            var lines = await File.ReadAllLinesAsync(_questionDirectoryPath + $"{category}");
            Random rnd = new Random();
            var questions = ParseQuestions(lines).Where(x => x.Answers.Count <= 4)
                                                 .OrderBy(x => rnd.Next())
                                                 .ToList();
            await SaveQuestions(questions, gameName);
            return true;
        }
        public Task<bool> CheckAnswer(string gameName, string playerName, int answerId)
        {
            throw new NotImplementedException();
        }


        public async Task<Question> GetQuestion(string gameName, int? questionId = null)
        {
            questionId = await GetCurrentQuestionId(gameName);
            var question = new Question();
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT questions.body, answers.body, answers.is_correct
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
                            question.Body = dr.GetFieldValue<string>("questions.body");
                            answer.Body = dr.GetFieldValue<string>("answers.body");
                            answer.isCorrect = dr.GetFieldValue<bool>("answers.is_correct");
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
                using (var cmd = new NpgsqlCommand(@"SELECT question_id
                                                         FROM games 
                                                         WHERE game_name = @gameName;", connection))
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

        private async Task IncrementQuestionId(string gameName)
        {

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
        private async Task SaveQuestions(List<Question> questions, string gameName)
        {

            var i = 0;
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();
                foreach (var question in questions)
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO questions (game_id, body)
                                                    SELECT game.id, @body
                                                    FROM
                                                    (SELECT id FROM games WHERE game_name = @gameName) game
                                                    RETURNING id;", connection))
                    {
                        cmd.Parameters.AddWithValue("gameName", gameName);
                        cmd.Parameters.AddWithValue("body", question.Body);
                        var questionId = await cmd.ExecuteNonQueryAsync();
                    }
                    foreach (var answer in question.Answers)
                    {
                        using (var cmd = new NpgsqlCommand(@"INSERT INTO answers (question_id, body, is_correct)
                                                        @questionId, @body, @isCorrect;", connection))
                        {
                            cmd.Parameters.AddWithValue("questionId", gameName);
                            cmd.Parameters.AddWithValue("body", answer.Body);
                            cmd.Parameters.AddWithValue("isCorrect", answer.isCorrect);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }
        public Task<IEnumerable<Question>> GetAllQuestions(string category)
        {
            throw new NotImplementedException();
        }
    }
}