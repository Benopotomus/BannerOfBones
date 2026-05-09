using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [Serializable]
    public class CardDeckEntry
    {
        public CardData card;

        [Min(0)]
        public int copies = 1;
    }
}
