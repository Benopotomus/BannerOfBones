using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Programmatic catalog of all 5 example enemies.
    /// Call <see cref="CreateAllEnemies"/> to obtain a list of runtime <see cref="EnemyData"/> instances.
    /// </summary>
    public static class EnemyCatalog
    {
        public static List<EnemyData> CreateAllEnemies()
        {
            return new List<EnemyData>
            {
                CreateGoblinScout(),
                CreateOrcWarrior(),
                CreateShadowWraith(),
                CreateStoneGolem(),
                CreateDeathKnight(),
            };
        }

        public static List<EnemyData> CreateEncounterGroup(int maxEnemies = 4)
        {
            var candidates = new List<(EnemyData enemy, int cost)>
            {
                (CreateGoblinScout(), 1),
                (CreateOrcWarrior(), 1),
                (CreateShadowWraith(), 1),
                (CreateStoneGolem(), 2),
                (CreateDeathKnight(), 2),
            };

            int budget = Random.Range(2, maxEnemies + 1);
            var encounter = new List<EnemyData>();

            while (candidates.Count > 0 && encounter.Count < maxEnemies)
            {
                int pick = Random.Range(0, candidates.Count);
                var candidate = candidates[pick];
                candidates.RemoveAt(pick);

                if (candidate.cost > budget)
                    continue;

                encounter.Add(candidate.enemy);
                budget -= candidate.cost;

                if (budget <= 0)
                    break;
            }

            if (encounter.Count == 0)
                encounter.Add(CreateGoblinScout());

            return encounter;
        }

        private static EnemyData CreateGoblinScout()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Goblin Scout";
            enemy.description = "A quick skirmisher that chips in with 2s and lucky doubles.";
            enemy.maxHealth = 8;
            enemy.diceCount = 2;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each [2] rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerDieValue,
                    dieValue = 2,
                    magnitude = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each pair rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerPair,
                    magnitude = 1,
                },
            };
            return enemy;
        }

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Orc Warrior";
            enemy.description = "A sturdy bruiser that leans on odd rolls and matching dice.";
            enemy.maxHealth = 12;
            enemy.diceCount = 2;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each odd die rolled ([1], [3], [5]).",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerOddDie,
                    magnitude = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each pair rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerPair,
                    magnitude = 1,
                },
            };
            return enemy;
        }

        private static EnemyData CreateShadowWraith()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Shadow Wraith";
            enemy.description = "A fragile predator that punishes low and odd results.";
            enemy.maxHealth = 10;
            enemy.diceCount = 3;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each odd die rolled ([1], [3], [5]).",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerOddDie,
                    magnitude = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each [1] rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerDieValue,
                    dieValue = 1,
                    magnitude = 1,
                },
            };
            return enemy;
        }

        private static EnemyData CreateStoneGolem()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Stone Golem";
            enemy.description = "A slow elite that smooths weak rolls and crushes with even numbers.";
            enemy.maxHealth = 16;
            enemy.diceCount = 3;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Rerolls all dice showing [1] at the start of each round.",
                    effectType = EEffectType.RerollByValue,
                    dieValue = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each even die rolled ([2], [4], [6]).",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerEvenDie,
                    magnitude = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each triple rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerTriple,
                    magnitude = 2,
                },
            };
            return enemy;
        }

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Death Knight";
            enemy.description = "An elite threat that spikes hard on pairs and high rolls.";
            enemy.maxHealth = 18;
            enemy.diceCount = 4;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each die showing 5 or 6.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerHighDie,
                    valueThreshold = 5,
                    magnitude = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each pair rolled.",
                    effectType = EEffectType.DealDamage,
                    triggerOn = EPokerHandType.PerPair,
                    magnitude = 2,
                },
            };
            return enemy;
        }
    }
}
