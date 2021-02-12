
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

        public async Task<string> CreateGame()
        {
            var gameName = Utils.RandomString();
            return await _storage.CreateGame(gameName);
        }

        public async Task<string> SetCategory(string gameName,string category)
        {
            return await _storage.SetCategory(gameName, category);
        }

        public async Task SetSettings(Game game, Settings settings)
        {
            await _storage.SetSettings(game, settings);
        }

    }
}