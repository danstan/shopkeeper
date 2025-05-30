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
        private static BarterSession? _activeBarterSession; 

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

        private static void AdvancePlayerTime(Player player, int hoursToAdvance)
        {
            if (player == null) return;
            player.CurrentHour += hoursToAdvance;
            if (player.CurrentHour >= 24)
            {
                int daysPassed = player.CurrentHour / 24;
                player.CurrentDay += daysPassed;
                player.CurrentHour %= 24; 
                for (int i = 0; i < daysPassed; i++)
                {
                    if (!player.TookLongRestToday)
                    {
                        player.ExhaustionLevel = Math.Min(player.ExhaustionLevel + 1, 6); 
                    }
                    player.TookLongRestToday = false; 
                }
                RefreshMarketItems(); 
            }
        }

        // Request/Response models
        public record CreatePlayerRequest(string Name);
        public record BuyItemRequest(string ItemName);
        public record RunShopStandardResponse(string Message, Player? UpdatedPlayer);
        public record InitiateBarterResponse(string? ItemName, string? ItemDescription, int NpcInitialOffer, string Message);
        public record BarterResponseRequest(string Action, int? CounterAmount);
        public record BarterActionResponse(string Message, Player? UpdatedPlayer, bool BarterEnded, string? NpcResponseAction, int? NewNpcOffer);


        [HttpPost("create")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Player name cannot be empty.");
            if (_players.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict($"Player with name '{request.Name}' already exists.");
            var newPlayer = new Player { Name = request.Name, Strength = _random.Next(3, 19), Dexterity = _random.Next(3, 19), Constitution = _random.Next(3, 19), Intelligence = _random.Next(3, 19), Wisdom = _random.Next(3, 19), Charisma = _random.Next(3, 19), Gold = 50 };
            _players.Add(newPlayer);
            return Ok(newPlayer);
        }

        [HttpPost("getstarteritem")]
        public IActionResult GetStarterItem()
        {
            var player = _players.FirstOrDefault();
            if (player == null) return NotFound("No player created yet. Please create a player first.");
            if (!_predefinedItems.Any()) return NotFound("No predefined items available to give.");
            var itemToAddDetails = _predefinedItems.First(); 
            player.Inventory.Add(new Item { Name = itemToAddDetails.Name, Description = itemToAddDetails.Description, Price = itemToAddDetails.Price });
            AdvancePlayerTime(player, 0); 
            return Ok(player); 
        }

        [HttpGet("marketitems")]
        public IActionResult GetMarketItems()
        {
            var player = _players.FirstOrDefault();
            if (player != null) AdvancePlayerTime(player, 1); 
            return Ok(_marketItems);
        }

        [HttpPost("buyitem")]
        public IActionResult BuyMarketItem([FromBody] BuyItemRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ItemName)) return BadRequest("Item name cannot be empty.");
            var player = _players.FirstOrDefault();
            if (player == null) return NotFound("Player not found. Please create a player first.");
            var marketItem = _marketItems.FirstOrDefault(item => item.Name.Equals(request.ItemName, StringComparison.OrdinalIgnoreCase));
            if (marketItem == null) return NotFound($"Item '{request.ItemName}' not found in market.");
            if (player.Gold < marketItem.Price) return BadRequest("Not enough gold to purchase this item.");
            player.Gold -= marketItem.Price;
            player.Inventory.Add(new Item { Name = marketItem.Name, Description = marketItem.Description, Price = marketItem.Price });
            AdvancePlayerTime(player, 0); 
            return Ok(player);
        }

        [HttpPost("runshopstandard")]
        public IActionResult RunShopStandardRate()
        {
            var player = _players.FirstOrDefault();
            if (player == null) return NotFound(new RunShopStandardResponse("Player not found. Please create a player first.", null));
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
            if (player == null) return NotFound("Player not found. Please create a player first.");
            player.TookLongRestToday = true;
            AdvancePlayerTime(player, 8); 
            player.ExhaustionLevel = 0; 
            // TODO: Implement HP and Hit Dice recovery
            return Ok(player);
        }

        [HttpPost("barter/initiate")]
        public IActionResult InitiateBarter()
        {
            var player = _players.FirstOrDefault();
            if (player == null)
                return BadRequest(new InitiateBarterResponse(null, null, 0, "No player available."));
            if (_activeBarterSession != null)
                return Conflict(new InitiateBarterResponse(null, null, 0, "A barter session is already active. Please resolve it first."));
            if (player.Inventory == null || !player.Inventory.Any())
            {
                _activeBarterSession = null; 
                return Ok(new InitiateBarterResponse(null, null, 0, "Your inventory is empty! Nothing to barter."));
            }
            int itemIndex = _random.Next(player.Inventory.Count);
            Item itemNpcWants = player.Inventory[itemIndex];
            int minNpcAcceptPrice = (int)(itemNpcWants.Price * 0.7); 
            int maxNpcOfferPrice = (int)(itemNpcWants.Price * 1.3); 
            if (minNpcAcceptPrice > maxNpcOfferPrice) minNpcAcceptPrice = maxNpcOfferPrice; 
            if (minNpcAcceptPrice <= 0 && maxNpcOfferPrice <=0) {
                 minNpcAcceptPrice = 0;
                 maxNpcOfferPrice = 1; 
            } else if (minNpcAcceptPrice == maxNpcOfferPrice) {
                 maxNpcOfferPrice = minNpcAcceptPrice +1; 
            }
            int npcInitialOffer = _random.Next(minNpcAcceptPrice, maxNpcOfferPrice + 1); 
            _activeBarterSession = new BarterSession(player.Name, itemNpcWants, npcInitialOffer, minNpcAcceptPrice, maxNpcOfferPrice);
            AdvancePlayerTime(player, 1); 
            return Ok(new InitiateBarterResponse(itemNpcWants.Name, itemNpcWants.Description, npcInitialOffer, $"An NPC is interested in your {itemNpcWants.Name} and offers {npcInitialOffer} gold."));
        }

        [HttpPost("barter/respond")]
        public IActionResult RespondToBarter([FromBody] BarterResponseRequest request)
        {
            var player = _players.FirstOrDefault();
            if (player == null)
                return BadRequest(new BarterActionResponse("No player available.", null, true, "error_no_player", null));

            if (_activeBarterSession == null || _activeBarterSession.PlayerId != player.Name)
                return BadRequest(new BarterActionResponse("No active barter session for this player.", player, true, "error_no_session", null));

            if (_activeBarterSession.CurrentTurn != BarterTurnState.PlayerResponding)
                return BadRequest(new BarterActionResponse("Not player's turn.", player, false, "error_wrong_turn", null));

            switch (request.Action.ToLower())
            {
                case "accept":
                    int agreedPrice = _activeBarterSession.NpcInitialOffer; 
                    player.Gold += agreedPrice;
                    var itemInInventory = player.Inventory.FirstOrDefault(i => i.Name == _activeBarterSession.ItemNpcWants.Name);
                    if (itemInInventory != null) player.Inventory.Remove(itemInInventory);
                    string acceptMsg = $"Sale complete! You sold {_activeBarterSession.ItemNpcWants.Name} for {agreedPrice} gold.";
                    _activeBarterSession = null; 
                    AdvancePlayerTime(player, 0); 
                    return Ok(new BarterActionResponse(acceptMsg, player, true, "sale_complete_npc_offer_accepted", null));

                case "deny":
                    string denyMsg = "Offer declined. The NPC walks away.";
                    _activeBarterSession = null; 
                    AdvancePlayerTime(player, 0);
                    return Ok(new BarterActionResponse(denyMsg, player, true, "negotiation_failed_player_denied", null));

                case "counter":
                    if (request.CounterAmount == null || request.CounterAmount <= 0)
                        return BadRequest(new BarterActionResponse("Invalid counter offer amount.", player, false, "error_invalid_counter", null));
                    
                    _activeBarterSession.PlayerCounterOffer = request.CounterAmount.Value;
                    
                    // NPC Decision Logic (Simplified)
                    if (request.CounterAmount.Value >= _activeBarterSession.MinNpcAcceptPrice && 
                        request.CounterAmount.Value <= _activeBarterSession.MaxNpcOfferPrice * 1.1) 
                    {
                        player.Gold += request.CounterAmount.Value;
                        var itemInInv = player.Inventory.FirstOrDefault(i => i.Name == _activeBarterSession.ItemNpcWants.Name);
                        if (itemInInv != null) player.Inventory.Remove(itemInInv);
                        string counterAcceptMsg = $"NPC accepts your counter-offer! Sold {_activeBarterSession.ItemNpcWants.Name} for {request.CounterAmount.Value} gold.";
                        _activeBarterSession = null; 
                        AdvancePlayerTime(player, 0);
                        return Ok(new BarterActionResponse(counterAcceptMsg, player, true, "sale_complete_player_counter_accepted", null));
                    }
                    else
                    {
                        string counterRejectMsg = $"NPC is not interested in your counter-offer of {request.CounterAmount.Value} gold and walks away.";
                        _activeBarterSession = null; 
                        AdvancePlayerTime(player, 0);
                        return Ok(new BarterActionResponse(counterRejectMsg, player, true, "negotiation_failed_npc_rejected_counter", null));
                    }

                default:
                    return BadRequest(new BarterActionResponse("Invalid action.", player, false, "error_invalid_action", null));
            }
        }
    }
}
