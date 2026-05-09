using System;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Runtime state for an enemy during a combat encounter.
    /// Enemies have no cards — they usually resolve simple repeating intents, with passive dice logic as fallback.
    /// </summary>
    public class EnemyCombatant
    {
        public EnemyData Data          { get; }
        public int       CurrentHealth { get; private set; }
        public bool      IsAlive       => CurrentHealth > 0;

        public DiceManager Dice { get; }

        public EnemyIntentData CurrentIntent
            => HasIntentPattern && _currentIntentIndex >= 0 ? Data.roundIntents[_currentIntentIndex] : null;

        public EnemyIntentData NextIntent
            => HasIntentPattern ? Data.roundIntents[(_currentIntentIndex + 1) % Data.roundIntents.Count] : null;

        private bool HasIntentPattern => Data.roundIntents != null && Data.roundIntents.Count > 0;
        private int _currentIntentIndex = -1;

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

            if (!HasIntentPattern) return;

            if (_currentIntentIndex < 0)
                _currentIntentIndex = 0;
            else
                _currentIntentIndex = (_currentIntentIndex + 1) % Data.roundIntents.Count;
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
        /// Calculates total damage this enemy deals this round from its current intent or legacy passives.
        /// </summary>
        public int CalculateDamage()
        {
            if (CurrentIntent != null)
                return Math.Max(0, CurrentIntent.damage);

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

        public string GetIntentSummary()
        {
            if (CurrentIntent == null)
                return "Acts from dice passives.";

            return string.IsNullOrWhiteSpace(CurrentIntent.description)
                ? $"{CurrentIntent.intentName}: {CurrentIntent.damage} damage."
                : CurrentIntent.description;
        }
    }
}
