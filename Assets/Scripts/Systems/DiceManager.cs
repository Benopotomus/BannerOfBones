using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Manages a pool of dice for one combatant.
    /// Individual dice can be of different standard types (d4, d6, d8, d10, d12, d20).
    /// Temporary dice are removed automatically at the start of the next round before rolling.
    /// </summary>
    public class DiceManager
    {
        /// <summary>Number of sides on the default (starting) die type.</summary>
        public const int DefaultDieSides = 6;

        /// <summary>Standard die-type tiers in ascending order of power.</summary>
        public static readonly int[] DieTiers = { 4, DefaultDieSides, 8, 10, 12, 20 };

        private readonly List<Die> _dice = new List<Die>();

        /// <summary>Number of dice currently in the pool (including temporary dice).</summary>
        public int DiceCount => _dice.Count;

        /// <summary>Read-only view of the individual dice, including type and temporary flags.</summary>
        public IReadOnlyList<Die> Pool => _dice;

        /// <summary>
        /// Current rolled values of all dice in the pool.
        /// This is a freshly-allocated array on each access; cache if calling frequently.
        /// </summary>
        public int[] CurrentRoll
        {
            get
            {
                var roll = new int[_dice.Count];
                for (int i = 0; i < _dice.Count; i++)
                    roll[i] = _dice[i].Value;
                return roll;
            }
        }

        /// <summary>Creates a pool of <paramref name="diceCount"/> dice, all of the given <paramref name="sides"/> type.</summary>
        public DiceManager(int diceCount, int sides = DefaultDieSides)
        {
            sides = ClampToValidTier(sides);
            int count = Math.Max(1, diceCount);
            for (int i = 0; i < count; i++)
                _dice.Add(new Die(sides));
        }

        // ── Rolling ───────────────────────────────────────────────────────────────

        /// <summary>Rolls all dice in the pool, each according to its own number of sides.</summary>
        public void RollAll()
        {
            for (int i = 0; i < _dice.Count; i++)
            {
                var d = _dice[i];
                d.Value = Random.Range(1, d.Sides + 1);
                _dice[i] = d;
            }
        }

        /// <summary>Rerolls every die in the pool.</summary>
        public void RerollAll() => RollAll();

        /// <summary>
        /// Rerolls up to <paramref name="count"/> dice, targeting the lowest-valued dice first.
        /// </summary>
        public void RerollCount(int count)
        {
            int n = Math.Min(count, _dice.Count);
            int[] indices = GetIndicesSortedAscendingByValue();
            for (int i = 0; i < n; i++)
                RerollIndex(indices[i]);
        }

        /// <summary>Rerolls the dice at the specified indices.</summary>
        public void RerollAtIndices(int[] indices)
        {
            foreach (int idx in indices)
                RerollIndex(idx);
        }

        /// <summary>Rerolls all dice whose current value equals <paramref name="value"/>.</summary>
        public void RerollDiceShowingValue(int value)
        {
            for (int i = 0; i < _dice.Count; i++)
                if (_dice[i].Value == value)
                    RerollIndex(i);
        }

        // ── Pool Sizing ───────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a die of the given type and rolls it immediately.
        /// Pass <paramref name="temporary"/> = true for a die that expires at the start of the next round.
        /// </summary>
        public void AddDie(int sides = DefaultDieSides, bool temporary = false)
        {
            sides = ClampToValidTier(sides);
            var d = new Die(sides, temporary);
            d.Value = Random.Range(1, sides + 1);
            _dice.Add(d);
        }

        /// <summary>Removes the lowest-valued die from the pool. Minimum pool size is 1.</summary>
        public void RemoveDie()
        {
            if (_dice.Count <= 1) return;
            _dice.RemoveAt(GetIndicesSortedAscendingByValue()[0]);
        }

        /// <summary>Removes all temporary dice from the pool before a new round begins.
        /// Guarantees at least 1 permanent die remains.
        /// </summary>
        public void RemoveTemporaryDice()
        {
            _dice.RemoveAll(d => d.IsTemporary);
            if (_dice.Count == 0)
                _dice.Add(new Die(DefaultDieSides));
        }

        // ── Die Upgrades / Downgrades ─────────────────────────────────────────────

        /// <summary>Advances the die at <paramref name="index"/> one step up the tier ladder (max d20).</summary>
        public void UpgradeDie(int index)
        {
            if (index < 0 || index >= _dice.Count) return;
            var d = _dice[index];
            d.Sides = NextTier(d.Sides);
            d.Value = Math.Min(d.Value, d.Sides);
            _dice[index] = d;
        }

        /// <summary>Moves the die at <paramref name="index"/> one step down the tier ladder (min d4).</summary>
        public void DowngradeDie(int index)
        {
            if (index < 0 || index >= _dice.Count) return;
            var d = _dice[index];
            d.Sides = PrevTier(d.Sides);
            d.Value = Math.Min(d.Value, d.Sides);
            _dice[index] = d;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void RerollIndex(int idx)
        {
            if (idx < 0 || idx >= _dice.Count) return;
            var d = _dice[idx];
            d.Value = Random.Range(1, d.Sides + 1);
            _dice[idx] = d;
        }

        private int[] GetIndicesSortedAscendingByValue()
        {
            var indices = new int[_dice.Count];
            for (int i = 0; i < _dice.Count; i++) indices[i] = i;
            Array.Sort(indices, (a, b) => _dice[a].Value.CompareTo(_dice[b].Value));
            return indices;
        }

        private static int NextTier(int sides)
        {
            for (int i = 0; i < DieTiers.Length - 1; i++)
                if (DieTiers[i] == sides) return DieTiers[i + 1];
            return sides; // already at max (d20)
        }

        private static int PrevTier(int sides)
        {
            for (int i = DieTiers.Length - 1; i > 0; i--)
                if (DieTiers[i] == sides) return DieTiers[i - 1];
            return sides; // already at min (d4)
        }

        private static int ClampToValidTier(int sides)
        {
            int closest = DieTiers[0];
            int minDist = Math.Abs(sides - closest);
            foreach (int tier in DieTiers)
            {
                int dist = Math.Abs(sides - tier);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = tier;
                }
            }
            return closest;
        }
    }
}