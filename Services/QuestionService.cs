
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
        public async Task<bool> CheckAnswer(string gameName, string playerName, int questionId, int answerId)
        {
            return await _storage.CheckAnswer(gameName, playerName, questionId, answerId);
        }

        public async Task<Question> NextQuestion(string gameName, int? questionId = null)
        {
            return await _storage.GetQuestion(gameName, questionId);
        }
    }
}