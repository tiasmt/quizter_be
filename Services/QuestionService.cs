
using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;
using quizter_be.Repository;

namespace quizter_be.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionStorage _storage;

        public QuestionService(IQuestionStorage storage)
        {
            _storage = storage;
        }
        public async Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions)
        {
            return await _storage.CreateQuestions(gameName, category, totalNumberOfQuestions);
        }
        public Task<bool> CheckAnswer(string gameName, string playerName, int questionId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<string>> NextQuestion(int questionId)
        {
            throw new System.NotImplementedException();
        }
    }
}