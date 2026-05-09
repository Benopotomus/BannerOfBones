using System;
using System.Collections.Generic;
using System.Linq;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Drives a single combat encounter between the player and a group of enemies.
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
        public IReadOnlyList<EnemyCombatant> Enemies => _enemies;
        public EnemyCombatant Enemy => GetFirstAliveEnemy() ?? (_enemies.Count > 0 ? _enemies[0] : null);
        public ECombatState State { get; private set; } = ECombatState.Idle;

        public bool IsAwaitingEnemySelection => _pendingEnemyResolver != null;
        public bool IsAwaitingDiceSelection => _pendingDiceResolver != null;
        public bool IsAwaitingHandSelection => _pendingHandAction != PendingHandAction.None;
        public bool IsSelectingRetain { get; private set; }
        public bool HasPendingChoice => IsAwaitingEnemySelection || IsAwaitingDiceSelection || IsAwaitingHandSelection || IsSelectingRetain;
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

        private readonly List<EnemyCombatant> _enemies = new List<EnemyCombatant>();
        private readonly List<int> _selectedDiceIndices = new List<int>();
        private readonly List<CardEffectData> _resolvingEffects = new List<CardEffectData>();
        private PendingHandAction _pendingHandAction = PendingHandAction.None;
        private Action<int> _pendingEnemyResolver;
        private Action<int[]> _pendingDiceResolver;
        private ECardTarget _pendingDiceTarget;
        private int _pendingEnemyDiceTargetIndex = -1;
        private int _pendingDiceSelectionLimit;
        private int _pendingDrawCount;
        private int _resolvingEffectIndex;
        private int _resolvingTargetEnemyIndex = -1;
        private bool _resolvingTargetsAllEnemies;
        private string _resolvingSourceName;

        /// <summary>Initialises combat and begins the first round.</summary>
        public void StartCombat(IReadOnlyList<EnemyData> enemyData, List<CardData> playerDeck,
                                int playerHealth = 30, int playerEnergy = 3)
        {
            Player = new PlayerCombatant(playerHealth, playerEnergy, playerDeck);
            _enemies.Clear();

            if (enemyData != null)
            {
                foreach (var enemy in enemyData.Where(data => data != null).Take(4))
                    _enemies.Add(new EnemyCombatant(enemy));
            }

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

        public bool CanSelectEnemy(int enemyIndex)
        {
            return IsAwaitingEnemySelection
                   && enemyIndex >= 0
                   && enemyIndex < _enemies.Count
                   && _enemies[enemyIndex].IsAlive;
        }

        public bool CanSelectDie(ECardTarget target, int dieIndex, int enemyIndex = -1)
        {
            if (!IsAwaitingDiceSelection || target != _pendingDiceTarget)
                return false;

            int dieCount;
            if (target == ECardTarget.PlayerDice)
            {
                dieCount = Player.Dice.DiceCount;
            }
            else
            {
                if (enemyIndex != _pendingEnemyDiceTargetIndex
                    || enemyIndex < 0
                    || enemyIndex >= _enemies.Count
                    || !_enemies[enemyIndex].IsAlive)
                {
                    return false;
                }

                dieCount = _enemies[enemyIndex].Dice.DiceCount;
            }

            return dieIndex >= 0 && dieIndex < dieCount;
        }

        public bool IsDieSelected(ECardTarget target, int dieIndex, int enemyIndex = -1)
        {
            if (target != _pendingDiceTarget)
                return false;

            if (target == ECardTarget.EnemyDice && enemyIndex != _pendingEnemyDiceTargetIndex)
                return false;

            return _selectedDiceIndices.Contains(dieIndex);
        }

        /// <summary>
        /// Handles the current meaning of a hand-card click: play, discard for scout/cycle, or retain select.
        /// </summary>
        public bool TryHandleHandCardClick(CardData card)
        {
            if (State != ECombatState.PlayerTurn || card == null) return false;
            if (IsAwaitingEnemySelection || IsAwaitingDiceSelection) return false;

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

            bool requiresEnemyTarget = CardEffectProcessor.CardRequiresEnemyTarget(card);
            bool targetsAllEnemies = card.targetsAllEnemies && requiresEnemyTarget;

            Log($"Played {card.cardName}.");

            if (requiresEnemyTarget && !targetsAllEnemies)
            {
                int singleEnemyIndex = GetSingleAliveEnemyIndex();
                if (singleEnemyIndex >= 0)
                {
                    BeginCardResolution(card, singleEnemyIndex, false);
                }
                else
                {
                    QueueEnemySelection($"Choose a target for {card.cardName}.",
                        enemyIndex => BeginCardResolution(card, enemyIndex, false));
                    NotifyStateChanged();
                }

                return true;
            }

            BeginCardResolution(card, -1, targetsAllEnemies);
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
            if (State != ECombatState.PlayerTurn
                || IsAwaitingEnemySelection
                || IsAwaitingDiceSelection
                || IsAwaitingHandSelection
                || Player.Deck.Hand.Count == 0)
            {
                return false;
            }

            IsSelectingRetain = !IsSelectingRetain;
            PendingPrompt = IsSelectingRetain ? "Choose 1 card to retain for next round." : null;
            NotifyStateChanged();
            return true;
        }

        public bool TrySelectEnemy(int enemyIndex)
        {
            if (!CanSelectEnemy(enemyIndex))
                return false;

            var resolver = _pendingEnemyResolver;
            ClearPendingEnemySelection();
            resolver?.Invoke(enemyIndex);
            return true;
        }

        public bool TogglePendingDieSelection(ECardTarget target, int dieIndex, int enemyIndex = -1)
        {
            if (!CanSelectDie(target, dieIndex, enemyIndex)) return false;

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

        private void BeginCardResolution(CardData card, int targetEnemyIndex, bool targetsAllEnemies)
        {
            if (card.duration == ECardDuration.Persistent)
                Player.ActivePersistentCards.Add(new PersistentCardRuntime(card, targetEnemyIndex, targetsAllEnemies));

            BeginEffectSequence(card.cardName, card.effects, targetEnemyIndex, targetsAllEnemies);
        }

        private void BeginRound()
        {
            ClearAllPrompts();

            Player.StartRound();
            foreach (var enemy in _enemies.Where(enemy => enemy.IsAlive))
                enemy.StartRound();

            foreach (var enemy in _enemies.Where(enemy => enemy.IsAlive))
                enemy.ApplyPreRoundEffects();

            ApplyPersistentCardEffects();
            if (CheckForCombatEnd()) return;

            ResolveWagers();
            if (CheckForCombatEnd()) return;

            SetState(ECombatState.PlayerTurn);
            OnRoundStarted?.Invoke();
        }

        private void ApplyPersistentCardEffects()
        {
            foreach (var persistent in Player.ActivePersistentCards)
            {
                foreach (var effect in persistent.Card.effects)
                {
                    if (effect.effectType == EEffectType.AddDie
                        || effect.effectType == EEffectType.RemoveDie
                        || effect.effectType == EEffectType.CycleHand
                        || effect.effectType == EEffectType.AddWager)
                    {
                        continue;
                    }

                    CardEffectProcessor.ProcessEffect(effect, Player, _enemies,
                        persistent.TargetEnemyIndex, persistent.TargetsAllEnemies);
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
                if (wager.DiceTarget == ECardTarget.PlayerDice)
                {
                    var targets = new List<EnemyCombatant>(GetEnemyTargets(wager.TargetEnemyIndex, wager.TargetsAllEnemies));
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        wager.TriggerOn, Player.Dice.CurrentRoll, wager.DieValue, wager.ValueThreshold);

                    if (triggers > 0 && targets.Count > 0)
                    {
                        int damage = wager.Magnitude;
                        foreach (var enemy in targets)
                            enemy.TakeDamage(damage);

                        Log($"{wager.SourceName} pays off for {damage} damage.");
                    }
                    else
                    {
                        Log($"{wager.SourceName} whiffs this round.");
                    }

                    continue;
                }

                bool anyTriggered = false;
                foreach (var enemy in GetEnemyTargets(wager.TargetEnemyIndex, wager.TargetsAllEnemies))
                {
                    int triggers = PokerEvaluator.EvaluateTriggerCount(
                        wager.TriggerOn, enemy.Dice.CurrentRoll, wager.DieValue, wager.ValueThreshold);

                    if (triggers <= 0) continue;
                    anyTriggered = true;
                    enemy.TakeDamage(wager.Magnitude);
                }

                Log(anyTriggered
                    ? $"{wager.SourceName} pays off for {wager.Magnitude} damage."
                    : $"{wager.SourceName} whiffs this round.");
            }
        }

        private void ExecuteEnemyTurn()
        {
            SetState(ECombatState.EnemyTurn);

            int damage = 0;
            foreach (var enemy in _enemies.Where(currentEnemy => currentEnemy.IsAlive))
                damage += enemy.CalculateDamage();

            Player.TakeDamage(damage);
            Log($"Enemies deal {damage} damage.");

            if (!Player.IsAlive)
            {
                EndCombat(false);
                return;
            }

            BeginRound();
        }

        private void BeginEffectSequence(string sourceName, IReadOnlyList<CardEffectData> effects,
            int targetEnemyIndex, bool targetsAllEnemies)
        {
            _resolvingEffects.Clear();
            _resolvingEffects.AddRange(effects);
            _resolvingEffectIndex = 0;
            _resolvingSourceName = sourceName;
            _resolvingTargetEnemyIndex = targetEnemyIndex;
            _resolvingTargetsAllEnemies = targetsAllEnemies;

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

                CardEffectProcessor.ProcessEffect(effect, Player, _enemies,
                    _resolvingTargetEnemyIndex, _resolvingTargetsAllEnemies);
                if (CheckForCombatEnd()) return;
            }

            _resolvingEffects.Clear();
            _resolvingEffectIndex = 0;
            _resolvingSourceName = null;
            _resolvingTargetEnemyIndex = -1;
            _resolvingTargetsAllEnemies = false;
            NotifyStateChanged();
        }

        private bool TryQueueChoiceEffect(CardEffectData effect)
        {
            if (State != ECombatState.PlayerTurn)
                return false;

            if (effect.effectType == EEffectType.RerollDice)
            {
                if (effect.diceTarget == ECardTarget.EnemyDice && _resolvingTargetsAllEnemies)
                    return false;

                int enemyIndex = effect.diceTarget == ECardTarget.EnemyDice ? _resolvingTargetEnemyIndex : -1;
                int dieCount = effect.diceTarget == ECardTarget.PlayerDice
                    ? Player.Dice.DiceCount
                    : GetEnemyAt(enemyIndex)?.Dice.DiceCount ?? 0;

                int selectionLimit = Math.Min(effect.count, dieCount);
                if (selectionLimit <= 0) return false;

                QueueDiceSelection(
                    effect.diceTarget,
                    selectionLimit,
                    effect.diceTarget == ECardTarget.PlayerDice
                        ? $"Choose up to {selectionLimit} player dice to reroll."
                        : $"Choose up to {selectionLimit} dice on {GetEnemyName(enemyIndex)} to reroll.",
                    indices =>
                    {
                        if (effect.diceTarget == ECardTarget.PlayerDice)
                        {
                            Player.Dice.RerollAtIndices(indices);
                            Log($"Rerolled {indices.Length} player dice.");
                        }
                        else
                        {
                            var enemy = GetEnemyAt(enemyIndex);
                            enemy?.Dice.RerollAtIndices(indices);
                            Log($"Rerolled {indices.Length} dice on {GetEnemyName(enemyIndex)}.");
                        }
                    },
                    enemyIndex);

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

            Player.ActiveWagers.Add(new WagerData(
                _resolvingSourceName,
                effect,
                _resolvingTargetEnemyIndex,
                _resolvingTargetsAllEnemies));
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

        private void QueueEnemySelection(string prompt, Action<int> resolver)
        {
            _pendingEnemyResolver = resolver;
            PendingPrompt = prompt;
        }

        private void QueueDiceSelection(ECardTarget target, int maxSelections, string prompt, Action<int[]> resolver,
            int enemyIndex = -1)
        {
            _selectedDiceIndices.Clear();
            _pendingDiceTarget = target;
            _pendingEnemyDiceTargetIndex = target == ECardTarget.EnemyDice ? enemyIndex : -1;
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

        private void ClearPendingEnemySelection()
        {
            _pendingEnemyResolver = null;
            PendingPrompt = null;
        }

        private void ClearPendingDiceSelection()
        {
            _selectedDiceIndices.Clear();
            _pendingDiceResolver = null;
            _pendingDiceSelectionLimit = 0;
            _pendingEnemyDiceTargetIndex = -1;
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
            ClearPendingEnemySelection();
            ClearPendingDiceSelection();
            ClearPendingHandSelection();
            IsSelectingRetain = false;
        }

        private bool CheckForCombatEnd()
        {
            if (_enemies.All(enemy => !enemy.IsAlive))
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
            _resolvingTargetEnemyIndex = -1;
            _resolvingTargetsAllEnemies = false;
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

        private EnemyCombatant GetEnemyAt(int enemyIndex)
        {
            return enemyIndex >= 0 && enemyIndex < _enemies.Count ? _enemies[enemyIndex] : null;
        }

        private string GetEnemyName(int enemyIndex)
        {
            return GetEnemyAt(enemyIndex)?.Data.enemyName ?? "the target";
        }

        private EnemyCombatant GetFirstAliveEnemy()
        {
            return _enemies.FirstOrDefault(enemy => enemy.IsAlive);
        }

        private int GetSingleAliveEnemyIndex()
        {
            int foundIndex = -1;

            for (int i = 0; i < _enemies.Count; i++)
            {
                if (!_enemies[i].IsAlive)
                    continue;

                if (foundIndex >= 0)
                    return -1;

                foundIndex = i;
            }

            return foundIndex;
        }

        private IEnumerable<EnemyCombatant> GetEnemyTargets(int targetEnemyIndex, bool targetsAllEnemies)
        {
            if (targetsAllEnemies)
            {
                foreach (var enemy in _enemies.Where(currentEnemy => currentEnemy.IsAlive))
                    yield return enemy;
                yield break;
            }

            var targetEnemy = GetEnemyAt(targetEnemyIndex);
            if (targetEnemy != null && targetEnemy.IsAlive)
                yield return targetEnemy;
        }
    }
}
