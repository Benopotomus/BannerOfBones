using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "CardGame/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;

        [TextArea(2, 5)]
        public string description;

        public int maxHealth;

        [Range(2, 5)]
        [Tooltip("Number of d6 this enemy rolls each round.")]
        public int diceCount;

        [Tooltip("Simple repeating action pattern shown to the player each round.")]
        public List<EnemyIntentData> roundIntents = new List<EnemyIntentData>();

        [Tooltip("Legacy passive effects evaluated against this enemy's dice roll when no intent pattern is defined.")]
        public List<EnemyPassiveEffectData> passiveEffects = new List<EnemyPassiveEffectData>();
    }
}
