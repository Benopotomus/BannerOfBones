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

        [Tooltip("Simple repeating action pattern shown to the player each round.")]
        public List<EnemyIntentData> roundIntents = new List<EnemyIntentData>();
    }
}
