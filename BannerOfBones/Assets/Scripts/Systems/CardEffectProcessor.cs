namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Resolves a single <see cref="CardEffectData"/> against the current combat state.
    /// All methods are stateless and operate on the provided combatant references.
    /// </summary>
    public static class CardEffectProcessor
    {
        /// <summary>Processes every effect on a card in order.</summary>
        public static void ProcessCard(CardData card, PlayerCombatant player, EnemyCombatant enemy)
        {
            foreach (var effect in card.effects)
                ProcessEffect(effect, player, enemy);
        }

        /// <summary>Resolves one effect entry against the current dice state.</summary>
        public static void ProcessEffect(CardEffectData effect, PlayerCombatant player, EnemyCombatant enemy)
        {
            int[] targetDice = effect.diceTarget == ECardTarget.PlayerDice
                ? player.Dice.CurrentRoll
                : enemy.Dice.CurrentRoll;

            switch (effect.effectType)
            {
                case EEffectType.DealDamage:
                {
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        effect.triggerOn, targetDice, effect.dieValue, effect.valueThreshold);
                    enemy.TakeDamage(triggers * effect.magnitude);
                    break;
                }

                case EEffectType.ConditionalDamage:
                {
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        effect.triggerOn, targetDice, effect.dieValue, effect.valueThreshold);
                    if (triggers > 0)
                        enemy.TakeDamage(effect.magnitude);
                    else
                        player.TakeDamage(effect.altMagnitude);
                    break;
                }

                case EEffectType.SelfDamage:
                    player.TakeDamage(effect.magnitude);
                    break;

                case EEffectType.GainBlock:
                {
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        effect.triggerOn, targetDice, effect.dieValue, effect.valueThreshold);
                    player.GainBlock(triggers * effect.magnitude);
                    break;
                }

                case EEffectType.RerollDice:
                    if (effect.diceTarget == ECardTarget.PlayerDice)
                        player.Dice.RerollCount(effect.count);
                    else
                        enemy.Dice.RerollCount(effect.count);
                    break;

                case EEffectType.RerollAllDice:
                    if (effect.diceTarget == ECardTarget.PlayerDice)
                        player.Dice.RerollAll();
                    else
                        enemy.Dice.RerollAll();
                    break;

                case EEffectType.RerollByValue:
                    if (effect.diceTarget == ECardTarget.PlayerDice)
                        player.Dice.RerollDiceShowingValue(effect.dieValue);
                    else
                        enemy.Dice.RerollDiceShowingValue(effect.dieValue);
                    break;

                case EEffectType.AddDie:
                    if (effect.diceTarget == ECardTarget.PlayerDice)
                        player.Dice.AddDie();
                    else
                        enemy.Dice.AddDie();
                    break;

                case EEffectType.RemoveDie:
                    if (effect.diceTarget == ECardTarget.EnemyDice)
                        enemy.Dice.RemoveDie();
                    else
                        player.Dice.RemoveDie();
                    break;
            }
        }
    }
}