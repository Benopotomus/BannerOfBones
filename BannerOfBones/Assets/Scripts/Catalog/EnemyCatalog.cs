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

        // ── Enemy 01 ──────────────────────────────────────────────────────────────

        private static EnemyData CreateGoblinScout()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name      = enemy.enemyName = "Goblin Scout";
            enemy.description = "A nimble scout who loves rolling 2s. Punishes paired dice.";
            enemy.maxHealth = 15;
            enemy.diceCount = 2;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each [2] rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerDieValue,
                    dieValue    = 2,
                    magnitude   = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each pair rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerPair,
                    magnitude   = 2,
                },
            };
            return enemy;
        }

        // ── Enemy 02 ──────────────────────────────────────────────────────────────

        private static EnemyData CreateOrcWarrior()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name      = enemy.enemyName = "Orc Warrior";
            enemy.description = "A brutish fighter who rewards matching dice and grinds with odd rolls.";
            enemy.maxHealth = 25;
            enemy.diceCount = 3;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each pair rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerPair,
                    magnitude   = 2,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each odd die rolled ([1], [3], [5]).",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerOddDie,
                    magnitude   = 1,
                },
            };
            return enemy;
        }

        // ── Enemy 03 ──────────────────────────────────────────────────────────────

        private static EnemyData CreateShadowWraith()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name      = enemy.enemyName = "Shadow Wraith";
            enemy.description = "An ethereal predator that thrives on low rolls and odd numbers.";
            enemy.maxHealth = 20;
            enemy.diceCount = 4;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each odd die rolled ([1], [3], [5]).",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerOddDie,
                    magnitude   = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each [1] rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerDieValue,
                    dieValue    = 1,
                    magnitude   = 2,
                },
            };
            return enemy;
        }

        // ── Enemy 04 ──────────────────────────────────────────────────────────────

        private static EnemyData CreateStoneGolem()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name      = enemy.enemyName = "Stone Golem";
            enemy.description = "A massive construct. Rerolls weak dice and devastates with triples.";
            enemy.maxHealth = 40;
            enemy.diceCount = 3;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                // Pre-round effect: rerolls any die showing [1]
                new EnemyPassiveEffectData
                {
                    description = "Rerolls all dice showing [1] at the start of each round.",
                    effectType  = EEffectType.RerollByValue,
                    dieValue    = 1,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 3 damage for each triple rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerTriple,
                    magnitude   = 3,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 1 damage for each even die rolled ([2], [4], [6]).",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerEvenDie,
                    magnitude   = 1,
                },
            };
            return enemy;
        }

        // ── Enemy 05 ──────────────────────────────────────────────────────────────

        private static EnemyData CreateDeathKnight()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.name      = enemy.enemyName = "Death Knight";
            enemy.description = "The most dangerous foe. Punishes pairs, triples, and straights heavily.";
            enemy.maxHealth = 35;
            enemy.diceCount = 5;
            enemy.passiveEffects = new List<EnemyPassiveEffectData>
            {
                new EnemyPassiveEffectData
                {
                    description = "Deal 2 damage for each pair rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerPair,
                    magnitude   = 2,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 4 damage for each triple rolled.",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.PerTriple,
                    magnitude   = 4,
                },
                new EnemyPassiveEffectData
                {
                    description = "Deal 6 damage if a straight is rolled (1-2-3-4-5 or 2-3-4-5-6).",
                    effectType  = EEffectType.DealDamage,
                    triggerOn   = EPokerHandType.IfStraight,
                    magnitude   = 6,
                },
            };
            return enemy;
        }
    }
}