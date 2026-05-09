using System;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Represents a single die in a dice pool.
    /// Each die can be of a different standard type (d4, d6, d8, d10, d12, or d20)
    /// and may be flagged as temporary — meaning it is automatically removed at the
    /// start of the next round after it was added.
    /// </summary>
    [Serializable]
    public struct Die
    {
        /// <summary>Number of faces on this die. Valid tiers: 4, 6, 8, 10, 12, 20.</summary>
        public int Sides;

        /// <summary>The value produced by the last roll. 0 if this die has not been rolled yet.</summary>
        public int Value;

        /// <summary>
        /// When true this die expires at the start of the next round and is removed from
        /// the pool before rolling. Useful for one-round empowerment effects.
        /// </summary>
        public bool IsTemporary;

        public Die(int sides, bool isTemporary = false)
        {
            Sides = sides;
            Value = 0;
            IsTemporary = isTemporary;
        }

        /// <summary>Human-readable label showing the die type, e.g. "d8". Appends "*" for temporary dice.</summary>
        public string TypeLabel => IsTemporary ? $"d{Sides}*" : $"d{Sides}";
    }
}
