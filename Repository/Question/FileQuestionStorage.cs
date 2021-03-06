using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public class FileQuestionStorage : IQuestionStorage
    {
        private readonly string _questionDirectoryPath;
        private readonly string _gameDirectoryPath;
        private readonly string _defaultQuestionFile = "questions.txt";
        private readonly string _defaultGameOverviewName = "GameOverview";
        private readonly string[] answerPrefixes = { "A ", "B ", "C ", "D ", "E ", "F ", "G ", "H " };
        // Prefixes
        private const string CurrentQuestionPrefix = "Current Question: ";

        public FileQuestionStorage(string questionDirectoryPath, string gameDirectoryPath)
        {
            _questionDirectoryPath = questionDirectoryPath;
            _gameDirectoryPath = gameDirectoryPath;
        }
        public async Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions)
        {
            var lines = await File.ReadAllLinesAsync(_questionDirectoryPath + $"{category}");
            Random rnd = new Random();
            var questions = ParseQuestions(lines).Where(x => x.Answers.Count <= 4)
                                                 .OrderBy(x => rnd.Next())
                                                 .ToList();
            SaveQuestions(questions, gameName, totalNumberOfQuestions);
            return true;
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

        private void SaveQuestions(List<Question> questions, string gameName, int numberOfQuestions)
        {

            var i = 0;
            try
            {
                using (var gameQuestionsFile = File.AppendText(_gameDirectoryPath + $"/{gameName}/{_defaultQuestionFile}"))
                {
                    while (i < numberOfQuestions)
                    {
                        i++;
                        var question = questions[i];
                        if (question.Answers.Count == 0 || question.Answers.Count > 4) // used when we cannot parse the question correctly
                            continue;
                        gameQuestionsFile.WriteLine();
                        gameQuestionsFile.WriteLine("#Q " + question.Body);
                        gameQuestionsFile.WriteLine("^ " + question.Answers.Where(x => x.isCorrect).FirstOrDefault().Body);
                        for (int j = 0; j < question.Answers.Count; j++)
                        {
                            Answer answer = question.Answers[j];
                            gameQuestionsFile.WriteLine(answerPrefixes[j] + question.Answers[j].Body);
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

        public async Task<Question> GetQuestion(string gameName, int? questionId = null)
        {
            int nextQuestionId = questionId ?? await GetCurrentQuestionId(gameName, true);
            var lines = await File.ReadAllLinesAsync(_gameDirectoryPath + $"/{gameName}/{_defaultQuestionFile}");
            var questions = ParseQuestions(lines);
            return questions[nextQuestionId];
        }

        public async Task<AnswerResponse> CheckAnswer(string gameName, string playerName, int answerId)
        {
            AnswerResponse data = new AnswerResponse();
            var lines = await File.ReadAllLinesAsync(_gameDirectoryPath + $"/{gameName}/{_defaultQuestionFile}");
            var questions = ParseQuestions(lines);
            var questionId = await GetCurrentQuestionId(gameName);
            data.isCorrect = questions[questionId].Answers[answerId].isCorrect;
            data.correctAnswerId = questions[questionId].Answers.FindIndex(x => x.isCorrect == true);
            return data;
        }

        private async Task<int> GetCurrentQuestionId(string gameName, bool isGetNextQuestion = false)
        {
            var path = _gameDirectoryPath + $"/{gameName}/{_defaultGameOverviewName}.txt";
            var playerData = await File.ReadAllLinesAsync(path);
            var currentQuestionId = int.Parse(playerData.LastOrDefault().Substring(CurrentQuestionPrefix.Length));
            if (isGetNextQuestion)
            {
                playerData[4] = CurrentQuestionPrefix + (currentQuestionId + 1).ToString();
                currentQuestionId += 1;
                await File.WriteAllLinesAsync(path, playerData);
            }
            return currentQuestionId;

        }

    }
}