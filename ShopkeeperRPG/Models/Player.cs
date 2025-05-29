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

        public Player()
        {
            Inventory = new List<Item>();
        }
    }
}
