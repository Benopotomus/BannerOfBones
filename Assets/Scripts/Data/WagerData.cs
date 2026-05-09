namespace BannerOfBones.CardGame
{
    /// <summary>
    /// One-shot delayed payoff created by cards that resolve at the start of the next round.
    /// </summary>
    public class WagerData
    {
        public string SourceName { get; }
        public EPokerHandType TriggerOn { get; }
        public ECardTarget DiceTarget { get; }
        public int DieValue { get; }
        public int ValueThreshold { get; }
        public int Magnitude { get; }

        public WagerData(string sourceName, CardEffectData effect)
        {
            SourceName = sourceName;
            TriggerOn = effect.triggerOn;
            DiceTarget = effect.diceTarget;
            DieValue = effect.dieValue;
            ValueThreshold = effect.valueThreshold;
            Magnitude = effect.magnitude;
        }
    }
}
