using System;
using System.Collections.Generic;
using System.Linq;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Drives a single combat encounter between the player and one enemy.
    ///
    /// Turn order each round:
    ///   1. Both sides roll their dice (StartRound).
    ///   2. Enemy pre-round passives fire (rerolls of specific values, etc.).
    ///   3. Persistent card effects apply (block from shield cards, forced enemy rerolls).
    ///   4. Player turn — cards may be played freely while energy allows.
    ///   5. Player calls EndPlayerTurn() to trigger the enemy's damage phase.
    ///   6. Enemy damage is applied to the player (block absorbs first).
    ///   7. A new round begins automatically unless combat has ended.
    /// </summary>
    public class CombatManager
    {
        public PlayerCombatant Player { get; private set; }
        public EnemyCombatant  Enemy  { get; private set; }
        public ECombatState    State  { get; private set; } = ECombatState.Idle;

        /// <summary>Fired whenever the combat state changes.</summary>
        public event Action<ECombatState> OnStateChanged;

        /// <summary>Fired at the start of every new round after setup is complete.</summary>
        public event Action OnRoundStarted;

        /// <summary>
        /// Fired when combat ends.
        /// Parameter is true if the player won, false if the player was defeated.
        /// </summary>
        public event Action<bool> OnCombatEnded;

        // ── Setup ─────────────────────────────────────────────────────────────────

        /// <summary>Initialises combat and begins the first round.</summary>
        public void StartCombat(EnemyData enemyData, List<CardData> playerDeck,
                                int playerHealth = 30, int playerEnergy = 3)
        {
            Player = new PlayerCombatant(playerHealth, playerEnergy, playerDeck);
            Enemy  = new EnemyCombatant(enemyData);
            BeginRound();
        }

        // ── Player Actions ────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to play a card from the player's hand.
        /// Returns false if the card is not in hand, energy is insufficient, or it is not the player's turn.
        /// </summary>
        public bool TryPlayCard(CardData card)
        {
            if (State != ECombatState.PlayerTurn)         return false;
            if (!Player.Energy.CanAfford(card.energyCost)) return false;
            if (!Player.Deck.Hand.Contains(card))          return false;

            Player.Energy.TrySpendEnergy(card.energyCost);
            Player.Deck.PlayCard(card);
            CardEffectProcessor.ProcessCard(card, Player, Enemy);

            if (card.duration == ECardDuration.Persistent)
                Player.ActivePersistentCards.Add(card);

            if (!Enemy.IsAlive)
            {
                SetState(ECombatState.Victory);
                OnCombatEnded?.Invoke(true);
            }

            return true;
        }

        /// <summary>Ends the player's turn and triggers enemy damage resolution.</summary>
        public void EndPlayerTurn()
        {
            if (State != ECombatState.PlayerTurn) return;
            ExecuteEnemyTurn();
        }

        // ── Private Round Flow ────────────────────────────────────────────────────

        private void BeginRound()
        {
            Player.StartRound();
            Enemy.StartRound();

            // Pre-round enemy passives (e.g., Stone Golem rerolls all [1]s)
            Enemy.ApplyPreRoundEffects();

            // Persistent cards apply their recurring effects (block, enemy rerolls)
            ApplyPersistentCardEffects();

            SetState(ECombatState.PlayerTurn);
            OnRoundStarted?.Invoke();
        }

        /// <summary>
        /// Re-applies recurring effects from persistent cards.
        /// One-time effects (AddDie, RemoveDie) are skipped — they already fired on play.
        /// </summary>
        private void ApplyPersistentCardEffects()
        {
            foreach (var card in Player.ActivePersistentCards)
            {
                foreach (var effect in card.effects)
                {
                    if (effect.effectType == EEffectType.AddDie ||
                        effect.effectType == EEffectType.RemoveDie)
                        continue;

                    CardEffectProcessor.ProcessEffect(effect, Player, Enemy);
                }
            }
        }

        private void ExecuteEnemyTurn()
        {
            SetState(ECombatState.EnemyTurn);

            int damage = Enemy.CalculateDamage();
            Player.TakeDamage(damage);

            if (!Player.IsAlive)
            {
                SetState(ECombatState.Defeat);
                OnCombatEnded?.Invoke(false);
                return;
            }

            BeginRound();
        }

        private void SetState(ECombatState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}