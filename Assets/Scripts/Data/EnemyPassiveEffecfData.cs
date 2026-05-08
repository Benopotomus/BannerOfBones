using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [Serializable]
    public class EnemyPassiveEffectData
    {
        [TextArea(1, 3)]
        [Tooltip("Human-readable description shown in the combat UI.")]
        public string description;

        [Tooltip("The type of effect this passive applies.")]
        public EEffectType effectType;

        [Tooltip("Poker-dice condition evaluated against the enemy's own dice.")]
        public EPokerHandType triggerOn;

        [Tooltip("Exact die face value for PerDieValue and RerollByValue conditions.")]
        public int dieValue;

        [Tooltip("Threshold for PerHighDie / PerLowDie conditions.")]
        public int valueThreshold;

        [Tooltip("Damage dealt per trigger (for DealDamage effects).")]
        public int magnitude;

        [Tooltip("Number of dice to reroll (for RerollDice effects).")]
        public int count;
    }
}