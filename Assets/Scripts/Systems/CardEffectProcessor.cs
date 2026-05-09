using System;
using System.Collections.Generic;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Resolves a single <see cref="CardEffectData"/> against the current combat state.
    /// All methods are stateless and operate on the provided combatant references.
    /// </summary>
    public static class CardEffectProcessor
    {
        /// <summary>Processes every effect on a card in order.</summary>
        public static void ProcessCard(CardData card, PlayerCombatant player, IReadOnlyList<EnemyCombatant> enemies,
            int targetEnemyIndex = -1)
        {
            foreach (var effect in card.effects)
                ProcessEffect(effect, player, enemies, targetEnemyIndex, card.targetsAllEnemies);
        }

        /// <summary>Resolves one effect entry against the current dice state.</summary>
        public static void ProcessEffect(CardEffectData effect, PlayerCombatant player, IReadOnlyList<EnemyCombatant> enemies,
            int targetEnemyIndex, bool targetsAllEnemies)
        {
            switch (effect.effectType)
            {
                case EEffectType.DealDamage:
                    ResolveDamage(effect, player, enemies, targetEnemyIndex, targetsAllEnemies);
                    break;

                case EEffectType.ConditionalDamage:
                    ResolveConditionalDamage(effect, player, enemies, targetEnemyIndex, targetsAllEnemies);
                    break;

                case EEffectType.SelfDamage:
                    player.TakeDamage(effect.magnitude);
                    break;

                case EEffectType.GainBlock:
                {
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        effect.triggerOn, player.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                    player.GainBlock(triggers * effect.magnitude);
                    break;
                }

                case EEffectType.RerollDice:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.RerollCount(effect.count));
                    break;

                case EEffectType.RerollAllDice:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.RerollAll());
                    break;

                case EEffectType.RerollByValue:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.RerollDiceShowingValue(effect.dieValue));
                    break;

                case EEffectType.AddDie:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.AddDie(effect.dieSides > 0 ? effect.dieSides : 6));
                    break;

                case EEffectType.AddTemporaryDie:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.AddDie(effect.dieSides > 0 ? effect.dieSides : 6, temporary: true));
                    break;

                case EEffectType.RemoveDie:
                    ResolveDiceEffect(effect, player, enemies, targetEnemyIndex, targetsAllEnemies,
                        dice => dice.RemoveDie());
                    break;

                case EEffectType.CycleHand:
                case EEffectType.AddWager:
                    break;
            }
        }

        public static bool CardRequiresEnemyTarget(CardData card)
        {
            foreach (var effect in card.effects)
            {
                if (EffectRequiresEnemyTarget(effect))
                    return true;
            }

            return false;
        }

        public static bool EffectRequiresEnemyTarget(CardEffectData effect)
        {
            switch (effect.effectType)
            {
                case EEffectType.DealDamage:
                case EEffectType.ConditionalDamage:
                case EEffectType.AddWager:
                    return true;

                case EEffectType.RerollDice:
                case EEffectType.RerollAllDice:
                case EEffectType.RerollByValue:
                case EEffectType.AddDie:
                case EEffectType.AddTemporaryDie:
                case EEffectType.RemoveDie:
                case EEffectType.UpgradeDie:
                case EEffectType.DowngradeDie:
                    return effect.diceTarget == ECardTarget.EnemyDice;

                default:
                    return false;
            }
        }

        private static void ResolveDamage(CardEffectData effect, PlayerCombatant player, IReadOnlyList<EnemyCombatant> enemies,
            int targetEnemyIndex, bool targetsAllEnemies)
        {
            if (effect.diceTarget == ECardTarget.PlayerDice)
            {
                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    effect.triggerOn, player.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                int damage = triggers * effect.magnitude;

                foreach (var enemy in GetEnemyTargets(enemies, targetEnemyIndex, targetsAllEnemies))
                    enemy.TakeDamage(damage);

                return;
            }

            foreach (var enemy in GetEnemyTargets(enemies, targetEnemyIndex, targetsAllEnemies))
            {
                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    effect.triggerOn, enemy.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                enemy.TakeDamage(triggers * effect.magnitude);
            }
        }

        private static void ResolveConditionalDamage(CardEffectData effect, PlayerCombatant player,
            IReadOnlyList<EnemyCombatant> enemies, int targetEnemyIndex, bool targetsAllEnemies)
        {
            if (effect.diceTarget == ECardTarget.PlayerDice)
            {
                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    effect.triggerOn, player.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                if (triggers > 0)
                {
                    foreach (var enemy in GetEnemyTargets(enemies, targetEnemyIndex, targetsAllEnemies))
                        enemy.TakeDamage(effect.magnitude);
                }
                else
                {
                    player.TakeDamage(effect.altMagnitude);
                }

                return;
            }

            bool anyTriggered = false;
            foreach (var enemy in GetEnemyTargets(enemies, targetEnemyIndex, targetsAllEnemies))
            {
                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    effect.triggerOn, enemy.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                if (triggers <= 0) continue;

                anyTriggered = true;
                enemy.TakeDamage(effect.magnitude);
            }

            if (!anyTriggered)
                player.TakeDamage(effect.altMagnitude);
        }

        private static void ResolveDiceEffect(CardEffectData effect, PlayerCombatant player, IReadOnlyList<EnemyCombatant> enemies,
            int targetEnemyIndex, bool targetsAllEnemies, Action<DiceManager> resolver)
        {
            if (effect.diceTarget == ECardTarget.PlayerDice)
            {
                resolver(player.Dice);
                return;
            }

            foreach (var enemy in GetEnemyTargets(enemies, targetEnemyIndex, targetsAllEnemies))
                resolver(enemy.Dice);
        }

        private static IEnumerable<EnemyCombatant> GetEnemyTargets(IReadOnlyList<EnemyCombatant> enemies,
            int targetEnemyIndex, bool targetsAllEnemies)
        {
            if (enemies == null)
                yield break;

            if (targetsAllEnemies)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    var enemy = enemies[i];
                    if (enemy != null && enemy.IsAlive)
                        yield return enemy;
                }

                yield break;
            }

            if (targetEnemyIndex < 0 || targetEnemyIndex >= enemies.Count)
                yield break;

            var targetEnemy = enemies[targetEnemyIndex];
            if (targetEnemy != null && targetEnemy.IsAlive)
                yield return targetEnemy;
        }
    }
}
