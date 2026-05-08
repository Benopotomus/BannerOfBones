using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    [Serializable]
    public class CardEffectData
    {
        [Tooltip("What this effect does.")]
        public EEffectType effectType;

        [Tooltip("Which dice pool this effect evaluates.")]
        public ECardTarget diceTarget;

        [Tooltip("The poker-dice condition that determines how many times the effect triggers.")]
        public EPokerHandType triggerOn;

        [Tooltip("Exact die face value for PerDieValue and RerollByValue conditions.")]
        public int dieValue;

        [Tooltip("Threshold for PerHighDie (>= threshold) and PerLowDie (<= threshold) conditions.")]
        public int valueThreshold;

        [Tooltip("Primary magnitude: damage dealt or block gained per trigger.")]
        public int magnitude;

        [Tooltip("Alternate magnitude used by ConditionalDamage when the condition is NOT met (self damage).")]
        public int altMagnitude;

        [Tooltip("Number of dice to reroll for RerollDice effects.")]
        public int count;
    }
}