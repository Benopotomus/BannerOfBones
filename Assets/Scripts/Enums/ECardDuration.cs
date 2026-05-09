namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Instant cards fire once when played and are discarded.
    /// Persistent cards apply their effects when played, and recurring effects repeat each round.
    /// Exhaust cards fire once, then are removed from the deck for the rest of combat.
    /// </summary>
    public enum ECardDuration
    {
        Instant,
        Persistent,
        Exhaust,
    }
}
