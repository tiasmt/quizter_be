﻿using System.Threading.Tasks;
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
        private readonly IHubContext<GameHub, IGameHub> _hubContext;

        public GameController(IGameService gameService, ILogger<GameController> logger, IHubContext<GameHub, IGameHub> hubContext)
        {
            _gameService = gameService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet("CreateGame")]
        public async Task<string> CreateGame()
        {
            return await _gameService.CreateGame();
        }

        [HttpPost("SetCategory")]
        public async Task<string> SetCategory(string gameName, string gameCategory)
        {
            return await _gameService.SetCategory(gameName, gameCategory);
        }

        [HttpPost("SetSettings")]
        public async Task<IActionResult> SetSettings(string gameName, string gameCategory, [FromBody] Settings settings)
        {
            var game = new Game { GameName = gameName, GameCategory = gameCategory };
            await _gameService.SetSettings(game, settings);
            return Ok();
        }
        [HttpPost("CreatePlayer")]
        public async Task<IActionResult> CreatePlayer(string username, string avatar, string gameName)
        {
            var player = new Player { Username = username, Avatar = avatar };
            var id = await _gameService.CreatePlayer(player, gameName);
            var gamehub = new GameHub(_hubContext, _gameService);
            await gamehub.CreateTimer(gameName);
            return Ok(id);
        }

       
    }
}
