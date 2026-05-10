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
            enemy.description = "A disruptive skirmisher that chips damage and scrambles your dice.";
            enemy.maxHealth = 8;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Pocket Sand",
                    intentType = EEnemyIntentType.RerollPlayerDice,
                    count = 1,
                },
                new EnemyIntentData
                {
                    intentName = "Needle Storm",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 2,
                    triggerOn = EPokerHandType.PerDieValue,
                    dieValue = 5,
                    description = "Deal 2 damage for each 5 in your roll.",
                },
                new EnemyIntentData
                {
                    intentName = "Shiv Twist",
                    intentType = EEnemyIntentType.WeakenPlayerDice,
                    magnitude = 1,
                    count = 1,
                },
            };
            return enemy;
        }

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Orc Warrior";
            enemy.description = "A bruiser that builds guard before punishing weak turns.";
            enemy.maxHealth = 12;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Hunker",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 3,
                },
                new EnemyIntentData
                {
                    intentName = "Cleave",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 3,
                },
                new EnemyIntentData
                {
                    intentName = "Batter",
                    intentType = EEnemyIntentType.WeakenPlayerDice,
                    magnitude = 1,
                    count = 2,
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
                },
                new EnemyIntentData
                {
                    intentName = "Ambush",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 5,
                },
                new EnemyIntentData
                {
                    intentName = "Fade",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 2,
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
                },
                new EnemyIntentData
                {
                    intentName = "Crush",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 8,
                },
                new EnemyIntentData
                {
                    intentName = "Fortify",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 4,
                },
            };
            return enemy;
        }

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Death Knight";
            enemy.description = "An elite controller that weakens your dice before execution hits.";
            enemy.maxHealth = 18;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Soul Rend",
                    intentType = EEnemyIntentType.WeakenPlayerDice,
                    magnitude = 1,
                    count = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Death Sentence",
                    intentType = EEnemyIntentType.AttackFlat,
                    magnitude = 2,
                    triggerOn = EPokerHandType.PerHighDie,
                    valueThreshold = 5,
                    description = "Deal 2 damage for each die showing 5 or higher.",
                },
                new EnemyIntentData
                {
                    intentName = "Dark Bulwark",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 5,
                },
            };
            return enemy;
        }
    }
}
