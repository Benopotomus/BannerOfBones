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
        public IReadOnlyList<CardData> ExhaustPile => _exhaustPile;
        public CardData RetainedCard { get; private set; }

        private readonly List<CardData> _drawPile   = new List<CardData>();
        private readonly List<CardData> _hand        = new List<CardData>();
        private readonly List<CardData> _discardPile = new List<CardData>();
        private readonly List<CardData> _exhaustPile = new List<CardData>();

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
            while (_hand.Count < _handSize)
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
            ClearRetainedCard(card);
            _hand.Remove(card);
            _discardPile.Add(card);
            return true;
        }

        /// <summary>
        /// Moves a card from the hand to the exhaust pile.
        /// Returns false if the card is not in the hand.
        /// </summary>
        public bool ExhaustCard(CardData card)
        {
            if (!_hand.Contains(card)) return false;
            ClearRetainedCard(card);
            _hand.Remove(card);
            _exhaustPile.Add(card);
            return true;
        }

        /// <summary>
        /// Discards a card from the player's hand without playing it.
        /// </summary>
        public bool DiscardCardFromHand(CardData card)
        {
            if (!_hand.Contains(card)) return false;
            ClearRetainedCard(card);
            _hand.Remove(card);
            _discardPile.Add(card);
            return true;
        }

        /// <summary>
        /// Marks a single card to stay in hand between rounds.
        /// Passing null clears the current retained selection.
        /// </summary>
        public void SetRetainedCard(CardData card)
        {
            RetainedCard = card != null && _hand.Contains(card) ? card : null;
        }

        /// <summary>Moves all cards in the hand to the discard pile.</summary>
        public void DiscardHand()
        {
            ClearRetainedCard();
            _discardPile.AddRange(_hand);
            _hand.Clear();
        }

        /// <summary>
        /// Discards the hand while leaving the retained card in place for next round.
        /// </summary>
        public void DiscardHandExceptRetained()
        {
            for (int i = _hand.Count - 1; i >= 0; i--)
            {
                var card = _hand[i];
                if (card == RetainedCard) continue;
                _discardPile.Add(card);
                _hand.RemoveAt(i);
            }
        }

        /// <summary>Draws a fixed number of cards without refilling to hand size.</summary>
        public void DrawSpecificCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_drawPile.Count == 0)
                    ShuffleDiscardIntoDrawPile();
                if (_drawPile.Count == 0)
                    break;

                _hand.Add(_drawPile[0]);
                _drawPile.RemoveAt(0);
            }
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

        private void ClearRetainedCard(CardData card = null)
        {
            if (card == null || RetainedCard == card)
                RetainedCard = null;
        }
    }
}
