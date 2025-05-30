using System.Collections.Generic;
// using ShopkeeperRPG.Controllers; // Not strictly needed if using fully qualified name

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
        public bool TookLongRestToday { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public SkillSet Skills { get; private set; }

        // New parameterized constructor
        public Player(string name, int strength, int dexterity, int constitution, int intelligence, int wisdom, int charisma, int initialGold)
        {
            this.Name = name;
            this.Strength = strength;
            this.Dexterity = dexterity;
            this.Constitution = constitution;
            this.Intelligence = intelligence;
            this.Wisdom = wisdom;
            this.Charisma = charisma;
            this.Gold = initialGold;

            // Initialize other default properties
            this.Inventory = new List<Item>();
            this.CurrentHour = 8;
            this.CurrentDay = 1;
            this.ExhaustionLevel = 0;
            this.TookLongRestToday = false;

            // Calculate HP
            int constitutionModifier = ShopkeeperRPG.Controllers.PlayerController.GetAbilityModifier(this.Constitution);
            this.MaxHP = 10 + constitutionModifier;
            this.CurrentHP = this.MaxHP;

            // Initialize SkillSet AFTER stats and HP are set
            this.Skills = new SkillSet(this);
        }
    }
}
