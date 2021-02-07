
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Services
{
    public interface IGameService
    {
        Task<Game> CreateGame();
    }
}