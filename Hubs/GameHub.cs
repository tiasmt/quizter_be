using Microsoft.AspNetCore.SignalR;
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
        private static Timer _timer;
        public GameHub(IHubContext<GameHub, IGameHub> hubContext, IGameService gameService, IQuestionService questionService)
        {
            _hubContext = hubContext;
            _gameService = gameService;
            _questionService = questionService;
        }

        public async Task CreateTimer(string gameName)
        {
            _timer ??= new Timer(1000);
            _timer.Enabled = false;
            _timeLeft = await GetInitialTime(gameName);
            _initialTime = _timeLeft;
            _timer.Elapsed += (sender, e) => HeartBeat(sender, e);
            _timer.Interval = 1000;
        }

        public void StopTimer()
        {
            _timer.Enabled = false;
        }

        public void StartTimer()
        {
            _timer.Enabled = true;
        }

        public async void HeartBeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timeLeft--;
            await _hubContext.Clients.All.Beat(_timeLeft);
            if (_timeLeft <= 0)
            {
                _timeLeft = _initialTime;
                StopTimer();
                await CheckAnswer();
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


    }
}