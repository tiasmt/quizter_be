using System.Threading.Tasks;

namespace quizter_be.Hubs
{

    public interface IGameHub 
    {
        Task StartTimer();
        Task HeartBeat(string message);
        Task Beat(int timeRemaining);
        Task GetQuestion();
        Task NextQuestion(string message);
    }
}