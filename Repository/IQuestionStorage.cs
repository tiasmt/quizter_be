using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public interface IQuestionStorage
    {
        Task<IEnumerable<Question>> GetAllQuestions(string category);
        Task<bool> CreateQuestions(string gameName, string category, int totalNumberOfQuestions);
        Task<Question> GetQuestion(string gameName, int? questionId = null);
        Task<bool> CheckAnswer(string gameName, string playerName, int questionId, int answerId);
    }
}