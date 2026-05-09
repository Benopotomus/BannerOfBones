namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Runtime state for a persistent card that may keep a chosen enemy target.
    /// </summary>
    public class PersistentCardRuntime
    {
        public CardData Card { get; }
        public int TargetEnemyIndex { get; }
        public bool TargetsAllEnemies { get; }

        public PersistentCardRuntime(CardData card, int targetEnemyIndex, bool targetsAllEnemies)
        {
            Card = card;
            TargetEnemyIndex = targetEnemyIndex;
            TargetsAllEnemies = targetsAllEnemies;
        }
    }
}
