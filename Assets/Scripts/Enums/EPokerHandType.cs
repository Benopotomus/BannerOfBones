namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Describes the dice condition that determines how many times an effect triggers.
    /// The evaluated count is multiplied by the effect's magnitude.
    /// </summary>
    public enum EPokerHandType
    {
        /// <summary>Triggers exactly once regardless of the dice roll.</summary>
        Always,

        /// <summary>Trigger count = number of dice showing exactly dieValue.</summary>
        PerDieValue,

        /// <summary>Trigger count = number of dice showing an odd value (1, 3, 5).</summary>
        PerOddDie,

        /// <summary>Trigger count = number of dice showing an even value (2, 4, 6).</summary>
        PerEvenDie,

        /// <summary>Trigger count = number of dice showing a value >= valueThreshold.</summary>
        PerHighDie,

        /// <summary>Trigger count = number of dice showing a value <= valueThreshold.</summary>
        PerLowDie,

        /// <summary>Trigger count = value of the highest die in the pool.</summary>
        HighestDieValue,

        /// <summary>Trigger count = total pairs in the pool (each group of N matching dice contributes N/2 pairs).</summary>
        PerPair,

        /// <summary>Trigger count = number of groups of exactly 3 matching dice.</summary>
        PerTriple,

        /// <summary>Trigger count = number of groups of exactly 4 matching dice.</summary>
        PerFourOfAKind,

        /// <summary>Trigger count = number of groups of exactly 5 matching dice.</summary>
        PerFiveOfAKind,

        /// <summary>Trigger count = number of distinct die values showing in the pool.</summary>
        PerUniqueDieValue,

        /// <summary>Trigger count = number of dice that match at least one other die value.</summary>
        PerMatchingDie,
    }
}
