using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public class FileGameStorage : IGameStorage
    {
        private readonly string _directoryPath;
        public FileGameStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        public async Task<Game> CreateGame(string gameName)
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
                
                return new Game{Name = gameName};
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
    }
}