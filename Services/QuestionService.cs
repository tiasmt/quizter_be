
using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Services
{
    public class QuestionService : IQuestionService
    {
        public Task CreateQuestions(string category)
        {
            throw new System.NotImplementedException();
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