using System.Collections.Generic;

namespace ShopkeeperRPG.Models
{
    public class Player
    {
        public required string Name { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public int Gold { get; set; }
        public List<Item> Inventory { get; set; }
        public int CurrentHour { get; set; }
        public int CurrentDay { get; set; }
        public int ExhaustionLevel { get; set; }
        public bool TookLongRestToday { get; set; } // New property

        public Player()
        {
            Inventory = new List<Item>();
            CurrentHour = 8; // Default start time, e.g., 8 AM
            CurrentDay = 1;
            ExhaustionLevel = 0;
            TookLongRestToday = false; // Initialize new property
        }
    }
}
