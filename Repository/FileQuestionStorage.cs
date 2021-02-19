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
        private readonly string[] answerPrefixes = { "A ", "B ", "C ", "D ", "E ", "F ", "G ", "H " };

        public FileQuestionStorage(string questionDirectoryPath, string gameDirectoryPath)
        {
            _questionDirectoryPath = questionDirectoryPath;
            _gameDirectoryPath = gameDirectoryPath;
        }
        public async Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions)
        {
            var lines = await File.ReadAllLinesAsync(_questionDirectoryPath + $"{category}");
            Random rnd = new Random();
            var questions = ParseQuestions(lines).OrderBy(x => rnd.Next()).ToList();
            SaveQuestions(questions, gameName, totalNumberOfQuestions);
            return true;
        }

        private List<Question> ParseQuestions(string[] lines)
        {
            var correctAnswer = string.Empty;
            var currentQuestion = new Question();
            var questions = new List<Question>();

            foreach (var line in lines)
            {

                if (line == string.Empty)
                {
                    if (currentQuestion.Answers != null)
                        questions.Add(currentQuestion);

                    currentQuestion = new Question();
                    currentQuestion.Answers = new List<Answer>();
                }


                else if (line.StartsWith("#Q"))
                    currentQuestion.Body = line.Substring(2);

                else if (line.StartsWith("^"))
                    correctAnswer = line.Substring(2);

                else if (line.StartsWith("A")
                        || line.StartsWith("B")
                        || line.StartsWith("C")
                        || line.StartsWith("D")
                        || line.StartsWith("E")
                        || line.StartsWith("F")
                        || line.StartsWith("G")
                        || line.StartsWith("H"))
                {
                    if (line.Contains(correctAnswer))
                        currentQuestion.Answers.Add(new Answer { Body = line.Substring(2), isCorrect = true });
                    else
                        currentQuestion.Answers.Add(new Answer { Body = line.Substring(2), isCorrect = false });
                }
                else
                {
                    currentQuestion.Body += " " + line;
                }

            }
            return questions;
        }

        private void SaveQuestions(List<Question> questions, string gameName, int numberOfQuestions)
        {

            try
            {
                using (var gameQuestionsFile = File.AppendText(_gameDirectoryPath + $"/{gameName}/{_defaultQuestionFile}"))
                {
                    for (int i = 0; i < numberOfQuestions; i++)
                    {
                        var question = questions[i];
                        if (question.Answers.Count == 0) // used when we cannot parse the question correctly
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
                System.Console.WriteLine(ex);
            }


        }
        public Task<IEnumerable<Question>> GetAllQuestions(string category)
        {
            throw new NotImplementedException();
        }

        public Task<Question> GetQuestion(int questionId)
        {
            throw new NotImplementedException();
        }
    }
}