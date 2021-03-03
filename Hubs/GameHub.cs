using Microsoft.AspNetCore.SignalR;
using quizter_be.Models;
using quizter_be.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;

namespace quizter_be.Hubs
{
    public class GameHub : Hub<IGameHub>
    {
        private readonly IHubContext<GameHub, IGameHub> _hubContext;
        private readonly IGameService _gameService;
        private readonly IQuestionService _questionService;
        private static ConcurrentDictionary<string, Timer> _gameTimers = new ConcurrentDictionary<string, Timer>();
        private static ConcurrentDictionary<string, Timer> _questionTimers = new ConcurrentDictionary<string, Timer>();
        private static ConcurrentDictionary<string, int> _timeLeft = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> _initialTime = new ConcurrentDictionary<string, int>();
        
        public GameHub(IHubContext<GameHub, IGameHub> hubContext, IGameService gameService, IQuestionService questionService)
        {
            _hubContext = hubContext;
            _gameService = gameService;
            _questionService = questionService;
        }

        public void JoinGroup(string gameName)
        {
            _hubContext.Groups.AddToGroupAsync(Context.ConnectionId, gameName); 
        }

        public async Task CreateTimers(string gameName)
        {
            await CreateGameTimer(gameName);
            CreateQuestionTimer(gameName);
        }

        private async Task CreateGameTimer(string gameName)
        {
            var currentTimer = _gameTimers.GetOrAdd(gameName, new Timer(1000));
            currentTimer.Enabled = false;
            _timeLeft.GetOrAdd(gameName, await GetInitialTime(gameName));
            _initialTime.GetOrAdd(gameName, _timeLeft[gameName]);
            currentTimer.Elapsed += (sender, e) => HeartBeat(sender, e, gameName);
            currentTimer.Interval = 1000;
        }
        private void CreateQuestionTimer(string gameName)
        {
            var currentTimer = _questionTimers.GetOrAdd(gameName, new Timer(2000));
            currentTimer.Enabled = false;
            currentTimer.Elapsed += (sender, e) => NextQuestion(sender, e, gameName);
        }

        private async void NextQuestion(object sender, System.Timers.ElapsedEventArgs e, string gameName)
        {
            if (await _gameService.AllPlayersReady(gameName))
            {
                var question = await _questionService.NextQuestion(gameName);
                await SendQuestion(gameName, question);

                if (_gameTimers.TryGetValue(gameName, out Timer gameTimer))
                {
                    StartTimer(gameTimer);
                }

                var players = await _gameService.GetPlayers(gameName);
                await _hubContext.Clients.Groups(gameName).SendLeaderboard(players);
            }
        }

        public void StopTimer(Timer timer)
        {
            timer.Enabled = false;
        }

        public void StartTimer(Timer timer)
        {
            timer.Enabled = true;
        }

        public void StartGameTimer(string gameName)
        {
            if (_gameTimers.TryGetValue(gameName, out Timer timer))
                StartTimer(timer);
        }

        public async void HeartBeat(object sender, System.Timers.ElapsedEventArgs e, string gameName)
        {
            _timeLeft.TryGetValue(gameName, out var timeLeft);
            timeLeft--;
            _timeLeft[gameName] = timeLeft;
            await _hubContext.Clients.Groups(gameName).Beat(timeLeft);
            if (timeLeft <= 0)
            {
                _timeLeft[gameName] = _initialTime[gameName];
                await _gameService.ResetAllPlayersReadyState(gameName);

                if (_gameTimers.TryGetValue(gameName, out Timer gameTimer))
                {
                    StopTimer(gameTimer);
                }

                await CheckAnswer(gameName);
                if (_questionTimers.TryGetValue(gameName, out Timer questionTimer))
                {
                    StartTimer(questionTimer);
                }
            }
        }

        private async Task<int> GetInitialTime(string gameName)
        {
            var game = await _gameService.GetGame(gameName);
            return game.GameSettings.TimePerQuestion + 1;
        }

        private async Task CheckAnswer(string gameName)
        {
            await _hubContext.Clients.Groups(gameName).CheckAnswer("CheckAnswer");
        }

        private async Task SendQuestion(string gameName, Question question)
        {
            await _hubContext.Clients.Groups(gameName).SendQuestion(question);
            if (_questionTimers.TryGetValue(gameName, out Timer questionTimer))
            {
                StopTimer(questionTimer);
            }
        }


    }
}