using System;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Runtime state for an enemy during a combat encounter.
    /// Enemies have no cards — they deal damage through passive effects evaluated each round.
    /// </summary>
    public class EnemyCombatant
    {
        public EnemyData Data          { get; }
        public int       CurrentHealth { get; private set; }
        public bool      IsAlive       => CurrentHealth > 0;

        public DiceManager Dice { get; }

        public EnemyCombatant(EnemyData data)
        {
            Data          = data;
            CurrentHealth = data.maxHealth;
            Dice          = new DiceManager(data.diceCount);
        }

        public void TakeDamage(int amount)
        {
            CurrentHealth -= Math.Max(0, amount);
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        /// <summary>Rolls all dice at the start of a round.</summary>
        public void StartRound()
        {
            Dice.RollAll();
        }

        /// <summary>
        /// Applies pre-round passive effects (e.g., rerolling specific die values).
        /// Call this after StartRound and before the player turn begins.
        /// </summary>
        public void ApplyPreRoundEffects()
        {
            foreach (var passive in Data.passiveEffects)
            {
                switch (passive.effectType)
                {
                    case EEffectType.RerollDice:
                        Dice.RerollCount(passive.count);
                        break;
                    case EEffectType.RerollAllDice:
                        Dice.RerollAll();
                        break;
                    case EEffectType.RerollByValue:
                        Dice.RerollDiceShowingValue(passive.dieValue);
                        break;
                }
            }
        }

        /// <summary>
        /// Calculates total damage this enemy deals this round from all DealDamage passives.
        /// </summary>
        public int CalculateDamage()
        {
            int total = 0;
            foreach (var passive in Data.passiveEffects)
            {
                if (passive.effectType != EEffectType.DealDamage) continue;
                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    passive.triggerOn, Dice.CurrentRoll, passive.dieValue, passive.valueThreshold);
                total += triggers * passive.magnitude;
            }
            return total;
        }
    }
}