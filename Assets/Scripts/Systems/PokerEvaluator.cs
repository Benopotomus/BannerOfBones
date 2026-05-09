namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Stateless utility for evaluating poker-dice hands from an integer array of die values (1–6).
    /// </summary>
    public static class PokerEvaluator
    {
        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// General-purpose trigger evaluator. Returns how many times the given condition
        /// is satisfied by the dice array.
        /// </summary>
        public static int EvaluateTriggerCount(EPokerHandType handType, int[] dice, int dieValue = 0, int valueThreshold = 0)
        {
            switch (handType)
            {
                case EPokerHandType.Always:           return 1;
                case EPokerHandType.PerDieValue:      return CountValue(dice, dieValue);
                case EPokerHandType.PerOddDie:        return CountOddDice(dice);
                case EPokerHandType.PerEvenDie:       return CountEvenDice(dice);
                case EPokerHandType.PerHighDie:       return CountHighDice(dice, valueThreshold);
                case EPokerHandType.PerLowDie:        return CountLowDice(dice, valueThreshold);
                case EPokerHandType.HighestDieValue:  return HighestDie(dice);
                case EPokerHandType.PerPair:          return CountPairs(dice);
                case EPokerHandType.PerTriple:        return CountTriples(dice);
                case EPokerHandType.PerFullHouse:     return CountFullHouses(dice);
                case EPokerHandType.PerFourOfAKind:   return CountFourOfAKind(dice);
                case EPokerHandType.PerFiveOfAKind:   return CountFiveOfAKind(dice);
                case EPokerHandType.PerUniqueDieValue: return CountUniqueDieValues(dice);
                case EPokerHandType.IfStraight:       return HasStraight(dice) ? 1 : 0;
                default:                              return 0;
            }
        }

        /// <summary>Returns the number of dice showing exactly <paramref name="value"/>.</summary>
        public static int CountValue(int[] dice, int value)
        {
            int count = 0;
            foreach (int d in dice)
                if (d == value) count++;
            return count;
        }

        /// <summary>
        /// Returns the total number of pairs across all die values.
        /// Each group of N matching dice contributes N/2 pairs (integer division),
        /// so a triple counts as 1 pair and a four-of-a-kind counts as 2 pairs.
        /// Works for any die type (d4–d20).
        /// </summary>
        public static int CountPairs(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            int pairs = 0;
            for (int i = 1; i < freq.Length; i++)
                pairs += freq[i] / 2;
            return pairs;
        }

        /// <summary>
        /// Returns the number of groups of exactly 3 matching dice.
        /// A four-of-a-kind does NOT count as a triple. Works for any die type.
        /// </summary>
        public static int CountTriples(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            int triples = 0;
            for (int i = 1; i < freq.Length; i++)
                if (freq[i] == 3) triples++;
            return triples;
        }

        /// <summary>
        /// Returns 1 if the dice contain both exactly one triple and exactly one pair (full house),
        /// otherwise 0. Works for any die type.
        /// </summary>
        public static int CountFullHouses(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            bool hasTriple = false, hasPair = false;
            for (int i = 1; i < freq.Length; i++)
            {
                if (freq[i] == 3) hasTriple = true;
                else if (freq[i] == 2) hasPair = true;
            }
            return (hasTriple && hasPair) ? 1 : 0;
        }

        /// <summary>Returns the number of groups of exactly 4 matching dice. Works for any die type.</summary>
        public static int CountFourOfAKind(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            int count = 0;
            for (int i = 1; i < freq.Length; i++)
                if (freq[i] == 4) count++;
            return count;
        }

        /// <summary>Returns the number of groups of exactly 5 matching dice (Yahtzee). Works for any die type.</summary>
        public static int CountFiveOfAKind(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            int count = 0;
            for (int i = 1; i < freq.Length; i++)
                if (freq[i] == 5) count++;
            return count;
        }

        /// <summary>
        /// Returns true if the pool of 5+ dice contains a straight (1-2-3-4-5 or 2-3-4-5-6).
        /// Returns false if fewer than 5 dice are present.
        /// </summary>
        public static bool HasStraight(int[] dice)
        {
            if (dice.Length < 5) return false;
            int[] freq = GetFrequencies(dice);
            bool low  = freq[1] >= 1 && freq[2] >= 1 && freq[3] >= 1 && freq[4] >= 1 && freq[5] >= 1;
            bool high = freq[2] >= 1 && freq[3] >= 1 && freq[4] >= 1 && freq[5] >= 1 && freq[6] >= 1;
            return low || high;
        }

        /// <summary>Returns the number of dice showing an odd value (1, 3, 5).</summary>
        public static int CountOddDice(int[] dice)
        {
            int count = 0;
            foreach (int d in dice)
                if (d % 2 == 1) count++;
            return count;
        }

        /// <summary>Returns the number of dice showing an even value (2, 4, 6).</summary>
        public static int CountEvenDice(int[] dice)
        {
            int count = 0;
            foreach (int d in dice)
                if (d % 2 == 0) count++;
            return count;
        }

        /// <summary>Returns the number of dice showing a value >= <paramref name="threshold"/>.</summary>
        public static int CountHighDice(int[] dice, int threshold)
        {
            int count = 0;
            foreach (int d in dice)
                if (d >= threshold) count++;
            return count;
        }

        /// <summary>Returns the number of dice showing a value <= <paramref name="threshold"/>.</summary>
        public static int CountLowDice(int[] dice, int threshold)
        {
            int count = 0;
            foreach (int d in dice)
                if (d <= threshold) count++;
            return count;
        }

        /// <summary>Returns the highest value among all dice, or 0 if the array is empty.</summary>
        public static int HighestDie(int[] dice)
        {
            int max = 0;
            foreach (int d in dice)
                if (d > max) max = d;
            return max;
        }

        /// <summary>Returns the number of distinct values present in the dice pool. Works for any die type.</summary>
        public static int CountUniqueDieValues(int[] dice)
        {
            int[] freq = GetFrequencies(dice);
            int unique = 0;
            for (int i = 1; i < freq.Length; i++)
                if (freq[i] > 0) unique++;
            return unique;
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns a frequency array large enough to hold every distinct die value present.
        /// freq[n] = number of dice showing value n. Supports values up to d20.
        /// </summary>
        private static int[] GetFrequencies(int[] dice)
        {
            int max = 6;
            foreach (int d in dice)
                if (d > max) max = d;
            int[] freq = new int[max + 1];
            foreach (int d in dice)
                if (d >= 1) freq[d]++;
            return freq;
        }
    }
}