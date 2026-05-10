using System;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Runtime state for an enemy during a combat encounter.
    /// Enemies have no cards and resolve simple repeating intents.
    /// </summary>
    public class EnemyCombatant
    {
        public EnemyData Data          { get; }
        public int       CurrentHealth { get; private set; }
        public int       Block         { get; private set; }
        public bool      IsAlive       => CurrentHealth > 0;

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

        /// <summary>Advances to the next intent at the start of a round.</summary>
        public void StartRound()
        {
            if (!HasIntentPattern) return;

            if (_currentIntentIndex < 0)
                _currentIntentIndex = 0;
            else
                _currentIntentIndex = (_currentIntentIndex + 1) % Data.roundIntents.Count;
        }

        /// <summary>
        /// Calculates damage this enemy intends to deal this round.
        /// </summary>
        public int CalculateIntentDamage()
        {
            return CalculateIntentDamage(null);
        }

        public int CalculateIntentDamage(int[] playerDiceRoll)
        {
            return CurrentIntent?.intentType == EEnemyIntentType.AttackFlat
                ? ResolveTriggeredMagnitude(CurrentIntent, playerDiceRoll)
                : 0;
        }

        public int ExecuteIntent(PlayerCombatant player)
        {
            if (CurrentIntent == null)
                return CalculateIntentDamage();

            int[] playerDiceRoll = player?.Dice?.CurrentRoll;
            int amount = Math.Max(0, CurrentIntent.magnitude);
            int count = Math.Max(1, CurrentIntent.count);
            int triggerCount = EvaluateTriggerCount(CurrentIntent, playerDiceRoll);
            switch (CurrentIntent.intentType)
            {
                case EEnemyIntentType.AttackFlat:
                    return amount * triggerCount;

                case EEnemyIntentType.Guard:
                    GainBlock(amount * triggerCount);
                    return 0;

                case EEnemyIntentType.ShredPlayerBlock:
                    if (triggerCount <= 0)
                        return 0;

                    player?.LoseBlock(amount * count * triggerCount);
                    return 0;

                case EEnemyIntentType.SapPlayerEnergy:
                    if (triggerCount <= 0)
                        return 0;

                    player?.ApplyNextTurnDieLoss(amount * count * triggerCount);
                    return 0;
            }

            return 0;
        }

        public string GetIntentSummary(int[] playerDiceRoll = null)
        {
            if (CurrentIntent == null)
                return "No action.";

            string fallback;
            int amount = Math.Max(0, CurrentIntent.magnitude);
            int count = Math.Max(1, CurrentIntent.count);
            int triggerCount = EvaluateTriggerCount(CurrentIntent, playerDiceRoll);
            bool usesPlayerDiceTrigger = CurrentIntent.triggerOn != EPokerHandType.Always;
            string triggerRule = BuildTriggerRuleText(CurrentIntent);
            switch (CurrentIntent.intentType)
            {
                case EEnemyIntentType.AttackFlat:
                    fallback = usesPlayerDiceTrigger
                        ? $"{CurrentIntent.intentName}: {ResolveTriggeredMagnitude(CurrentIntent, playerDiceRoll)} damage now ({triggerRule})."
                        : $"{CurrentIntent.intentName}: {amount} damage.";
                    break;
                case EEnemyIntentType.Guard:
                    fallback = usesPlayerDiceTrigger
                        ? $"{CurrentIntent.intentName}: gain {ResolveTriggeredMagnitude(CurrentIntent, playerDiceRoll)} block now ({triggerRule})."
                        : $"{CurrentIntent.intentName}: gain {amount} block.";
                    break;
                case EEnemyIntentType.ShredPlayerBlock:
                    fallback = usesPlayerDiceTrigger
                        ? $"{CurrentIntent.intentName}: remove up to {amount * count * triggerCount} block now ({triggerRule})."
                        : $"{CurrentIntent.intentName}: remove up to {amount * count} block.";
                    break;
                case EEnemyIntentType.SapPlayerEnergy:
                    int dieLoss = usesPlayerDiceTrigger ? amount * count * triggerCount : amount * count;
                    string dieText = dieLoss == 1 ? "1 die" : $"{dieLoss} dice";
                    fallback = usesPlayerDiceTrigger
                        ? $"{CurrentIntent.intentName}: lose {dieText} next turn ({triggerRule})."
                        : $"{CurrentIntent.intentName}: lose {dieText} next turn.";
                    break;
                default:
                    fallback = CurrentIntent.intentName;
                    break;
            }

            return string.IsNullOrWhiteSpace(CurrentIntent.description) ? fallback : CurrentIntent.description;
        }

        private static int EvaluateTriggerCount(EnemyIntentData intent, int[] playerDiceRoll)
        {
            if (intent == null)
                return 0;

            if (intent.triggerOn == EPokerHandType.Always)
                return 1;

            if (playerDiceRoll == null || playerDiceRoll.Length == 0)
                return 0;

            return Math.Max(0, PokerEvaluator.EvaluateTriggerCount(
                intent.triggerOn,
                playerDiceRoll,
                intent.dieValue,
                intent.valueThreshold));
        }

        private static int ResolveTriggeredMagnitude(EnemyIntentData intent, int[] playerDiceRoll)
        {
            return Math.Max(0, intent?.magnitude ?? 0) * EvaluateTriggerCount(intent, playerDiceRoll);
        }

        private static string BuildTriggerRuleText(EnemyIntentData intent)
        {
            if (intent == null || intent.triggerOn == EPokerHandType.Always)
                return string.Empty;

            switch (intent.triggerOn)
            {
                case EPokerHandType.PerDieValue:
                    return $"{Math.Max(0, intent.magnitude)} damage per {intent.dieValue} rolled";
                case EPokerHandType.PerOddDie:
                    return $"{Math.Max(0, intent.magnitude)} damage per odd die";
                case EPokerHandType.PerEvenDie:
                    return $"{Math.Max(0, intent.magnitude)} damage per even die";
                case EPokerHandType.PerHighDie:
                    return $"{Math.Max(0, intent.magnitude)} damage per die showing {intent.valueThreshold}+";
                case EPokerHandType.PerLowDie:
                    return $"{Math.Max(0, intent.magnitude)} damage per die showing {intent.valueThreshold} or less";
                case EPokerHandType.HighestDieValue:
                    return $"damage equal to your highest die × {Math.Max(0, intent.magnitude)}";
                case EPokerHandType.PerPair:
                    return $"{Math.Max(0, intent.magnitude)} damage per pair rolled";
                case EPokerHandType.PerTriple:
                    return $"{Math.Max(0, intent.magnitude)} damage per triple rolled";
                case EPokerHandType.PerFullHouse:
                    return $"{Math.Max(0, intent.magnitude)} damage if you roll a full house";
                case EPokerHandType.PerFourOfAKind:
                    return $"{Math.Max(0, intent.magnitude)} damage per four of a kind";
                case EPokerHandType.PerFiveOfAKind:
                    return $"{Math.Max(0, intent.magnitude)} damage per five of a kind";
                case EPokerHandType.PerUniqueDieValue:
                    return $"{Math.Max(0, intent.magnitude)} damage per unique die value";
                case EPokerHandType.IfStraight:
                    return $"{Math.Max(0, intent.magnitude)} damage if you roll a straight";
                default:
                    return "triggered by your roll";
            }
        }
    }
}
