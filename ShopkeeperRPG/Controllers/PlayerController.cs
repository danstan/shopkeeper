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

        // Method to advance player time
        private static void AdvancePlayerTime(Player player, int hoursToAdvance)
        {
            if (player == null)
            {
                return;
            }

            player.CurrentHour += hoursToAdvance;

            if (player.CurrentHour >= 24)
            {
                int daysPassed = player.CurrentHour / 24;
                player.CurrentDay += daysPassed;
                player.CurrentHour %= 24; 

                // End of Day Logic
                for (int i = 0; i < daysPassed; i++)
                {
                    if (!player.TookLongRestToday)
                    {
                        player.ExhaustionLevel = Math.Min(player.ExhaustionLevel + 1, 6); // Cap at 6
                    }
                    player.TookLongRestToday = false; // Reset for the new day that just started
                }
                
                RefreshMarketItems(); // Market refreshes daily
                // Future: Other daily events, NPC schedules, etc.
            }
        }

        // Request models
        public record CreatePlayerRequest(string Name);
        public record BuyItemRequest(string ItemName);
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

            AdvancePlayerTime(player, 0); 
            return Ok(player); 
        }

        [HttpGet("marketitems")]
        public IActionResult GetMarketItems()
        {
            var player = _players.FirstOrDefault();
            if (player != null)
            {
                AdvancePlayerTime(player, 1); 
            }
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
            
            AdvancePlayerTime(player, 0); 
            return Ok(player);
        }

        [HttpPost("runshopstandard")]
        public IActionResult RunShopStandardRate()
        {
            var player = _players.FirstOrDefault();
            if (player == null)
            {
                return NotFound(new RunShopStandardResponse("Player not found. Please create a player first.", null));
            }

            if (player.Inventory == null || !player.Inventory.Any())
            {
                AdvancePlayerTime(player, 2); 
                return Ok(new RunShopStandardResponse("Your inventory is empty! Nothing to sell.", player));
            }

            int itemIndex = _random.Next(player.Inventory.Count);
            Item itemToSell = player.Inventory[itemIndex];
            int salePrice = itemToSell.Price; 

            player.Gold += salePrice;
            player.Inventory.RemoveAt(itemIndex); 

            AdvancePlayerTime(player, 2); 
            string message = $"Sold {itemToSell.Name} for {salePrice} gold.";
            return Ok(new RunShopStandardResponse(message, player));
        }

        [HttpPost("longrest")]
        public IActionResult TakeLongRest()
        {
            var player = _players.FirstOrDefault();
            if (player == null)
            {
                return NotFound("Player not found. Please create a player first."); // Consistent with other player checks
            }

            // For this version, we assume the player can always attempt a long rest.
            // No food/drink check implemented.

            player.TookLongRestToday = true;
            AdvancePlayerTime(player, 8); // Advance time by 8 hours
            player.ExhaustionLevel = 0; // Reset exhaustion

            // TODO: Implement HP and Hit Dice recovery as per D&D long rest rules.

            return Ok(player);
        }
    }
}
