using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using quizter_be.Models;

namespace quizter_be.Repository
{
    public class FileGameStorage : IGameStorage
    {
        private readonly string _directoryPath;
        private readonly string _defaultFileName = "GameOverview.txt";
        private readonly string _playersFolder = "Players";

        //Game Overview Prefix
        private const string Category = "Category: ";
        private const string TotalNumberOfPlayersPrefix = "Total Number Of Players: ";
        private const string TotalNumberOfQuestionsPrefix = "Total Number Of Questions: ";
        private const string TimePerQuestionPrefix = "Time Per Question: ";
        private const string CurrentQuestionPrefix = "Current Question: ";

        //Player file Prefix
        private const string PlayerIdPrefix = "PlayerId: ";
        private const string UsernamePrefix = "Username: ";
        private const string AvatarPrefix = "Avatar: ";
        private const string CorrectAnswersPrefix = "Correct Answers: ";
        private const string WrongAnswersPrefix = "Wrong Answers: ";
        private const string IsReadyPrefix = "IsReady: ";

        public FileGameStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        public async Task<string> CreateGame(string gameName)
        {
            try
            {
                // Determine whether the directory exists.
                var directory = Path.Combine(_directoryPath, gameName);
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

            var path = Path.Combine(_directoryPath, name, _defaultFileName);
            var gameData = await File.ReadAllLinesAsync(path);
            //Parse data
            game.GameCategory = gameData[0].Substring(Category.Length);
            game.GameSettings.NumberOfPlayers = int.Parse(gameData[1].Substring(TotalNumberOfPlayersPrefix.Length));
            game.GameSettings.TotalNumberOfQuestions = int.Parse(gameData[2].Substring(TotalNumberOfQuestionsPrefix.Length));
            game.GameSettings.TimePerQuestion = int.Parse(gameData[3].Substring(TimePerQuestionPrefix.Length));

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
                var path = Path.Combine(_directoryPath,gameName, _defaultFileName);
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
                $"Time Per Question: {settings.TimePerQuestion}",
                $"Current Question: 0"
            };
            var path = Path.Combine(_directoryPath, game.GameName, _defaultFileName);
            await File.WriteAllLinesAsync(path, lines);

        }

        public async Task<int> CreatePlayer(Player player, string gameName)
        {
            var parentPath = Path.Combine(_directoryPath, gameName, _playersFolder);
            DirectoryInfo di = Directory.CreateDirectory(parentPath);
            var playerId = new List<string>(Directory.EnumerateFiles(parentPath)).Count + 1;
            string[] lines =
            {
                $"PlayerId: {playerId}",
                $"Username: {player.Username}",
                $"Avatar: {player.Avatar}",
                $"Correct Answers: {player.CorrectAnswers}",
                $"Wrong Answers: {player.WrongAnswers}",
                $"IsReady: {true}"
            };

            var path = Path.Combine(_directoryPath, gameName, _playersFolder, player.Username) + ".txt";
            await File.WriteAllLinesAsync(path, lines);
            return playerId;
        }

        public async Task SetPlayerReadyState(string gameName, string username, bool state)
        {
            var path = Path.Combine(_directoryPath ,gameName, _playersFolder, username) + ".txt";
            var playerData = await File.ReadAllLinesAsync(path);
            playerData[5] = IsReadyPrefix + state.ToString();
            await File.WriteAllLinesAsync(path, playerData);
        }

        public async Task<Player> SetPlayerScore(string gameName, string username, bool isCorrect)
        {
            var path = Path.Combine(_directoryPath, gameName, _playersFolder, username) + ".txt";
            var playerData = await File.ReadAllLinesAsync(path);
            if (isCorrect)
            {
                var correctAnswers = int.Parse(playerData[3].Substring(CorrectAnswersPrefix.Length)) + 1;
                playerData[3] = CorrectAnswersPrefix + correctAnswers.ToString();
            }
            else
            {
                var wrongAnswers = int.Parse(playerData[4].Substring(WrongAnswersPrefix.Length)) + 1;
                playerData[4] = WrongAnswersPrefix + wrongAnswers.ToString();
            }
            await File.WriteAllLinesAsync(path, playerData);
            var player = GetPlayerDetails(playerData);
            player.LastAnswerIsCorrect = isCorrect;
            return player;
        }

        public Player GetPlayerDetails(string[] playerData)
        {
            var player = new Player();
            player.PlayerId = int.Parse(playerData[0].Substring(PlayerIdPrefix.Length));
            player.Username = playerData[1].Substring(UsernamePrefix.Length);
            player.Avatar = playerData[2].Substring(AvatarPrefix.Length);
            player.CorrectAnswers = int.Parse(playerData[3].Substring(CorrectAnswersPrefix.Length));
            player.WrongAnswers = int.Parse(playerData[4].Substring(WrongAnswersPrefix.Length));
            return player;
        }

        public async Task<bool> AllPlayersReady(string gameName)
        {
            var path = Path.Combine(_directoryPath , gameName, _playersFolder);
            var allPlayers = Directory.EnumerateFiles(path);
            foreach (var player in allPlayers)
            {
                if (!(await IsPlayerReady(player)))
                    return false;
            }
            return true;
        }

        private async Task<bool> IsPlayerReady(string playerFile)
        {
            var playerData = await File.ReadAllLinesAsync(playerFile);
            return bool.Parse(playerData[5].Substring(IsReadyPrefix.Length));
        }

        public async Task ResetAllPlayersReadyState(string gameName)
        {
            var fileExtensionPattern = @"(?<=Players\\)(.*)(?=.txt)";
            var rg = new Regex(fileExtensionPattern);
            var path = Path.Combine(_directoryPath , gameName, _playersFolder);
            var allPlayers = Directory.EnumerateFiles(path);
            foreach (var player in allPlayers)
            {
                var username = rg.Matches(player)[0].ToString();
                await SetPlayerReadyState(gameName, username, false);
            }
        }

        public async Task<List<Player>> GetPlayers(string gameName)
        {
            var fileExtensionPattern = @"(?<=Players\\)(.*)(?=.txt)";
            var rg = new Regex(fileExtensionPattern);
            var playersData = new List<Player>();
            var path = Path.Combine(_directoryPath , gameName, _playersFolder);
            var allPlayers = Directory.EnumerateFiles(path);
            foreach (var player in allPlayers)
            {
                var username = rg.Matches(player)[0].ToString();
                var playerPath = Path.Combine(_directoryPath , gameName, _playersFolder, username) + ".txt";
                var playerData = await File.ReadAllLinesAsync(playerPath);
                playersData.Add(GetPlayerDetails(playerData));
            }

            return playersData;
        }


    }
}