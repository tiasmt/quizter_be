using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public class FileGameStorage : IGameStorage
    {
        
        private const string Category = "Category: ";
        private const string TotalNumberOfPlayers = "Total Number Of Players: ";
        private const string TotalNumberOfQuestions = "Total Number Of Questions: ";
        private const string TimePerQuestion = "Time Per Question: ";
        private readonly string _directoryPath;
        private readonly string _defaultFileName = "GameOverview";
        private readonly string _playersFolder = "Players";
        public FileGameStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        public async Task<string> CreateGame(string gameName)
        {
            try
            {
                // Determine whether the directory exists.
                var directory = _directoryPath + $"/{gameName}";
                if (Directory.Exists(directory))
                    throw new Exception();

                // Try to create the directory.
                await Task.Run(() =>
                {
                    DirectoryInfo di = Directory.CreateDirectory(directory);
                });

                return gameName;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<Game> GetGame(string name)
        {
            Game game = new Game();
            game.GameSettings = new Settings();
            game.GameName = name;

            var path = _directoryPath + name + '/' + _defaultFileName + ".txt";
            var gameData =  await File.ReadAllLinesAsync(path);
            //Parse data
            game.GameCategory = gameData[0].Substring(Category.Length);
            game.GameSettings.NumberOfPlayers = int.Parse(gameData[1].Substring(TotalNumberOfPlayers.Length));
            game.GameSettings.TotalNumberOfQuestions = int.Parse(gameData[2].Substring(TotalNumberOfQuestions.Length));
            game.GameSettings.TimePerQuestion = int.Parse(gameData[3].Substring(TimePerQuestion.Length));
            
            return game;
        }

        public Task<Game> GetGame(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Game>> GetAllGames()
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> SetCategory(string gameName, string category)
        {
            try
            {
                var path = _directoryPath + $"/{gameName}/{_defaultFileName}.txt";

                // Create the file, or overwrite if the file exists.
                FileStream fs = File.Create(path);
                await fs.DisposeAsync();

                return category;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task SetSettings(Game game, Settings settings)
        {
            string[] lines =
            {
                $"Category: {game.GameCategory}",
                $"Total Number Of Players: {settings.NumberOfPlayers}",
                $"Total Number Of Questions: {settings.TotalNumberOfQuestions}",
                $"Time Per Question: {settings.TimePerQuestion}"
            };
            var path = _directoryPath + $"/{game.GameName}/{_defaultFileName}.txt";
            await File.WriteAllLinesAsync(path, lines);

        }

        public async Task<int> CreatePlayer(Player player, string gameName)
        {
            var parentPath = _directoryPath + $"{gameName}/{_playersFolder}/";
            DirectoryInfo di = Directory.CreateDirectory(parentPath);
            var playerId = new List<string>(Directory.EnumerateFiles(parentPath)).Count + 1;
            string[] lines =
            {
                $"PlayerId: {playerId}",
                $"Username: {player.Username}",
                $"Avatar: {player.Avatar}",
                $"Correct Answers: {player.CorrectAnswers}",
                $"Wrong Answers: {player.WrongAnswers}",
                $"IsReady:  {true}"
            };

            var path = _directoryPath + $"/{gameName}/{_playersFolder}/{player.Username}.txt";
            await File.WriteAllLinesAsync(path, lines);
            return playerId;
        }

        public async Task SetPlayerReady(string gameName, string username, int questionId)
        {
            var path = _directoryPath + $"{gameName}/{_playersFolder}/{username}.txt";
            var playerData =  await File.ReadAllLinesAsync(path);
        }

        
    }
}