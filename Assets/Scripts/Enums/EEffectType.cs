namespace BannerOfBones.CardGame
{
    public enum EEffectType
    {
        /// <summary>Deal (magnitude * triggerCount) damage to the enemy.</summary>
        DealDamage,

        /// <summary>
        /// If trigger condition is met, deal magnitude damage to enemy.
        /// Otherwise the player takes altMagnitude damage.
        /// </summary>
        ConditionalDamage,

        /// <summary>Player takes magnitude damage.</summary>
        SelfDamage,

        /// <summary>Player gains (magnitude * triggerCount) block.</summary>
        GainBlock,

        /// <summary>Reroll 'count' dice from the target pool (lowest-value dice first).</summary>
        RerollDice,

        /// <summary>Reroll all dice in the target pool.</summary>
        RerollAllDice,

        /// <summary>Reroll all dice in the target pool that show exactly dieValue.</summary>
        RerollByValue,

        /// <summary>Add one die to the target pool and roll it immediately.</summary>
        AddDie,

        /// <summary>Remove one die from the target pool.</summary>
        RemoveDie,

        /// <summary>
        /// Add one die of the specified type to the target pool for this round only.
        /// The die is removed automatically at the start of the next round before rolling.
        /// </summary>
        AddTemporaryDie,

        /// <summary>
        /// Upgrade one chosen die in the target pool to the next standard tier (d4 → d6 → d8 → d10 → d12).
        /// Requires the player to select which die to upgrade.
        /// </summary>
        UpgradeDie,

        /// <summary>
        /// Downgrade one chosen die in the target pool to the previous standard tier.
        /// Requires the player to select which die to downgrade.
        /// </summary>
        DowngradeDie,

        /// <summary>Discard another card from hand, then draw additional cards.</summary>
        CycleHand,

        /// <summary>Queue a one-shot delayed payoff that resolves at the start of the next round.</summary>
        AddWager,

        /// <summary>Player gains (magnitude * triggerCount) energy, capped at max energy.</summary>
        GainEnergy,

        /// <summary>Draw (magnitude * triggerCount) cards.</summary>
        DrawCards,
    }
}
