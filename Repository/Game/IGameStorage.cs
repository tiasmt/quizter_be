using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public interface IGameStorage
    {
        Task<IEnumerable<Game>> GetAllGames();
        Task<Game> GetGame(string gameName);
        Task<Game> GetGame(int id);
        Task<string> CreateGame(string gameName);
        Task<string> SetCategory(string gameName, string category);
        Task SetSettings(Game game, Settings settings);
        Task<int> CreatePlayer(Player player, string gameName);

        Task SetPlayerReadyState(string gameName, string username, bool state);
        Task<Player> SetPlayerScore(string gameName, string username, bool isCorrect);
        Task<bool> AllPlayersReady(string gameName);
        Task ResetAllPlayersReadyState(string gameName);
        Task<List<Player>> GetPlayers(string gameName);
        Task CleanUp();

        Task<bool> GameEnded(string gameName);
    }
}