
using System.Threading.Tasks;
using quizter_be.Models;
using quizter_be.Repository;
using quizter_be.Utilities;

namespace quizter_be.Services
{
    public class GameService : IGameService
    {
        private readonly IGameStorage _storage;

        public GameService(IGameStorage storage)
        {
            _storage = storage;
        }

        public Task<Game> CreateGame()
        {
            var gameName = Utils.RandomString();
            return _storage.CreateGame(gameName);
        }
    }
}