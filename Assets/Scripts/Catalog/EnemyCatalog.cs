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
            enemy.diceCount = 2;
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
                    intentName = "Stab",
                    intentType = EEnemyIntentType.AttackFromHighestDie,
                    magnitude = 1,
                },
                new EnemyIntentData
                {
                    intentName = "Shiv Twist",
                    intentType = EEnemyIntentType.WeakenPlayerDice,
                    magnitude = 1,
                    count = 1,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Orc Warrior";
            enemy.description = "A bruiser that builds guard before punishing weak turns.";
            enemy.maxHealth = 12;
            enemy.diceCount = 2;
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
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateShadowWraith()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Shadow Wraith";
            enemy.description = "A trickster that repeatedly disrupts your setup before striking.";
            enemy.maxHealth = 10;
            enemy.diceCount = 3;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Hex",
                    intentType = EEnemyIntentType.RerollPlayerDice,
                    count = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Ambush",
                    intentType = EEnemyIntentType.AttackFromHighestDie,
                    magnitude = 1,
                },
                new EnemyIntentData
                {
                    intentName = "Fade",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 2,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateStoneGolem()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Stone Golem";
            enemy.description = "A scaling elite that upgrades itself before crushing blows.";
            enemy.maxHealth = 16;
            enemy.diceCount = 3;
            enemy.roundIntents = new List<EnemyIntentData>
            {
                new EnemyIntentData
                {
                    intentName = "Wind Up",
                    intentType = EEnemyIntentType.UpgradeSelfDice,
                    count = 1,
                },
                new EnemyIntentData
                {
                    intentName = "Crush",
                    intentType = EEnemyIntentType.AttackFromHighestDie,
                    magnitude = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Fortify",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 4,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name = enemy.enemyName = "Death Knight";
            enemy.description = "An elite controller that weakens your dice before execution hits.";
            enemy.maxHealth = 18;
            enemy.diceCount = 4;
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
                    intentName = "Executioner Swing",
                    intentType = EEnemyIntentType.AttackFromHighestDie,
                    magnitude = 2,
                },
                new EnemyIntentData
                {
                    intentName = "Dark Bulwark",
                    intentType = EEnemyIntentType.Guard,
                    magnitude = 5,
                },
            };
            enemy.passiveEffects = new List<EnemyPassiveEffectData>();
            return enemy;
        }
    }
}
