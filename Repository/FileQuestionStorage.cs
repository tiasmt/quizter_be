using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public class FileQuestionStorage : IQuestionStorage
    {
        private readonly string _directoryPath;

        public FileQuestionStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        public Task<bool> CreateQuestions(string gameName, string category)
        {
            throw new NotImplementedException();
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