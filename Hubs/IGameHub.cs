using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Hubs
{

    public interface IGameHub 
    {
        Task StartTimer();
        Task HeartBeat(string message);
        Task Beat(int timeRemaining);
        Task GetQuestion();
        Task SendQuestion(Question question);
        Task CheckAnswer(string message);
    }
}