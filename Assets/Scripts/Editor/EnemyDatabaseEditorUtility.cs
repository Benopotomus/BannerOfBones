using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BannerOfBones.CardGame.Editor
{
    public static class EnemyDatabaseEditorUtility
    {
        public const string DatabaseAssetPath = "Assets/Resources/EnemyDatabase.asset";
        private const string ResourcesFolder = "Assets/Resources";
        private const string EnemyAssetsFolder = ResourcesFolder + "/EnemyLibrary";

        [MenuItem("CardGame/Enemy Database")]
        public static void OpenWindow()
        {
            EnemyDatabaseWindow.OpenWindow();
        }

        [MenuItem("CardGame/Create Enemy Database From Defaults")]
        public static void CreateDatabaseFromDefaultsMenu()
        {
            var database = CreateOrUpdateDatabaseFromDefaults(true);
            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
        }

        public static EnemyDatabase LoadDatabase()
        {
            return AssetDatabase.LoadAssetAtPath<EnemyDatabase>(DatabaseAssetPath);
        }

        public static EnemyDatabase CreateOrUpdateDatabaseFromDefaults(bool overwriteExistingEnemies)
        {
            EnsureFolder(ResourcesFolder);
            EnsureFolder(EnemyAssetsFolder);

            var database = LoadDatabase();
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<EnemyDatabase>();
                AssetDatabase.CreateAsset(database, DatabaseAssetPath);
            }

            var existingEnemiesByName = LoadExistingEnemiesByName();
            var defaultEnemies = EnemyCatalog.CreateDefaultEnemyLibrary();

            try
            {
                var createdEnemies = new List<EnemyData>(defaultEnemies.Count);
                foreach (var defaultEnemy in defaultEnemies)
                {
                    if (!existingEnemiesByName.TryGetValue(defaultEnemy.enemyName, out var assetEnemy) || assetEnemy == null)
                    {
                        assetEnemy = Object.Instantiate(defaultEnemy);
                        assetEnemy.name = defaultEnemy.enemyName;
                        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{EnemyAssetsFolder}/{SanitizeFileName(defaultEnemy.enemyName)}.asset");
                        AssetDatabase.CreateAsset(assetEnemy, assetPath);
                        existingEnemiesByName[defaultEnemy.enemyName] = assetEnemy;
                    }
                    else if (overwriteExistingEnemies)
                    {
                        CopyEnemy(defaultEnemy, assetEnemy);
                        EditorUtility.SetDirty(assetEnemy);
                    }

                    createdEnemies.Add(assetEnemy);
                }

                database.allEnemies = createdEnemies
                    .Where(enemy => enemy != null)
                    .OrderBy(enemy => enemy.enemyName)
                    .ToList();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return database;
            }
            finally
            {
                DestroyTemporaryEnemies(defaultEnemies);
            }
        }

        private static Dictionary<string, EnemyData> LoadExistingEnemiesByName()
        {
            var guids = AssetDatabase.FindAssets("t:EnemyData", new[] { EnemyAssetsFolder });
            var enemies = new Dictionary<string, EnemyData>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (enemy != null && !string.IsNullOrWhiteSpace(enemy.enemyName) && !enemies.ContainsKey(enemy.enemyName))
                    enemies.Add(enemy.enemyName, enemy);
            }

            return enemies;
        }

        private static void DestroyTemporaryEnemies(IEnumerable<EnemyData> enemies)
        {
            if (enemies == null)
                return;

            foreach (var enemy in enemies.Distinct())
            {
                if (enemy != null)
                    Object.DestroyImmediate(enemy);
            }
        }

        private static void CopyEnemy(EnemyData source, EnemyData destination)
        {
            destination.enemyName = source.enemyName;
            destination.name = source.enemyName;
            destination.description = source.description;
            destination.maxHealth = source.maxHealth;
            destination.roundIntents = source.roundIntents == null
                ? new List<EnemyIntentData>()
                : source.roundIntents.Select(CloneIntent).ToList();
            destination.passiveEffects = source.passiveEffects == null
                ? new List<EnemyPassiveEffectData>()
                : source.passiveEffects.Select(ClonePassive).ToList();
        }

        private static EnemyIntentData CloneIntent(EnemyIntentData intent)
        {
            return new EnemyIntentData
            {
                intentType = intent.intentType,
                intentName = intent.intentName,
                description = intent.description,
                magnitude = intent.magnitude,
                count = intent.count,
                triggerOn = intent.triggerOn,
                dieValue = intent.dieValue,
                valueThreshold = intent.valueThreshold,
            };
        }

        private static EnemyPassiveEffectData ClonePassive(EnemyPassiveEffectData passive)
        {
            return new EnemyPassiveEffectData
            {
                description = passive.description,
                effectType = passive.effectType,
                triggerOn = passive.triggerOn,
                dieValue = passive.dieValue,
                valueThreshold = passive.valueThreshold,
                magnitude = passive.magnitude,
                count = passive.count,
            };
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string SanitizeFileName(string value)
        {
            foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(invalidChar, '_');

            return value.Replace("'", string.Empty);
        }
    }
}
