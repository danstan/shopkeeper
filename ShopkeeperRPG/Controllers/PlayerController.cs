using Microsoft.AspNetCore.Mvc;
using ShopkeeperRPG.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopkeeperRPG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private static List<Player> _players = new List<Player>();
        private static Random _random = new Random();

        private static readonly List<Item> _predefinedItems = new List<Item>
        {
            new Item { Name = "Healing Potion", Description = "A simple potion that restores a few hit points." },
            new Item { Name = "Iron Dagger", Description = "A basic, somewhat rusty dagger." },
            new Item { Name = "Loaf of Bread", Description = "A hearty loaf of bread." },
            new Item { Name = "Mystic Scroll", Description = "A scroll covered in glowing runes of unknown purpose." } 
        };

        // Request model for creating a player
        public record CreatePlayerRequest(string Name);

        [HttpPost("create")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Player name cannot be empty.");
            }

            if (_players.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict($"Player with name '{request.Name}' already exists.");
            }

            var newPlayer = new Player
            {
                Name = request.Name,
                Strength = _random.Next(3, 19),
                Dexterity = _random.Next(3, 19),
                Constitution = _random.Next(3, 19),
                Intelligence = _random.Next(3, 19),
                Wisdom = _random.Next(3, 19),
                Charisma = _random.Next(3, 19),
                Gold = 50
            };

            _players.Add(newPlayer);
            return Ok(newPlayer);
        }

        [HttpPost("getstarteritem")]
        public IActionResult GetStarterItem()
        {
            if (!_players.Any()) // Or _players.FirstOrDefault() == null
            {
                return NotFound("No player created yet. Please create a player first.");
            }

            var player = _players.First(); // Get the first player

            if (!_predefinedItems.Any())
            {
                return NotFound("No predefined items available to give.");
            }

            var itemToAdd = _predefinedItems.First(); // Select the first predefined item

            // Player.Inventory is guaranteed to be initialized by its constructor.
            // If it weren't, this would be a good defensive check:
            // player.Inventory ??= new List<Item>();

            // Add a new instance/copy of the item to the player's inventory
            player.Inventory.Add(new Item { Name = itemToAdd.Name, Description = itemToAdd.Description });

            return Ok(player); // Return the updated player object
        }
    }
}
