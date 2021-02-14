using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public interface IQuestionStorage
    {
        Task<IEnumerable<Question>> GetAllQuestions(string category);
        Task<bool> CreateQuestions(string gameName, string category);
        Task<Question> GetQuestion(int questionId);
    }
}