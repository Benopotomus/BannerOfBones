using System;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Seeded random-number generator for Banner of Bones.
    /// All dice rolls, deck shuffles, and other randomised events are driven
    /// through this class so that a given seed always produces an identical
    /// sequence of outcomes.
    ///
    /// Call <see cref="Init"/> (or <see cref="InitRandom"/>) once before
    /// combat begins, then use <see cref="Range"/> wherever a random integer
    /// is needed.
    /// </summary>
    public static class BoBRandom
    {
        private static System.Random _rng;

        /// <summary>The seed that was last used to initialise the RNG.</summary>
        public static int Seed { get; private set; }

        /// <summary>Whether the RNG has been initialised with a seed.</summary>
        public static bool IsSeeded { get; private set; }

        /// <summary>
        /// Initialises the RNG with a specific <paramref name="seed"/>.
        /// The same seed always produces the same sequence of results.
        /// </summary>
        public static void Init(int seed)
        {
            Seed      = seed;
            _rng      = new System.Random(seed);
            IsSeeded  = true;
            Debug.Log($"[BoBRandom] Seeded with {seed}");
        }

        /// <summary>
        /// Initialises the RNG with a random seed derived from the current time.
        /// The chosen seed is stored in <see cref="Seed"/> so it can be logged or replayed.
        /// </summary>
        public static void InitRandom()
        {
            Init(Environment.TickCount);
        }

        /// <summary>
        /// Returns a random integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).
        /// Behaviour mirrors <c>UnityEngine.Random.Range(int, int)</c>.
        /// </summary>
        public static int Range(int minInclusive, int maxExclusive)
        {
            EnsureInit();
            return _rng.Next(minInclusive, maxExclusive);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private static void EnsureInit()
        {
            if (_rng != null) return;
            // Fall back to a random seed if Init was never called.
            InitRandom();
        }
    }
}
