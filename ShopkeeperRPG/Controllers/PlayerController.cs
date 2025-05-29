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
            new Item { Name = "Healing Potion", Description = "A simple potion that restores a few hit points.", Price = 25 },
            new Item { Name = "Iron Dagger", Description = "A basic, somewhat rusty dagger.", Price = 50 },
            new Item { Name = "Loaf of Bread", Description = "A hearty loaf of bread.", Price = 5 },
            new Item { Name = "Mystic Scroll", Description = "A scroll covered in glowing runes of unknown purpose.", Price = 150 } 
        };

        private static List<Item> _marketItems = new List<Item>();

        static PlayerController()
        {
            RefreshMarketItems();
        }

        private static void RefreshMarketItems()
        {
            _marketItems.Clear();
            if (_predefinedItems == null || !_predefinedItems.Any()) return;
            
            int numberOfItemsToOffer;
            if (_predefinedItems.Count == 0) numberOfItemsToOffer = 0;
            else if (_predefinedItems.Count == 1) numberOfItemsToOffer = 1;
            else 
            { 
                int maxPossible = Math.Min(4, _predefinedItems.Count); 
                numberOfItemsToOffer = _random.Next(2, maxPossible + 1);
            }

            if (numberOfItemsToOffer == 0) return;

            var shuffledPredefinedItems = _predefinedItems.OrderBy(item => _random.Next()).ToList();
            var itemsForMarket = shuffledPredefinedItems.Take(numberOfItemsToOffer);

            foreach (var selectedItem in itemsForMarket)
            {
                _marketItems.Add(new Item 
                { 
                    Name = selectedItem.Name, 
                    Description = selectedItem.Description, 
                    Price = selectedItem.Price 
                });
            }
        }

        // Request models
        public record CreatePlayerRequest(string Name);
        public record BuyItemRequest(string ItemName);
        // Response model for RunShopStandardRate
        public record RunShopStandardResponse(string Message, Player? UpdatedPlayer);


        [HttpPost("create")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Player name cannot be empty.");

            if (_players.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict($"Player with name '{request.Name}' already exists.");

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
            var player = _players.FirstOrDefault();
            if (player == null)
                return NotFound("No player created yet. Please create a player first.");

            if (!_predefinedItems.Any())
                return NotFound("No predefined items available to give.");

            var itemToAddDetails = _predefinedItems.First(); 
            player.Inventory.Add(new Item 
            { 
                Name = itemToAddDetails.Name, 
                Description = itemToAddDetails.Description, 
                Price = itemToAddDetails.Price 
            });
            return Ok(player); 
        }

        [HttpGet("marketitems")]
        public IActionResult GetMarketItems()
        {
            return Ok(_marketItems);
        }

        [HttpPost("buyitem")]
        public IActionResult BuyMarketItem([FromBody] BuyItemRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ItemName))
            {
                return BadRequest("Item name cannot be empty.");
            }

            var player = _players.FirstOrDefault();
            if (player == null)
            {
                return NotFound("Player not found. Please create a player first.");
            }

            var marketItem = _marketItems.FirstOrDefault(item => item.Name.Equals(request.ItemName, StringComparison.OrdinalIgnoreCase));
            if (marketItem == null)
            {
                return NotFound($"Item '{request.ItemName}' not found in market.");
            }

            if (player.Gold < marketItem.Price)
            {
                return BadRequest("Not enough gold to purchase this item.");
            }

            player.Gold -= marketItem.Price;
            player.Inventory.Add(new Item 
            { 
                Name = marketItem.Name, 
                Description = marketItem.Description, 
                Price = marketItem.Price 
            });

            return Ok(player);
        }

        [HttpPost("runshopstandard")]
        public IActionResult RunShopStandardRate()
        {
            var player = _players.FirstOrDefault();
            if (player == null)
            {
                // Using NotFound for consistency with other player-not-found scenarios
                return NotFound(new RunShopStandardResponse("Player not found. Please create a player first.", null));
            }

            if (player.Inventory == null || !player.Inventory.Any())
            {
                return Ok(new RunShopStandardResponse("Your inventory is empty! Nothing to sell.", player));
            }

            // Randomly pick an item from player.Inventory.
            // _random is the static Random instance from the class.
            int itemIndex = _random.Next(player.Inventory.Count);
            Item itemToSell = player.Inventory[itemIndex];

            int salePrice = itemToSell.Price; // For now, sale price is item's base price

            player.Gold += salePrice;
            player.Inventory.RemoveAt(itemIndex); // Remove by index is safest

            string message = $"Sold {itemToSell.Name} for {salePrice} gold.";
            return Ok(new RunShopStandardResponse(message, player));
        }
    }
}
