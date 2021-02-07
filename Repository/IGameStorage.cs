using System.Collections.Generic;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public interface IGameStorage
    {
        Task<IEnumerable<Game>> GetAllGames();
        Task<Game> GetGame(string name);
        Task<Game> GetGame(int id);
        Task<Game> CreateGame(string gameName);
    }
}