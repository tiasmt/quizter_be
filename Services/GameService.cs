
using System.Collections.Generic;
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

        public async Task<int> CreatePlayer(Player player, string gameName)
        {
            return await _storage.CreatePlayer(player, gameName);
        }

        public async Task<Game> GetGame(string gameName)
        {
            return await _storage.GetGame(gameName);
        }

        public async Task SetPlayerReadyState(string gameName, string username, bool state)
        {
            await _storage.SetPlayerReadyState(gameName, username, state);
        }

        public async Task<Player> SetPlayerScore(string gameName, string username, bool isCorrect)
        {
            return await _storage.SetPlayerScore(gameName, username, isCorrect);
        }

        public async Task<bool> AllPlayersReady(string gameName)
        {
            return await _storage.AllPlayersReady(gameName);
        }

        public async Task ResetAllPlayersReadyState(string gameName)
        {
            await _storage.ResetAllPlayersReadyState(gameName);
        }

        public async Task<List<Player>> GetPlayers(string gameName)
        {
            return await _storage.GetPlayers(gameName);
        }
    }
}