using ShopkeeperRPG.Models; // Assuming Item model is here

namespace ShopkeeperRPG.Models
{
    public enum BarterTurnState
    {
        NpcOffering,        // NPC has made an initial offer
        PlayerResponding,   // Player needs to respond (accept, deny, counter)
        NpcRespondingToCounter // NPC is responding to player's counter offer
        // We might add more states later if NPC can counter multiple times
    }

    public class BarterSession
    {
        public required string PlayerId { get; set; } // Using player Name as ID for now
        public required Item ItemNpcWants { get; set; }
        public int NpcInitialOffer { get; set; }
        public int? PlayerCounterOffer { get; set; } // Nullable if player hasn't countered
        public int? NpcFinalOffer { get; set; }      // Nullable if NPC doesn't make a final counter
        public BarterTurnState CurrentTurn { get; set; }
        public int MinNpcAcceptPrice { get; set; }
        public int MaxNpcOfferPrice { get; set; }

        // Constructor to initialize a new barter session
        public BarterSession(string playerId, Item itemNpcWants, int npcInitialOffer, int minNpcAcceptPrice, int maxNpcOfferPrice)
        {
            PlayerId = playerId;
            ItemNpcWants = itemNpcWants;
            NpcInitialOffer = npcInitialOffer;
            MinNpcAcceptPrice = minNpcAcceptPrice;
            MaxNpcOfferPrice = maxNpcOfferPrice;
            CurrentTurn = BarterTurnState.PlayerResponding; // After NPC's initial offer, it's player's turn
            // PlayerCounterOffer and NpcFinalOffer start as null
        }
    }
}
