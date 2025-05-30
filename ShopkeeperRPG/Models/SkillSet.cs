using System.Collections.Generic;
using ShopkeeperRPG.Models; // For Skill and AbilityScore enums
// Assuming GetAbilityModifier is now public static in PlayerController.
// If it were moved to a GameRules class, this using would change.
using static ShopkeeperRPG.Controllers.PlayerController;

namespace ShopkeeperRPG.Models
{
    public class SkillSet
    {
        private readonly Dictionary<Skill, int> _modifiers = new Dictionary<Skill, int>();

        // Constructor takes a Player object to access its base ability scores
        public SkillSet(Player player)
        {
            // Populate all skill modifiers based on the player's ability scores
            _modifiers[Skill.Athletics] = GetAbilityModifier(player.Strength);
            _modifiers[Skill.Acrobatics] = GetAbilityModifier(player.Dexterity);
            _modifiers[Skill.SleightOfHand] = GetAbilityModifier(player.Dexterity);
            _modifiers[Skill.Stealth] = GetAbilityModifier(player.Dexterity);
            // Constitution has no directly associated skills in standard D&D 5e
            _modifiers[Skill.Arcana] = GetAbilityModifier(player.Intelligence);
            _modifiers[Skill.History] = GetAbilityModifier(player.Intelligence);
            _modifiers[Skill.Investigation] = GetAbilityModifier(player.Intelligence);
            _modifiers[Skill.Nature] = GetAbilityModifier(player.Intelligence);
            _modifiers[Skill.Religion] = GetAbilityModifier(player.Intelligence);
            _modifiers[Skill.AnimalHandling] = GetAbilityModifier(player.Wisdom);
            _modifiers[Skill.Insight] = GetAbilityModifier(player.Wisdom);
            _modifiers[Skill.Medicine] = GetAbilityModifier(player.Wisdom);
            _modifiers[Skill.Perception] = GetAbilityModifier(player.Wisdom);
            _modifiers[Skill.Survival] = GetAbilityModifier(player.Wisdom);
            _modifiers[Skill.Deception] = GetAbilityModifier(player.Charisma);
            _modifiers[Skill.Intimidation] = GetAbilityModifier(player.Charisma);
            _modifiers[Skill.Performance] = GetAbilityModifier(player.Charisma);
            _modifiers[Skill.Persuasion] = GetAbilityModifier(player.Charisma);
        }

        public int GetModifier(Skill skill)
        {
            return _modifiers.TryGetValue(skill, out int modifier) ? modifier : 0;
        }

        // Optional: Method to get all modifiers, e.g., for display or saving
        public IReadOnlyDictionary<Skill, int> GetAllModifiers()
        {
            return _modifiers;
        }
    }
}
