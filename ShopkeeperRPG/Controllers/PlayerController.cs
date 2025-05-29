using Microsoft.AspNetCore.Mvc;
using ShopkeeperRPG.Models;
using System;
using System.Collections.Generic;
using System.Linq; // Although not strictly needed for current implementation, good to have for future list operations

namespace ShopkeeperRPG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private static List<Player> _players = new List<Player>();
        private static Random _random = new Random();

        // Request model for creating a player
        public record CreatePlayerRequest(string Name);

        [HttpPost("create")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Player name cannot be empty.");
            }

            // Optional: Check if player name already exists
            if (_players.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict($"Player with name '{request.Name}' already exists.");
            }

            var newPlayer = new Player
            {
                Name = request.Name,
                Strength = _random.Next(3, 19), // Next(min, max) max is exclusive, so 19 for 3-18
                Dexterity = _random.Next(3, 19),
                Constitution = _random.Next(3, 19),
                Intelligence = _random.Next(3, 19),
                Wisdom = _random.Next(3, 19),
                Charisma = _random.Next(3, 19),
                Gold = 50
                // Inventory is initialized by the Player constructor
            };

            _players.Add(newPlayer);

            return Ok(newPlayer);
        }
    }
}
