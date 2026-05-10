using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [Serializable]
    public class EnemyIntentData
    {
        [Tooltip("What gameplay action this intent performs.")]
        public EEnemyIntentType intentType = EEnemyIntentType.AttackFlat;

        [Tooltip("Short label shown in the enemy UI.")]
        public string intentName;

        [TextArea(1, 3)]
        [Tooltip("Human-readable description shown in the combat UI.")]
        public string description;

        [Tooltip("Primary value used by this intent. For attacks this is damage or multiplier.")]
        public int magnitude;

        [Tooltip("How many dice this intent affects for reroll/weaken/upgrade actions.")]
        public int count = 1;

        [Tooltip("Optional player-dice condition that controls how many times this intent triggers.")]
        public EPokerHandType triggerOn = EPokerHandType.Always;

        [Tooltip("Exact die face value used by PerDieValue triggers.")]
        public int dieValue;

        [Tooltip("Threshold used by PerHighDie / PerLowDie triggers.")]
        public int valueThreshold;
    }
}
