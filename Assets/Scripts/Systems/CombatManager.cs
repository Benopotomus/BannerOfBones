using System;
using System.Collections.Generic;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Drives a single combat encounter between the player and one enemy.
    /// Handles card play, baseline actions, retained cards, wagers, and player-choice prompts.
    /// </summary>
    public class CombatManager
    {
        private enum PendingHandAction
        {
            None,
            Scout,
            Cycle,
        }

        public PlayerCombatant Player { get; private set; }
        public EnemyCombatant Enemy { get; private set; }
        public ECombatState State { get; private set; } = ECombatState.Idle;

        public bool IsAwaitingDiceSelection => _pendingDiceResolver != null;
        public bool IsAwaitingHandSelection => _pendingHandAction != PendingHandAction.None;
        public bool IsSelectingRetain { get; private set; }
        public bool HasPendingChoice => IsAwaitingDiceSelection || IsAwaitingHandSelection || IsSelectingRetain;
        public string PendingPrompt { get; private set; }
        public int SelectedDiceCount => _selectedDiceIndices.Count;
        public int PendingDiceSelectionLimit => _pendingDiceSelectionLimit;

        /// <summary>Fired whenever the combat state changes or a prompt updates.</summary>
        public event Action<ECombatState> OnStateChanged;

        /// <summary>Fired at the start of every new round after setup is complete.</summary>
        public event Action OnRoundStarted;

        /// <summary>Fired when combat ends. Parameter is true if the player won.</summary>
        public event Action<bool> OnCombatEnded;

        /// <summary>Fired for combat log output.</summary>
        public event Action<string> OnLogMessage;

        private readonly List<int> _selectedDiceIndices = new List<int>();
        private readonly List<CardEffectData> _resolvingEffects = new List<CardEffectData>();
        private PendingHandAction _pendingHandAction = PendingHandAction.None;
        private Action<int[]> _pendingDiceResolver;
        private ECardTarget _pendingDiceTarget;
        private int _pendingDiceSelectionLimit;
        private int _pendingDrawCount;
        private int _resolvingEffectIndex;
        private string _resolvingSourceName;

        /// <summary>Initialises combat and begins the first round.</summary>
        public void StartCombat(EnemyData enemyData, List<CardData> playerDeck,
                                int playerHealth = 30, int playerEnergy = 3)
        {
            Player = new PlayerCombatant(playerHealth, playerEnergy, playerDeck);
            Enemy = new EnemyCombatant(enemyData);
            BeginRound();
        }

        public bool CanPlayCard(CardData card)
        {
            return State == ECombatState.PlayerTurn
                   && !HasPendingChoice
                   && card != null
                   && Player.Deck.Hand.Contains(card)
                   && Player.Energy.CanAfford(card.energyCost);
        }

        public bool CanUseBaselineActions()
        {
            return State == ECombatState.PlayerTurn && !HasPendingChoice;
        }

        public bool CanSelectHandCard(CardData card)
        {
            if (State != ECombatState.PlayerTurn || card == null || !Player.Deck.Hand.Contains(card))
                return false;

            return IsAwaitingHandSelection || IsSelectingRetain || CanPlayCard(card);
        }

        public bool CanSelectDie(ECardTarget target, int dieIndex)
        {
            if (!IsAwaitingDiceSelection || target != _pendingDiceTarget)
                return false;

            int dieCount = target == ECardTarget.PlayerDice
                ? Player.Dice.DiceCount
                : Enemy.Dice.DiceCount;

            return dieIndex >= 0 && dieIndex < dieCount;
        }

        public bool IsDieSelected(ECardTarget target, int dieIndex)
        {
            return target == _pendingDiceTarget && _selectedDiceIndices.Contains(dieIndex);
        }

        /// <summary>
        /// Handles the current meaning of a hand-card click: play, discard for scout/cycle, or retain select.
        /// </summary>
        public bool TryHandleHandCardClick(CardData card)
        {
            if (State != ECombatState.PlayerTurn || card == null) return false;
            if (IsAwaitingDiceSelection) return false;

            if (IsAwaitingHandSelection)
                return ResolvePendingHandSelection(card);

            if (IsSelectingRetain)
            {
                if (!Player.Deck.Hand.Contains(card)) return false;
                Player.Deck.SetRetainedCard(Player.Deck.RetainedCard == card ? null : card);
                IsSelectingRetain = false;
                PendingPrompt = null;
                NotifyStateChanged();
                return true;
            }

            return TryPlayCard(card);
        }

        public bool TryPlayCard(CardData card)
        {
            if (!CanPlayCard(card)) return false;
            if (!Player.Energy.TrySpendEnergy(card.energyCost)) return false;

            switch (card.duration)
            {
                case ECardDuration.Exhaust:
                    Player.Deck.ExhaustCard(card);
                    break;
                default:
                    Player.Deck.PlayCard(card);
                    break;
            }

            if (card.duration == ECardDuration.Persistent)
                Player.ActivePersistentCards.Add(card);

            Log($"Played {card.cardName}.");
            BeginEffectSequence(card.cardName, card.effects);
            return true;
        }

        public bool TryUseFocus()
        {
            if (!CanUseBaselineActions() || !Player.Energy.TrySpendEnergy(1))
                return false;

            QueueDiceSelection(ECardTarget.PlayerDice, 1, "Focus: choose 1 player die to reroll.", indices =>
            {
                Player.Dice.RerollAtIndices(indices);
                Log("Focus rerolled 1 player die.");
            });
            return true;
        }

        public bool TryUseBrace()
        {
            if (!CanUseBaselineActions() || !Player.Energy.TrySpendEnergy(1))
                return false;

            Player.GainBlock(2);
            Log("Brace gained 2 block.");
            NotifyStateChanged();
            return true;
        }

        public bool TryUseScout()
        {
            if (!CanUseBaselineActions()
                || Player.Deck.Hand.Count == 0
                || !Player.Energy.TrySpendEnergy(2))
                return false;

            QueueHandSelection(PendingHandAction.Scout, 2, "Scout: choose 1 card to discard, then draw 2.");
            return true;
        }

        public bool ToggleRetainSelection()
        {
            if (State != ECombatState.PlayerTurn || IsAwaitingDiceSelection || IsAwaitingHandSelection || Player.Deck.Hand.Count == 0)
                return false;

            IsSelectingRetain = !IsSelectingRetain;
            PendingPrompt = IsSelectingRetain ? "Choose 1 card to retain for next round." : null;
            NotifyStateChanged();
            return true;
        }

        public bool TogglePendingDieSelection(ECardTarget target, int dieIndex)
        {
            if (!CanSelectDie(target, dieIndex)) return false;

            if (_selectedDiceIndices.Contains(dieIndex))
            {
                _selectedDiceIndices.Remove(dieIndex);
            }
            else
            {
                if (_selectedDiceIndices.Count >= _pendingDiceSelectionLimit)
                    return false;

                _selectedDiceIndices.Add(dieIndex);
            }

            NotifyStateChanged();

            if (_pendingDiceSelectionLimit == 1 && _selectedDiceIndices.Count == 1)
                ConfirmPendingDiceSelection();

            return true;
        }

        public bool ConfirmPendingDiceSelection()
        {
            if (!IsAwaitingDiceSelection || _selectedDiceIndices.Count == 0)
                return false;

            var selected = _selectedDiceIndices.ToArray();
            Array.Sort(selected);

            var resolver = _pendingDiceResolver;
            ClearPendingDiceSelection();
            resolver?.Invoke(selected);

            ContinueEffectSequence();
            return true;
        }

        /// <summary>Ends the player's turn and triggers enemy damage resolution.</summary>
        public void EndPlayerTurn()
        {
            if (State != ECombatState.PlayerTurn || HasPendingChoice) return;
            ExecuteEnemyTurn();
        }

        private void BeginRound()
        {
            ClearAllPrompts();

            Player.StartRound();
            Enemy.StartRound();

            Enemy.ApplyPreRoundEffects();
            ApplyPersistentCardEffects();
            if (CheckForCombatEnd()) return;

            ResolveWagers();
            if (CheckForCombatEnd()) return;

            SetState(ECombatState.PlayerTurn);
            OnRoundStarted?.Invoke();
        }

        private void ApplyPersistentCardEffects()
        {
            foreach (var card in Player.ActivePersistentCards)
            {
                foreach (var effect in card.effects)
                {
                    if (effect.effectType == EEffectType.AddDie
                        || effect.effectType == EEffectType.RemoveDie
                        || effect.effectType == EEffectType.CycleHand
                        || effect.effectType == EEffectType.AddWager)
                    {
                        continue;
                    }

                    CardEffectProcessor.ProcessEffect(effect, Player, Enemy);
                }
            }
        }

        private void ResolveWagers()
        {
            if (Player.ActiveWagers.Count == 0) return;

            var wagers = new List<WagerData>(Player.ActiveWagers);
            Player.ActiveWagers.Clear();

            foreach (var wager in wagers)
            {
                int[] dice = wager.DiceTarget == ECardTarget.PlayerDice
                    ? Player.Dice.CurrentRoll
                    : Enemy.Dice.CurrentRoll;

                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    wager.TriggerOn, dice, wager.DieValue, wager.ValueThreshold);

                if (triggers > 0)
                {
                    Enemy.TakeDamage(wager.Magnitude);
                    Log($"{wager.SourceName} pays off for {wager.Magnitude} damage.");
                }
                else
                {
                    Log($"{wager.SourceName} whiffs this round.");
                }
            }
        }

        private void ExecuteEnemyTurn()
        {
            SetState(ECombatState.EnemyTurn);

            int damage = Enemy.CalculateDamage();
            Player.TakeDamage(damage);
            Log($"Enemy deals {damage} damage.");

            if (!Player.IsAlive)
            {
                EndCombat(false);
                return;
            }

            BeginRound();
        }

        private void BeginEffectSequence(string sourceName, IReadOnlyList<CardEffectData> effects)
        {
            _resolvingEffects.Clear();
            _resolvingEffects.AddRange(effects);
            _resolvingEffectIndex = 0;
            _resolvingSourceName = sourceName;

            ContinueEffectSequence();
        }

        private void ContinueEffectSequence()
        {
            while (_resolvingEffectIndex < _resolvingEffects.Count)
            {
                var effect = _resolvingEffects[_resolvingEffectIndex++];

                if (TryQueueChoiceEffect(effect))
                {
                    NotifyStateChanged();
                    return;
                }

                if (ResolveSpecialEffect(effect))
                {
                    if (CheckForCombatEnd()) return;
                    continue;
                }

                CardEffectProcessor.ProcessEffect(effect, Player, Enemy);
                if (CheckForCombatEnd()) return;
            }

            _resolvingEffects.Clear();
            _resolvingEffectIndex = 0;
            _resolvingSourceName = null;
            NotifyStateChanged();
        }

        private bool TryQueueChoiceEffect(CardEffectData effect)
        {
            if (State != ECombatState.PlayerTurn)
                return false;

            if (effect.effectType == EEffectType.RerollDice)
            {
                int dieCount = effect.diceTarget == ECardTarget.PlayerDice
                    ? Player.Dice.DiceCount
                    : Enemy.Dice.DiceCount;

                int selectionLimit = Math.Min(effect.count, dieCount);
                if (selectionLimit <= 0) return false;

                QueueDiceSelection(
                    effect.diceTarget,
                    selectionLimit,
                    $"Choose up to {selectionLimit} {(effect.diceTarget == ECardTarget.PlayerDice ? "player" : "enemy")} dice to reroll.",
                    indices =>
                    {
                        if (effect.diceTarget == ECardTarget.PlayerDice)
                            Player.Dice.RerollAtIndices(indices);
                        else
                            Enemy.Dice.RerollAtIndices(indices);

                        Log($"Rerolled {indices.Length} {(effect.diceTarget == ECardTarget.PlayerDice ? "player" : "enemy")} dice.");
                    });

                return true;
            }

            if (effect.effectType == EEffectType.CycleHand)
            {
                if (Player.Deck.Hand.Count == 0)
                {
                    Log($"{_resolvingSourceName} has no extra card to cycle.");
                    return false;
                }

                QueueHandSelection(PendingHandAction.Cycle, effect.drawCount, "Choose another card to discard, then draw 2.");
                return true;
            }

            return false;
        }

        private bool ResolveSpecialEffect(CardEffectData effect)
        {
            if (effect.effectType != EEffectType.AddWager)
                return false;

            Player.ActiveWagers.Add(new WagerData(_resolvingSourceName, effect));
            Log($"{_resolvingSourceName} sets a wager for next round.");
            return true;
        }

        private bool ResolvePendingHandSelection(CardData card)
        {
            if (!Player.Deck.DiscardCardFromHand(card))
                return false;

            switch (_pendingHandAction)
            {
                case PendingHandAction.Scout:
                    Player.Deck.DrawSpecificCount(_pendingDrawCount);
                    Log($"Scout cycled {card.cardName} into {_pendingDrawCount} new cards.");
                    break;

                case PendingHandAction.Cycle:
                    Player.Deck.DrawSpecificCount(_pendingDrawCount);
                    Log($"{_resolvingSourceName} cycled {card.cardName} into {_pendingDrawCount} new cards.");
                    break;
            }

            ClearPendingHandSelection();
            ContinueEffectSequence();
            return true;
        }

        private void QueueDiceSelection(ECardTarget target, int maxSelections, string prompt, Action<int[]> resolver)
        {
            _selectedDiceIndices.Clear();
            _pendingDiceTarget = target;
            _pendingDiceSelectionLimit = maxSelections;
            _pendingDiceResolver = resolver;
            PendingPrompt = prompt;
        }

        private void QueueHandSelection(PendingHandAction action, int drawCount, string prompt)
        {
            _pendingHandAction = action;
            _pendingDrawCount = drawCount;
            PendingPrompt = prompt;
        }

        private void ClearPendingDiceSelection()
        {
            _selectedDiceIndices.Clear();
            _pendingDiceResolver = null;
            _pendingDiceSelectionLimit = 0;
            PendingPrompt = null;
        }

        private void ClearPendingHandSelection()
        {
            _pendingHandAction = PendingHandAction.None;
            _pendingDrawCount = 0;
            PendingPrompt = null;
        }

        private void ClearAllPrompts()
        {
            ClearPendingDiceSelection();
            ClearPendingHandSelection();
            IsSelectingRetain = false;
        }

        private bool CheckForCombatEnd()
        {
            if (!Enemy.IsAlive)
            {
                EndCombat(true);
                return true;
            }

            if (!Player.IsAlive)
            {
                EndCombat(false);
                return true;
            }

            return false;
        }

        private void EndCombat(bool playerWon)
        {
            _resolvingEffects.Clear();
            _resolvingEffectIndex = 0;
            _resolvingSourceName = null;
            ClearAllPrompts();
            SetState(playerWon ? ECombatState.Victory : ECombatState.Defeat);
            OnCombatEnded?.Invoke(playerWon);
        }

        private void SetState(ECombatState newState)
        {
            State = newState;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke(State);
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke(message);
        }
    }
}
