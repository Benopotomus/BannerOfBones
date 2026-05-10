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

            int budget = BoBRandom.Range(2, maxEnemies + 1);
            var encounter = new List<EnemyData>();

            while (candidates.Count > 0 && encounter.Count < maxEnemies)
            {
                int pick = BoBRandom.Range(0, candidates.Count);
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
            enemy.description = "A dirty skirmisher that disrupts your setup before darting in.";
            enemy.maxHealth = 8;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Pocket Sand",
                    intentType = EEnemyIntentType.SapPlayerEnergy,
                    magnitude = 1,
                    description = "Pocket Sand: lose 1 energy next turn.",
                },
                new EnemyIntentData
                {
                    intentName = "Stab",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 4,
                    description = "Stab: deal 4 damage.",
                },
                new EnemyIntentData
                {
                    intentName = "Hamstring",
                    intentType = EEnemyIntentType.ShredPlayerBlock,
                    magnitude = 4,
                    description = "Hamstring: remove up to 4 block.",
                },
            };
            return enemy;
        }

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Orc Warrior";
            enemy.description = "A bruiser that builds guard, then smashes through defenses.";
            enemy.maxHealth = 12;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Hunker",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 3,
                    description = "Hunker: gain 3 block.",
                },
                new EnemyIntentData
                {
                    intentName = "Cleave",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 3,
                    description = "Cleave: deal 3 damage.",
                },
                new EnemyIntentData
                {
                    intentName = "Guard Break",
                    intentType = EEnemyIntentType.ShredPlayerBlock,
                    magnitude = 6,
                    description = "Guard Break: remove up to 6 block.",
                },
            };
            return enemy;
        }

        private static EnemyData CreateShadowWraith()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Shadow Wraith";
            enemy.description = "A slippery duelist that alternates pressure, defense, and burst damage.";
            enemy.maxHealth = 10;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Shroud",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 3,
                    description = "Shroud: gain 3 block.",
                },
                new EnemyIntentData
                {
                    intentName = "Ambush",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 5,
                    description = "Ambush: deal 5 damage.",
                },
                new EnemyIntentData
                {
                    intentName = "Fade",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 2,
                    description = "Fade: gain 2 block.",
                },
            };
            return enemy;
        }

        private static EnemyData CreateStoneGolem()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Stone Golem";
            enemy.description = "A slow elite that braces before crushing blows.";
            enemy.maxHealth = 16;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Wind Up",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 3,
                    description = "Wind Up: gain 3 block.",
                },
                new EnemyIntentData
                {
                    intentName = "Crush",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 8,
                    description = "Crush: deal 8 damage.",
                },
                new EnemyIntentData
                {
                    intentName = "Fortify",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 4,
                    description = "Fortify: gain 4 block.",
                },
            };
            return enemy;
        }

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Death Knight";
            enemy.description = "An elite controller that drains your tempo before execution hits.";
            enemy.maxHealth = 18;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Soul Rend",
                    intentType = EEnemyIntentType.SapPlayerEnergy,
                    magnitude = 1,
                    description = "Soul Rend: lose 1 energy next turn.",
                },
                new EnemyIntentData
                {
                    intentName = "Executioner Swing",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 9,
                    description = "Executioner Swing: deal 9 damage.",
                },
                new EnemyIntentData
                {
                    intentName = "Dark Bulwark",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 5,
                    description = "Dark Bulwark: gain 5 block.",
                },
            };
            return enemy;
        }
    }
}
