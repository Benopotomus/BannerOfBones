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
        public int       Block         { get; private set; }
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
            Block         = 0;
            Dice          = new DiceManager(data.diceCount);
        }

        public void TakeDamage(int amount)
        {
            int incoming = Math.Max(0, amount);
            int absorbed = Math.Min(Block, incoming);
            Block = Math.Max(0, Block - absorbed);
            CurrentHealth -= incoming - absorbed;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        /// <summary>
        /// Adds block to this enemy.
        /// Block absorbs incoming damage before health is reduced.
        /// </summary>
        public void GainBlock(int amount)
        {
            Block += Math.Max(0, amount);
        }

        /// <summary>
        /// Clears all enemy block.
        /// Called at the start of each round.
        /// </summary>
        public void ClearBlock()
        {
            Block = 0;
        }

        /// <summary>Rolls all dice at the start of a round.</summary>
        public void StartRound()
        {
            Dice.RollAll();
            ClearBlock();

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
        /// Calculates damage this enemy intends to deal this round.
        /// </summary>
        public int CalculateIntentDamage()
        {
            if (CurrentIntent != null)
            {
                switch (CurrentIntent.intentType)
                {
                    case EEnemyIntentType.AttackFlat:
                        return Math.Max(0, CurrentIntent.magnitude);
                    case EEnemyIntentType.AttackFromHighestDie:
                        return Math.Max(0, CurrentIntent.magnitude) * PokerEvaluator.HighestDie(Dice.CurrentRoll);
                    default:
                        return 0;
                }
            }

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

        public int ExecuteIntent(PlayerCombatant player)
        {
            if (CurrentIntent == null)
                return CalculateIntentDamage();

            int amount = Math.Max(0, CurrentIntent.magnitude);
            int count = Math.Max(1, CurrentIntent.count);

            switch (CurrentIntent.intentType)
            {
                case EEnemyIntentType.AttackFlat:
                case EEnemyIntentType.AttackFromHighestDie:
                    return CalculateIntentDamage();

                case EEnemyIntentType.Guard:
                    GainBlock(amount);
                    return 0;

                case EEnemyIntentType.RerollPlayerDice:
                    player?.Dice.RerollLowestDice(count);
                    return 0;

                case EEnemyIntentType.WeakenPlayerDice:
                    player?.Dice.AdjustHighestDice(count, -amount);
                    return 0;

                case EEnemyIntentType.UpgradeSelfDice:
                    Dice.UpgradeHighestDice(count);
                    return 0;
            }

            return 0;
        }

        public string GetIntentSummary()
        {
            if (CurrentIntent == null)
                return "Acts from dice passives.";

            string fallback;
            int amount = Math.Max(0, CurrentIntent.magnitude);
            int count = Math.Max(1, CurrentIntent.count);
            switch (CurrentIntent.intentType)
            {
                case EEnemyIntentType.AttackFlat:
                    fallback = $"{CurrentIntent.intentName}: {amount} damage.";
                    break;
                case EEnemyIntentType.AttackFromHighestDie:
                    fallback = $"{CurrentIntent.intentName}: {amount}×highest die damage ({CalculateIntentDamage()} now).";
                    break;
                case EEnemyIntentType.Guard:
                    fallback = $"{CurrentIntent.intentName}: gain {amount} block.";
                    break;
                case EEnemyIntentType.RerollPlayerDice:
                    fallback = $"{CurrentIntent.intentName}: reroll {count} of your lowest dice.";
                    break;
                case EEnemyIntentType.WeakenPlayerDice:
                    fallback = $"{CurrentIntent.intentName}: reduce your {count} highest dice by {amount}.";
                    break;
                case EEnemyIntentType.UpgradeSelfDice:
                    fallback = $"{CurrentIntent.intentName}: upgrade {count} of its highest dice.";
                    break;
                default:
                    fallback = CurrentIntent.intentName;
                    break;
            }

            return string.IsNullOrWhiteSpace(CurrentIntent.description) ? fallback : CurrentIntent.description;
        }
    }
}
