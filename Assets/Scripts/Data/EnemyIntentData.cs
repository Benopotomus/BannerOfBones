using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [Serializable]
    public class EnemyIntentData
    {
        [Tooltip("Short label shown in the enemy UI.")]
        public string intentName;

        [TextArea(1, 3)]
        [Tooltip("Human-readable description shown in the combat UI.")]
        public string description;

        [Tooltip("Flat damage this intent deals at the end of the player's turn.")]
        public int damage;
    }
}
