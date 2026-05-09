 Card & Dice Combat — Game Design Document

## Overview

A turn-based card game with poker-dice mechanics. The player builds a deck of action cards and fights small enemy groups through a series of combat encounters. Each round, the player and up to four enemies roll pools of dice. Cards are played to spend those dice results on damage, defense, and dice manipulation. Dice can be upgraded or downgraded between types, and temporary dice can be borrowed for a single round.

---

## Core Concepts

### Dice

#### Die Types
Dice come in six standard tiers. Higher-tier dice produce larger values, which benefits cards that reward high rolls.

| Die | Faces | Notes |
|---|---|---|
| d4 | 1–4 | Weakest; cannot be downgraded further |
| d6 | 1–6 | Default starting die |
| d8 | 1–8 | First upgrade tier |
| d10 | 1–10 | |
| d12 | 1–12 | Strongest; cannot be upgraded further |

#### Pool Rules
- **Player dice pool**: starts with 5 d6, rolled at the start of every round.
- **Enemy dice pool**: each enemy rolls their own 2–4 d6 dice at the start of every round.
- Dice results are evaluated using **poker-dice style** rules: pairs, triples, straights, full houses, four-of-a-kind, and five-of-a-kind.

#### Upgrading and Downgrading
- **Upgrade Die**: advances one die one tier up (e.g. d6 → d8). The change is permanent for the rest of combat.
- **Downgrade Die**: drops one die one tier down. Can be applied to enemy dice to weaken them.
- Both effects require the player to select which die to target.

#### Temporary Dice
- Some cards add a **temporary** die to the pool (shown with a `*` label in the UI, e.g. "d8*").
- Temporary dice are rolled normally and contribute to poker hands for the round they were added.
- At the **start of the next round**, all temporary dice are removed before rolling.
- A persistent card that adds a temporary die will re-add it at the start of each subsequent round.

### Poker-Dice Hand Reference
| Hand | Description | Example |
|---|---|---|
| Single Value | Count dice showing a specific face | Three [4]s showing |
| Pair | Exactly two dice share a value | 3, 3 |
| Two Pair | Two separate pairs | 2, 2, 5, 5 |
| Triple | Exactly three dice share a value | 6, 6, 6 |
| Straight | Five sequential values (d6 faces only) | 1-2-3-4-5 or 2-3-4-5-6 |
| Full House | One triple + one pair | 4, 4, 4, 2, 2 |
| Four of a Kind | Exactly four dice share a value | 1, 1, 1, 1 |
| Five of a Kind | All dice show the same value | 5, 5, 5, 5, 5 |

### Energy
- The player has a **maximum energy** value (default: 3) that resets at the start of every round.
- Each card has an **energy cost** (0–3). Cards can only be played if the player has enough current energy.
- Playing a card reduces current energy by its cost. Energy does not carry between rounds.

### Cards
- The player builds a **deck** before combat.
- At the start of each round, the player **draws 5 cards** from their deck.
- Cards can be played freely in any order during the player's turn, up to the energy limit.
- Unplayed cards are **discarded** at end of round.
- If the draw pile is empty, the discard pile is **shuffled** back into it.

#### Card Duration
- **Instant**: Effect fires once when played, card goes to discard.
- **Persistent**: Effect was applied once when played (e.g., add a die) AND recurring effects (block, reroll) apply again at the start of each subsequent round. Persistent cards remain in play for the rest of combat.
- **Exhaust**: Effect fires once when played, then the card is removed from the deck for the rest of combat.

#### Card Targets
Cards specify which dice pool their effects evaluate:
- **Player Dice**: Read your own dice.
- **Enemy Dice**: Read one chosen enemy's dice, unless the card targets all enemies.

Cards that damage or manipulate enemies must choose a target enemy when played, unless the card explicitly says it hits **all enemies**.

---

## Turn Structure

### 1. Roll Phase
Both the player and the enemy roll all their dice simultaneously.

### 2. Persistent Effect Phase
Any persistent cards already in play apply their recurring effects:
- Block cards grant block based on the current dice roll.
- Persistent enemy-reroll cards force the enemy to re-roll one or more dice.
- The new enemy dice are used for the rest of the round.

### 3. Player Turn
The player may spend energy on any number of cards or baseline actions during their turn:
- **Play cards** from hand.
- **Focus** (1 energy): Reroll 1 chosen player die.
- **Brace** (1 energy): Gain 2 block.
- **Scout** (2 energy): Discard 1 card, then draw 2 cards.
- **Retain**: Mark 1 card in hand to keep for the next round instead of discarding it.

Card effects resolve immediately in play order:
- Most attack cards choose a single living enemy when played.
- Cards marked as affecting **all enemies** resolve against every living enemy.
- Rerolling dice lets the player choose which dice to reroll.
- Damage dealt reduces the chosen enemy's health immediately, or all enemies for area effects.
- Block gained reduces incoming damage from the enemy this round.

The player ends their turn when they choose to pass or run out of playable cards.

### 4. Enemy Turn
Each living enemy resolves a clearly shown **intent** at the end of the player's turn.
Standard enemies follow a simple repeating action pattern, and the UI shows both the current and next intent.
All enemy damage is totaled together, then reduced by any remaining player block.
Excess damage is applied to the player's health.

### 5. End of Round
- Player block is cleared (only persistent block cards restore it next round).
- The player's hand is discarded except for 1 retained card, if chosen.
- A new round begins (Step 1).

---

## Player Rules
- **Starting Health**: 30 HP.
- **Starting Energy**: 3 per round (can be modified by deck/items).
- **Starting Dice Pool**: 5 d6.
- The player wins when every enemy's health reaches 0.
- The player loses when their own health reaches 0.

---

## Enemy Rules
- Enemies do **not** have cards or a hand.
- Standard enemies use a short repeating list of **intents** instead of stacked passive formulas.
- The current and next intent are always visible to the player.
- Enemy dice are still rolled because player cards and wagers can target enemy dice pools.
- Encounters field **1–4 enemies** at once.
- Enemies have **fixed dice counts** (2–4 dice), defined per enemy.

---

## Card Effect Types

| Type | Description |
|---|---|
| Deal Damage | Deal `magnitude × trigger_count` damage to the enemy |
| Conditional Damage | Deal `magnitude` damage if condition is met; otherwise the player takes `altMagnitude` damage |
| Self Damage | Player takes `magnitude` damage |
| Gain Block | Player gains `magnitude × trigger_count` block |
| Reroll Dice | Reroll `count` dice from the target pool (lowest values rerolled first in automation) |
| Reroll All Dice | Reroll all dice in the target pool |
| Reroll By Value | Reroll all dice in target pool that show a specific value |
| Add Die | Add one permanent die (of the specified type) to a pool |
| Remove Die | Remove one die from a pool |
| Add Temporary Die | Add one die of the specified type that is removed at the start of the next round |
| Upgrade Die | Player selects one die to advance one tier (d6→d8, d8→d10, etc.) |
| Downgrade Die | Player selects one die to drop one tier; can target enemy dice |
| Cycle Hand | Discard another card from hand, then draw cards |
| Add Wager | Queue a one-shot payoff that resolves at the start of the next round |

---

## Example Cards

### Damage Cards
- **Strike** (1 energy): Deal 1 damage for every die showing 5 or 6.
- **Swift Slash** (1 energy): Deal 1 damage for each [1] rolled.
- **Ace High** (1 energy): Deal 3 damage for each [6] rolled.
- **Twin Fangs** (1 energy): Deal 2 damage for each pair rolled.
- **Spirit Strike** (2 energy): Deal 3 damage for each pair rolled.
- **Focused Strike** (2 energy): Deal 4 damage for each triple rolled.
- **Gambler's Blade** (2 energy): Deal 5 damage if you have a straight; otherwise take 1 damage.
- **Crushing Blow** (3 energy): Deal 4 damage per full house rolled.
- **Dragon's Roar** (3 energy): Deal 6 damage per five-of-a-kind rolled.
- **Scatter Shot** (2 energy): Deal 1 damage to all enemies for each unique die value showing.
- **Chain Lightning** (2 energy): Deal 2 damage to every enemy for each pair found in their dice.

### Defense Cards
- **Iron Shield** (1 energy): Block 1 damage for each [4] rolled.
- **Bone Ward** (1 energy): Block damage equal to the value of your highest die.
- **Tower Shield** (2 energy, Persistent): Each round, block 1 damage per die showing 5 or 6.
- **Rune Shield** (2 energy, Persistent): Each round, block 2 damage per [6] rolled.

### Dice Manipulation
- **Lucky Reroll** (1 energy): Choose up to 3 of your dice to reroll.
- **Full Send** (2 energy): Reroll all of your dice.
- **Hex Curse** (2 energy): Choose an enemy, then reroll up to 2 of their dice.
- **Cursed Dice** (2 energy): Force all enemies to reroll all of their dice.
- **War Drums** (3 energy): Reroll all enemy dice, then deal 1 damage to each enemy per unique value in their new roll.
- **Lucky Charm** (3 energy, Persistent): Add 1 die to your pool for the rest of combat.

### Hand Planning
- **Tactical Pivot** (1 energy): Discard another card, then draw 2 cards.
- **Loaded Bet** (1 energy): Next round, if you roll a pair, deal 6 damage to the chosen enemy.

### Exhaust Cards
- **Last Stand** (0 energy, Exhaust): Deal damage equal to 3× your highest die.
- **Sacrifice** (1 energy, Exhaust): Remove 1 die from your pool, then deal 10 damage.

### Die Type Manipulation
- **Rune Forge** (2 energy): Choose one of your dice. Upgrade it to the next tier permanently (d6→d8, d8→d10, etc.).
- **Corrosive Touch** (1 energy): Choose one enemy die. Downgrade it one tier permanently.
- **Borrowed Die** (1 energy): Add a temporary d8 to your pool. It is removed at the start of the next round.
- **Crystal Conduit** (3 energy, Persistent): At the start of each round, add a temporary d10 to your pool.

### High-Risk Cards
- **Death's Gamble** (1 energy): If all 5 dice match, deal 8 damage. Otherwise, take 2 damage.

---

## Example Enemies

### Goblin Scout (8 HP, 2 dice)
- Feint for 1 damage.
- Stab for 2 damage.

### Orc Warrior (12 HP, 2 dice)
- Cleave for 3 damage.
- Backhand for 2 damage.

### Shadow Wraith (10 HP, 3 dice)
- Drain for 2 damage.
- Ambush for 3 damage.

### Stone Golem (16 HP, 3 dice)
- Wind Up for 2 damage.
- Crush for 4 damage.

### Death Knight (18 HP, 4 dice)
- Harry for 3 damage.
- Executioner Swing for 5 damage.

---

## Deck Building Guidelines (Starter Deck)
A balanced starter deck of 10–15 cards might include:
- 4 basic **Strike** cards for reliable single-target damage.
- 3–4 block cards for sustained defense.
- 1–2 area cards to help clear groups.
- 1–2 reroll cards for dice manipulation.
- 1 hand-management or setup card.

---

## Glossary
| Term | Definition |
|---|---|
| Trigger Count | Number of times a condition is met (e.g., 3 dice showing [4] → trigger count = 3) |
| Magnitude | The base value applied per trigger (damage dealt, block gained) |
| Persistent | A card effect that applies repeatedly each round until combat ends |
| Energy | Resource spent to play cards; resets each round |
| Block | Damage reduction that absorbs incoming enemy damage; cleared each round |
| Discard | Cards played or unused at end of round enter the discard pile |
| Die Tier | One step in the upgrade chain d4 → d6 → d8 → d10 → d12 |
| Temporary Die | A die added mid-round that is removed before the next round's roll |
