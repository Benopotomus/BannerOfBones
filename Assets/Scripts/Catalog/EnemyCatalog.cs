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
            enemy.description = "A quick skirmisher with a simple two-step attack pattern.";
            enemy.maxHealth = 8;
            enemy.diceCount = 2;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Feint",
                    damage = 1,
                },
                new EnemyIntentData
                {
                    intentName = "Stab",
                    damage = 2,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Orc Warrior";
            enemy.description = "A bruiser that alternates between a heavy hit and a follow-up swing.";
            enemy.maxHealth = 12;
            enemy.diceCount = 2;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Cleave",
                    damage = 3,
                },
                new EnemyIntentData
                {
                    intentName = "Backhand",
                    damage = 2,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateShadowWraith()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Shadow Wraith";
            enemy.description = "A fragile predator that alternates chip damage with a sharper strike.";
            enemy.maxHealth = 10;
            enemy.diceCount = 3;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Drain",
                    damage = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Ambush",
                    damage = 3,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateStoneGolem()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Stone Golem";
            enemy.description = "A slow elite that telegraphs a steady slam pattern.";
            enemy.maxHealth = 16;
            enemy.diceCount = 3;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Wind Up",
                    damage = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Crush",
                    damage = 4,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Death Knight";
            enemy.description = "An elite threat with a very clear spike-damage cadence.";
            enemy.maxHealth = 18;
            enemy.diceCount = 4;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Harry",
                    damage = 3,
                },
                new EnemyIntentData
                {
                    intentName = "Executioner Swing",
                    damage = 5,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }
    }
}
