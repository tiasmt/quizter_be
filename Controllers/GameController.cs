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
        public Task<Game> CreateGame()
        {
            return _gameService.CreateGame();
        }

    }
}
