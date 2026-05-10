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

        [Tooltip("Optional secondary value used by intent types that act on multiple targets or scale repeated effects.")]
        public int count = 1;
    }
}
