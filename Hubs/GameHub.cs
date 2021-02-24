using Microsoft.AspNetCore.SignalR;
using quizter_be.Models;
using quizter_be.Services;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace quizter_be.Hubs
{
    public class GameHub : Hub<IGameHub>
    {
        private readonly IHubContext<GameHub, IGameHub> _hubContext;
        private readonly IGameService _gameService;
        private readonly IQuestionService _questionService;
        private static int _timeLeft;
        private static int _initialTime;
        private static Timer _gameTimer;
        private static Timer _questionTimer;
        public GameHub(IHubContext<GameHub, IGameHub> hubContext, IGameService gameService, IQuestionService questionService)
        {
            _hubContext = hubContext;
            _gameService = gameService;
            _questionService = questionService;
        }

        public async Task CreateTimers(string gameName)
        {
            await CreateGameTimer(gameName);
            CreateQuestionTimer(gameName);
        }

        private async Task CreateGameTimer(string gameName)
        {
            _gameTimer ??= new Timer(1000);
            _gameTimer.Enabled = false;
            _timeLeft = await GetInitialTime(gameName);
            _initialTime = _timeLeft;
            _gameTimer.Elapsed += (sender, e) => HeartBeat(sender, e, gameName);
            _gameTimer.Interval = 1000;
        }

        private void CreateQuestionTimer(string gameName)
        {
            _questionTimer ??= new Timer(2000);
            _questionTimer.Enabled = false;
            _questionTimer.Elapsed += (sender, e) => NextQuestion(sender, e, gameName);
        }

        private async void NextQuestion(object sender, System.Timers.ElapsedEventArgs e, string gameName)
        {
            if (await _gameService.AllPlayersReady(gameName))
            {
                var question = await _questionService.NextQuestion(gameName);
                await SendQuestion(question);
                StartTimer(_gameTimer);
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

        public void StartGameTimer()
        {
            _gameTimer.Enabled = true;
        }

        public async void HeartBeat(object sender, System.Timers.ElapsedEventArgs e, string gameName)
        {
            _timeLeft--;
            await _hubContext.Clients.All.Beat(_timeLeft);
            if (_timeLeft <= 0)
            {
                _timeLeft = _initialTime;
                await _gameService.ResetAllPlayersReadyState(gameName);
                StopTimer(_gameTimer);
                await CheckAnswer();
                StartTimer(_questionTimer);
            }
        }

        private async Task<int> GetInitialTime(string gameName)
        {
            var game = await _gameService.GetGame(gameName);
            return game.GameSettings.TimePerQuestion + 1;
        }

        private async Task CheckAnswer()
        {
            await _hubContext.Clients.All.CheckAnswer("CheckAnswer");
        }

        private async Task SendQuestion(Question question)
        {
            await _hubContext.Clients.All.SendQuestion(question);
            StopTimer(_questionTimer);
        }


    }
}