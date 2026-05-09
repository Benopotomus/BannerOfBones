using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BannerOfBones.CardGame.Editor
{
    public static class CardDatabaseEditorUtility
    {
        public const string DatabaseAssetPath = "Assets/Resources/CardDatabase.asset";
        private const string ResourcesFolder = "Assets/Resources";
        private const string CardAssetsFolder = ResourcesFolder + "/CardLibrary";

        [MenuItem("CardGame/Card Database")]
        public static void OpenWindow()
        {
            CardDatabaseWindow.OpenWindow();
        }

        [MenuItem("CardGame/Create Card Database From Defaults")]
        public static void CreateDatabaseFromDefaultsMenu()
        {
            var database = CreateOrUpdateDatabaseFromDefaults(true);
            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
        }

        public static CardDatabase LoadDatabase()
        {
            return AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabaseAssetPath);
        }

        public static CardDatabase CreateOrUpdateDatabaseFromDefaults(bool overwriteExistingCards)
        {
            EnsureFolder(ResourcesFolder);
            EnsureFolder(CardAssetsFolder);

            var database = LoadDatabase();
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CardDatabase>();
                AssetDatabase.CreateAsset(database, DatabaseAssetPath);
            }

            var existingCardsByName = LoadExistingCardsByName();
            var defaultCards = CardCatalog.CreateDefaultCardLibrary();
            var defaultDeck = CardCatalog.CreateDefaultStarterDeck();

            try
            {
                var createdCards = new List<CardData>(defaultCards.Count);

                foreach (var defaultCard in defaultCards)
                {
                    if (!existingCardsByName.TryGetValue(defaultCard.cardName, out var assetCard) || assetCard == null)
                    {
                        assetCard = Object.Instantiate(defaultCard);
                        assetCard.name = defaultCard.cardName;
                        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{CardAssetsFolder}/{SanitizeFileName(defaultCard.cardName)}.asset");
                        AssetDatabase.CreateAsset(assetCard, assetPath);
                        existingCardsByName[defaultCard.cardName] = assetCard;
                    }
                    else if (overwriteExistingCards)
                    {
                        CopyCard(defaultCard, assetCard);
                        EditorUtility.SetDirty(assetCard);
                    }

                    createdCards.Add(assetCard);
                }

                database.allCards = createdCards;
                database.starterDeck = overwriteExistingCards || database.starterDeck == null || database.starterDeck.Count == 0
                    ? BuildStarterDeckEntries(defaultDeck, existingCardsByName)
                    : PreserveStarterDeckEntries(database.starterDeck, createdCards);
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return database;
            }
            finally
            {
                DestroyTemporaryCards(defaultCards);
                DestroyTemporaryCards(defaultDeck);
            }
        }

        private static Dictionary<string, CardData> LoadExistingCardsByName()
        {
            var guids = AssetDatabase.FindAssets("t:CardData", new[] { CardAssetsFolder });
            var cards = new Dictionary<string, CardData>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null && !string.IsNullOrWhiteSpace(card.cardName) && !cards.ContainsKey(card.cardName))
                    cards.Add(card.cardName, card);
            }

            return cards;
        }

        private static List<CardDeckEntry> BuildStarterDeckEntries(IEnumerable<CardData> defaultDeck, IReadOnlyDictionary<string, CardData> cardsByName)
        {
            var counts = new Dictionary<string, int>();
            foreach (var card in defaultDeck)
            {
                if (card == null || string.IsNullOrWhiteSpace(card.cardName))
                    continue;

                counts.TryGetValue(card.cardName, out var currentCount);
                counts[card.cardName] = currentCount + 1;
            }

            return counts
                .Select(kvp => new CardDeckEntry
                {
                    card = cardsByName.TryGetValue(kvp.Key, out var card) ? card : null,
                    copies = kvp.Value,
                })
                .Where(entry => entry.card != null)
                .OrderBy(entry => entry.card.cardName)
                .ToList();
        }


        private static List<CardDeckEntry> PreserveStarterDeckEntries(IEnumerable<CardDeckEntry> existingEntries, IEnumerable<CardData> orderedCards)
        {
            var countsByName = existingEntries
                .Where(entry => entry?.card != null)
                .GroupBy(entry => entry.card.cardName)
                .ToDictionary(group => group.Key, group => Mathf.Max(0, group.First().copies));

            return orderedCards
                .Select(card => new CardDeckEntry
                {
                    card = card,
                    copies = countsByName.TryGetValue(card.cardName, out var copies) ? copies : 0,
                })
                .ToList();
        }

        private static void DestroyTemporaryCards(IEnumerable<CardData> cards)
        {
            if (cards == null)
                return;

            foreach (var card in cards.Distinct())
            {
                if (card != null)
                    Object.DestroyImmediate(card);
            }
        }

        private static void CopyCard(CardData source, CardData destination)
        {
            destination.cardName = source.cardName;
            destination.name = source.cardName;
            destination.description = source.description;
            destination.energyCost = source.energyCost;
            destination.duration = source.duration;
            destination.targetsAllEnemies = source.targetsAllEnemies;
            destination.effects = source.effects == null
                ? new List<CardEffectData>()
                : source.effects.Select(CloneEffect).ToList();
        }

        private static CardEffectData CloneEffect(CardEffectData effect)
        {
            return new CardEffectData
            {
                effectType = effect.effectType,
                diceTarget = effect.diceTarget,
                triggerOn = effect.triggerOn,
                dieValue = effect.dieValue,
                valueThreshold = effect.valueThreshold,
                magnitude = effect.magnitude,
                altMagnitude = effect.altMagnitude,
                count = effect.count,
                drawCount = effect.drawCount,
                dieSides = effect.dieSides,
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
