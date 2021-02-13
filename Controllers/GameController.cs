using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using quizter_be.Models;
using quizter_be.Services;

namespace quizter_be.Controllers
{
    [ApiController]
    [Route("rest/api/latest/game")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private IGameService _gameService;

        public GameController(IGameService gameService,ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpGet("creategame")]
        public async Task<string> CreateGame()
        {
            return await _gameService.CreateGame();
        }

        [HttpPost("setcategory")]
        public async Task<string> SetCategory(string gameName, string gameCategory)
        {
            return await _gameService.SetCategory(gameName, gameCategory);
        }

        [HttpPost("setsettings")]
        public async Task<IActionResult> SetSettings(string gameName,string gameCategory, [FromBody] Settings settings)
        {
            var game = new Game{GameName=gameName, GameCategory=gameCategory};
            await _gameService.SetSettings(game, settings);
            return Ok();
        }
        [HttpPost("createplayer")]
        public async Task<IActionResult> CreatePlayer(string username,string avatar, string gameName)
        {
            var player = new Player{Username=username, Avatar=avatar };
            var id = await _gameService.CreatePlayer(player, gameName);
            return Ok(id);
        }
    }
}
