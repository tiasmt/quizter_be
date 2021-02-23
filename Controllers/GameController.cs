using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using quizter_be.Hubs;
using quizter_be.Models;
using quizter_be.Repository;
using quizter_be.Services;

namespace quizter_be.Controllers
{
    [ApiController]
    [Route("rest/api/latest/game")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private IGameService _gameService;
        private readonly IQuestionService _questionService;
        private readonly IHubContext<GameHub, IGameHub> _hubContext;

        public GameController(IGameService gameService, IQuestionService questionService, ILogger<GameController> logger, IHubContext<GameHub, IGameHub> hubContext)
        {
            _gameService = gameService;
            _questionService = questionService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet("CreateGame")]
        public async Task<string> CreateGame()
        {
            return await _gameService.CreateGame();
        }

        [HttpPost("SetCategory")]
        public async Task<IActionResult> SetCategory(string gameName, string gameCategory)
        {
            await _gameService.SetCategory(gameName, gameCategory);
            return Ok(gameCategory);
        }

        [HttpPost("SetSettings")]
        public async Task<IActionResult> SetSettings(string gameName, string gameCategory, [FromBody] Settings settings)
        {
            var game = new Game { GameName = gameName, GameCategory = gameCategory };
            await _gameService.SetSettings(game, settings);
            await _questionService.CreateQuestions(gameName, gameCategory, settings.TotalNumberOfQuestions);
            return Ok();
        }

        [HttpPost("CreatePlayer")]
        public async Task<IActionResult> CreatePlayer(string username, string avatar, string gameName)
        {
            username??= "TestPlayer";
            var gamehub = new GameHub(_hubContext, _gameService, _questionService);
            await gamehub.CreateTimers(gameName);
            var player = new Player { Username = username, Avatar = avatar };
            var id = await _gameService.CreatePlayer(player, gameName);
            return Ok(id);
        }

        [HttpPost("StartGame")]
        public async Task<IActionResult> StartGame(string gameName)
        {
            var gamehub = new GameHub(_hubContext, _gameService, _questionService);
            var question = await _questionService.NextQuestion(gameName, 0);
            await _hubContext.Clients.All.SendQuestion(question); 
            gamehub.StartGameTimer();
            return Ok();
        }

        [HttpPost("CheckAnswer")]
        public async Task<IActionResult> CheckAnswer(string gameName, string username, int answerId)
        {
            var isCorrect = await _questionService.CheckAnswer(gameName, username, answerId);
            // await _gameService.SetPlayerReady(gameName, username);          
            return Ok(isCorrect);
        }
    }
}
