using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Manages a pool of d6 dice for one combatant. Handles rolling, rerolling, and resizing.
    /// </summary>
    public class DiceManager
    {
        public int DiceCount { get; private set; }

        /// <summary>Current values of all dice in the pool (read-only copy should be used for evaluation).</summary>
        public int[] CurrentRoll { get; private set; }

        public DiceManager(int diceCount)
        {
            DiceCount = Math.Max(1, diceCount);
            CurrentRoll = new int[DiceCount];
        }

        /// <summary>Rolls all dice in the pool.</summary>
        public void RollAll()
        {
            for (int i = 0; i < DiceCount; i++)
                CurrentRoll[i] = Random.Range(1, 7);
        }

        /// <summary>Rerolls every die in the pool.</summary>
        public void RerollAll()
        {
            RollAll();
        }

        /// <summary>
        /// Rerolls up to <paramref name="count"/> dice, targeting the lowest-valued dice first.
        /// </summary>
        public void RerollCount(int count)
        {
            int rerollCount = Math.Min(count, DiceCount);
            int[] indices = GetIndicesSortedAscending();
            for (int i = 0; i < rerollCount; i++)
                CurrentRoll[indices[i]] = Random.Range(1, 7);
        }

        /// <summary>Rerolls the dice at the specified indices.</summary>
        public void RerollAtIndices(int[] indices)
        {
            foreach (int idx in indices)
                if (idx >= 0 && idx < DiceCount)
                    CurrentRoll[idx] = Random.Range(1, 7);
        }

        /// <summary>Rerolls all dice whose current value equals <paramref name="value"/>.</summary>
        public void RerollDiceShowingValue(int value)
        {
            for (int i = 0; i < DiceCount; i++)
                if (CurrentRoll[i] == value)
                    CurrentRoll[i] = Random.Range(1, 7);
        }

        /// <summary>Adds one die to the pool and rolls it immediately.</summary>
        public void AddDie()
        {
            DiceCount++;
            int[] newRoll = new int[DiceCount];
            Array.Copy(CurrentRoll, newRoll, CurrentRoll.Length);
            newRoll[DiceCount - 1] = Random.Range(1, 7);
            CurrentRoll = newRoll;
        }

        /// <summary>Removes the lowest-valued die from the pool. Minimum pool size is 1.</summary>
        public void RemoveDie()
        {
            if (DiceCount <= 1) return;

            int[] indices = GetIndicesSortedAscending();
            int removeIdx = indices[0];

            DiceCount--;
            int[] newRoll = new int[DiceCount];
            int dst = 0;
            for (int i = 0; i <= DiceCount; i++)
            {
                if (i == removeIdx) continue;
                newRoll[dst++] = CurrentRoll[i];
            }
            CurrentRoll = newRoll;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private int[] GetIndicesSortedAscending()
        {
            int[] indices = new int[DiceCount];
            for (int i = 0; i < DiceCount; i++) indices[i] = i;
            Array.Sort(indices, (a, b) => CurrentRoll[a].CompareTo(CurrentRoll[b]));
            return indices;
        }
    }
}