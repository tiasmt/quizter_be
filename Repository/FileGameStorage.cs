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
        private readonly string _directoryPath;
        private readonly string _defaultFileName = "GameOverview";
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

        public Task<Game> GetGame(string name)
        {
            throw new System.NotImplementedException();
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
    }
}