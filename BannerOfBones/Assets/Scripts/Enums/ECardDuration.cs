namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Instant cards fire once when played and are discarded.
    /// Persistent cards apply their effects when played, and recurring effects repeat each round.
    /// </summary>
    public enum ECardDuration
    {
        Instant,
        Persistent,
    }
}