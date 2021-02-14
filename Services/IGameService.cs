
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Services
{
    public interface IGameService
    {
        Task<string> CreateGame();
        Task<string> SetCategory(string gameName, string category);
        Task SetSettings(Game game, Settings settings);
        Task<int> CreatePlayer(Player player, string gameName);
        Task<Game> GetGame(string gameName);
    }
}