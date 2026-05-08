using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Manages the player's draw pile, hand, and discard pile.
    /// </summary>
    public class DeckManager
    {
        public IReadOnlyList<CardData> DrawPile  => _drawPile;
        public IReadOnlyList<CardData> Hand       => _hand;
        public IReadOnlyList<CardData> DiscardPile => _discardPile;

        private readonly List<CardData> _drawPile   = new List<CardData>();
        private readonly List<CardData> _hand        = new List<CardData>();
        private readonly List<CardData> _discardPile = new List<CardData>();

        private readonly int _handSize;

        public DeckManager(List<CardData> deck, int handSize = 5)
        {
            _handSize = handSize;
            _drawPile.AddRange(deck);
            Shuffle(_drawPile);
        }

        /// <summary>Draws up to handSize cards from the draw pile into the hand.</summary>
        public void DrawCards()
        {
            for (int i = 0; i < _handSize; i++)
            {
                if (_drawPile.Count == 0)
                    ShuffleDiscardIntoDrawPile();
                if (_drawPile.Count == 0)
                    break;

                _hand.Add(_drawPile[0]);
                _drawPile.RemoveAt(0);
            }
        }

        /// <summary>
        /// Moves a card from the hand to the discard pile.
        /// Returns false if the card is not in the hand.
        /// </summary>
        public bool PlayCard(CardData card)
        {
            if (!_hand.Contains(card)) return false;
            _hand.Remove(card);
            _discardPile.Add(card);
            return true;
        }

        /// <summary>Moves all cards in the hand to the discard pile.</summary>
        public void DiscardHand()
        {
            _discardPile.AddRange(_hand);
            _hand.Clear();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void ShuffleDiscardIntoDrawPile()
        {
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);
        }

        private static void Shuffle(List<CardData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}