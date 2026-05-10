namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Tracks the player's energy for a single round.
    /// Energy resets to MaxEnergy at the start of each round.
    /// </summary>
    public class EnergyManager
    {
        public int MaxEnergy     { get; }
        public int CurrentEnergy { get; private set; }
        public int PendingNextTurnPenalty { get; private set; }

        public EnergyManager(int maxEnergy)
        {
            MaxEnergy    = maxEnergy;
            CurrentEnergy = maxEnergy;
        }

        public void ResetEnergy()
        {
            CurrentEnergy = System.Math.Max(0, MaxEnergy - PendingNextTurnPenalty);
            PendingNextTurnPenalty = 0;
        }

        public bool CanAfford(int cost) => CurrentEnergy >= cost;

        /// <summary>Deducts cost from current energy. Returns false if insufficient energy.</summary>
        public bool TrySpendEnergy(int cost)
        {
            if (!CanAfford(cost)) return false;
            CurrentEnergy -= cost;
            return true;
        }

        public void ApplyNextTurnPenalty(int amount)
        {
            PendingNextTurnPenalty += System.Math.Max(0, amount);
        }
    }
}
