using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "CardGame/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        public const string ResourcesPath = "CardDatabase";

        public List<CardData> allCards = new List<CardData>();
        public List<CardDeckEntry> starterDeck = new List<CardDeckEntry>();

        public bool HasCards => allCards != null && allCards.Count > 0;
        public bool HasStarterDeck => starterDeck != null && starterDeck.Count > 0;

        public List<CardData> CreateAllRuntimeCards()
        {
            var cards = new List<CardData>();
            if (allCards == null)
                return cards;

            foreach (var card in allCards)
            {
                if (card != null)
                    cards.Add(CloneCard(card));
            }

            return cards;
        }

        public List<CardData> CreateStarterDeckRuntime()
        {
            var cards = new List<CardData>();
            if (starterDeck == null)
                return cards;

            foreach (var entry in starterDeck)
            {
                if (entry?.card == null || entry.copies <= 0)
                    continue;

                for (int i = 0; i < entry.copies; i++)
                    cards.Add(CloneCard(entry.card));
            }

            return cards;
        }

        private static CardData CloneCard(CardData source)
        {
            var clone = Instantiate(source);
            clone.name = source.cardName;
            return clone;
        }
    }
}
