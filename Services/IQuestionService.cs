
using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Services
{
    public interface IQuestionService
    {
        Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions);
        Task<bool> CheckAnswer(string gameName, string playerName, int questionId);
        Task<List<string>> NextQuestion(int questionId);
    }
}