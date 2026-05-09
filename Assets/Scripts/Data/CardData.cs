using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [CreateAssetMenu(fileName = "Card", menuName = "CardGame/Card")]
    public class CardData : ScriptableObject
    {
        public string cardName;

        [TextArea(2, 5)]
        public string description;

        [Range(0, 5)]
        public int energyCost;

        public ECardDuration duration;
        public bool targetsAllEnemies;

        public List<CardEffectData> effects = new List<CardEffectData>();
    }
}
