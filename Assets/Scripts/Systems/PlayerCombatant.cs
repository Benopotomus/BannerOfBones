using System;
using System.Collections.Generic;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Runtime state for the player during a combat encounter.
    /// </summary>
    public class PlayerCombatant
    {
        public int MaxHealth     { get; }
        public int CurrentHealth { get; private set; }
        public int Block         { get; private set; }
        public bool IsAlive      => CurrentHealth > 0;

        public DiceManager  Dice   { get; }
        public DeckManager  Deck   { get; }
        public EnergyManager Energy { get; }
        public int PendingNextTurnDieLoss { get; private set; }
        private readonly List<int> _temporarilySuppressedDieSides = new List<int>();

        /// <summary>Cards with Persistent duration currently in play.</summary>
        public List<PersistentCardRuntime> ActivePersistentCards { get; } = new List<PersistentCardRuntime>();
        public List<WagerData> ActiveWagers { get; } = new List<WagerData>();

        /// <summary>
        /// Creates player combat state. Pass currentHealth as -1 to start at full health.
        /// </summary>
        public PlayerCombatant(int maxHealth, int maxEnergy, List<CardData> deck, int currentHealth = -1, int diceCount = 5)
        {
            MaxHealth    = Math.Max(1, maxHealth);
            CurrentHealth = currentHealth >= 0
                ? Math.Min(currentHealth, MaxHealth)
                : MaxHealth;
            Dice   = new DiceManager(diceCount);
            Deck   = new DeckManager(deck);
            Energy = new EnergyManager(maxEnergy);
        }

        /// <summary>
        /// Applies incoming damage after subtracting current block.
        /// Block is consumed first; any remainder reduces health.
        /// </summary>
        public void TakeDamage(int amount)
        {
            int absorbed = Math.Min(Block, amount);
            Block         -= absorbed;
            CurrentHealth -= amount - absorbed;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        public void GainBlock(int amount)
        {
            Block += Math.Max(0, amount);
        }

        public void ClearBlock()
        {
            Block = 0;
        }

        public int LoseBlock(int amount)
        {
            int removed = Math.Min(Block, Math.Max(0, amount));
            Block -= removed;
            return removed;
        }

        /// <summary>
        /// Called at the start of each round: removes temporary dice, clears block, resets energy,
        /// discards the previous hand, draws a new hand, and rolls all dice.
        /// </summary>
        public void StartRound()
        {
            // Restore one-turn die suppression first so this round starts from the true persistent pool.
            RestoreTemporarilySuppressedDice();
            Dice.RemoveTemporaryDice();
            for (int i = 0; i < PendingNextTurnDieLoss; i++)
            {
                if (Dice.TryRemoveDie(out var removedDie))
                    _temporarilySuppressedDieSides.Add(removedDie.Sides);
            }
            PendingNextTurnDieLoss = 0;
            ClearBlock();
            Energy.ResetEnergy();
            Deck.DiscardHandExceptRetained();
            Deck.DrawCards();
            Dice.RollAll();
        }

        public void ApplyNextTurnDieLoss(int amount)
        {
            PendingNextTurnDieLoss += Math.Max(0, amount);
        }

        private void RestoreTemporarilySuppressedDice()
        {
            if (_temporarilySuppressedDieSides.Count == 0)
                return;

            foreach (var sides in _temporarilySuppressedDieSides)
                Dice.AddDie(sides);

            _temporarilySuppressedDieSides.Clear();
        }
    }
}
