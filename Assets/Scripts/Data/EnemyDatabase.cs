using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "CardGame/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public const string ResourcesPath = "EnemyDatabase";

        public List<EnemyData> allEnemies = new List<EnemyData>();

        public bool HasEnemies => allEnemies != null && allEnemies.Count > 0;

        public List<EnemyData> CreateAllRuntimeEnemies()
        {
            var enemies = new List<EnemyData>();
            if (allEnemies == null)
                return enemies;

            foreach (var enemy in allEnemies)
            {
                if (enemy == null)
                    continue;

                var clone = Instantiate(enemy);
                clone.name = enemy.enemyName;
                enemies.Add(clone);
            }

            return enemies;
        }
    }
}
