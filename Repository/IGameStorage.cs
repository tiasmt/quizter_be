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
        Task SetSettings(Game gamee, Settings settings);
    }
}