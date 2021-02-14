using Microsoft.AspNetCore.SignalR;
using quizter_be.Services;
using System.Threading.Tasks;

namespace quizter_be.Hubs
{
    public class GameHub : Hub<IGameHub>
    {
        private readonly IHubContext<GameHub, IGameHub> _hubContext;
        private readonly IGameService _service;
        private int _timeLeft;
        private int _initialTime;
        public GameHub(IHubContext<GameHub, IGameHub> hubContext, IGameService service)
        {
            _hubContext = hubContext;
            _service = service;
        }

        public async Task StartTimer(string gameName)
        {
            var timer = new System.Timers.Timer(1000);
            _timeLeft = await GetInitialTime(gameName);
            _initialTime = _timeLeft;
            timer.Elapsed +=(sender, e) => HeartBeat(sender, e);
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        public async void HeartBeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timeLeft --;
            await _hubContext.Clients.All.Beat(_timeLeft);
            if(_timeLeft <= 0)
                _timeLeft = _initialTime;
        }

        public async Task GetQuestion()
        {
            await Clients.All.NextQuestion("next");
        }

        private async Task<int> GetInitialTime(string gameName)
        {
            var game = await _service.GetGame(gameName);
            return game.GameSettings.TimePerQuestion;
        }


    }
}