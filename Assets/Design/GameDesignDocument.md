 Card & Dice Combat — Game Design Document

## Overview

A turn-based card game with poker-dice mechanics. The player builds a deck of action cards and fights enemies through a series of combat encounters. Each round, both the player and the enemy roll pools of six-sided dice. Cards are played to spend those dice results on damage, defense, and dice manipulation.

---

## Core Concepts

### Dice
- **Player dice pool**: 5 six-sided dice (d6), rolled at the start of every round.
- **Enemy dice pool**: 2–5 six-sided dice, defined per enemy and rolled at the start of every round.
- Dice results are evaluated using **poker-dice style** rules: singles, pairs, triples, straights, full houses, four-of-a-kind, and five-of-a-kind (Yahtzee).

### Poker-Dice Hand Reference
| Hand | Description | Example |
|---|---|---|
| Single Value | Count dice showing a specific face | Three [4]s showing |
| Pair | Exactly two dice share a value | 3, 3 |
| Two Pair | Two separate pairs | 2, 2, 5, 5 |
| Triple | Exactly three dice share a value | 6, 6, 6 |
| Straight | Five sequential values | 1-2-3-4-5 or 2-3-4-5-6 |
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
- **Enemy Dice**: Read the enemy's dice.

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
- Rerolling dice lets the player choose which dice to reroll.
- Damage dealt reduces the enemy's health immediately.
- Block gained reduces incoming damage from the enemy this round.

The player ends their turn when they choose to pass or run out of playable cards.

### 4. Enemy Turn
Enemy passive effects are evaluated against the enemy's current dice pool.
All enemy damage is totaled, then reduced by any remaining player block.
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
- The player wins when the enemy's health reaches 0.
- The player loses when their own health reaches 0.

---

## Enemy Rules
- Enemies do **not** have cards or a hand.
- Each enemy has a set of **passive effects** defined at creation that fire every round automatically.
- Passive effects are evaluated against the **enemy's own dice roll**, not the player's.
- Enemies have **fixed dice counts** (2–5 dice), defined per enemy.
- Some enemies have **pre-round effects** that modify their own dice before the player's turn (e.g., rerolling unfavorable faces).

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
| Add Die | Add one die to a pool (rolled immediately) |
| Remove Die | Remove one die from a pool |
| Cycle Hand | Discard another card from hand, then draw cards |
| Add Wager | Queue a one-shot payoff that resolves at the start of the next round |

---

## Example Cards

### Damage Cards
- **Swift Slash** (1 energy): Deal 1 damage for each [1] rolled.
- **Ace High** (1 energy): Deal 3 damage for each [6] rolled.
- **Twin Fangs** (1 energy): Deal 1 damage for each pair rolled.
- **Spirit Strike** (2 energy): Deal 2 damage for each pair rolled.
- **Focused Strike** (2 energy): Deal 3 damage for each triple rolled.
- **Gambler's Blade** (2 energy): Deal 5 damage if you have a straight; otherwise deal 1 damage.
- **Crushing Blow** (3 energy): Deal 4 damage per full house rolled.
- **Dragon's Roar** (3 energy): Deal 6 damage per five-of-a-kind rolled.

### Defense Cards
- **Iron Shield** (1 energy): Block 1 damage for each [4] rolled.
- **Bone Ward** (1 energy): Block damage equal to the value of your highest die.
- **Tower Shield** (2 energy, Persistent): Each round, block 1 damage per die showing 5 or 6.
- **Rune Shield** (2 energy, Persistent): Each round, block 2 damage per [6] rolled.

### Dice Manipulation
- **Lucky Reroll** (1 energy): Choose up to 3 of your dice to reroll.
- **Full Send** (2 energy): Reroll all of your dice.
- **Hex Curse** (2 energy): Choose up to 2 enemy dice to reroll.
- **Cursed Dice** (2 energy): Force the enemy to reroll all of their dice.
- **War Drums** (3 energy): Reroll all enemy dice, then deal 1 damage per unique value in their new roll.
- **Lucky Charm** (3 energy, Persistent): Add 1 die to your pool for the rest of combat.

### Hand Planning
- **Tactical Pivot** (1 energy): Discard another card, then draw 2 cards.
- **Loaded Bet** (1 energy): Next round, if you roll a pair, deal 6 damage.

### Exhaust Cards
- **Last Stand** (0 energy, Exhaust): Deal damage equal to 3× your highest die.
- **Sacrifice** (1 energy, Exhaust): Remove 1 die from your pool, then deal 10 damage.

### High-Risk Cards
- **Death's Gamble** (1 energy): If all 5 dice match, deal 8 damage. Otherwise, take 2 damage.

---

## Example Enemies

### Goblin Scout (15 HP, 2 dice)
- Deals 1 damage for each [2] rolled.
- Deals 2 damage for each pair rolled.

### Orc Warrior (25 HP, 3 dice)
- Deals 2 damage for each pair rolled.
- Deals 1 damage for each odd die rolled.

### Shadow Wraith (20 HP, 4 dice)
- Deals 1 damage for each odd die rolled.
- Deals 2 damage for each [1] rolled.

### Stone Golem (40 HP, 3 dice)
- Rerolls all dice showing [1] at the start of each round.
- Deals 3 damage for each triple rolled.
- Deals 1 damage for each even die rolled.

### Death Knight (35 HP, 5 dice)
- Deals 2 damage for each pair rolled.
- Deals 4 damage for each triple rolled.
- Deals 6 damage if a straight is rolled.

---

## Deck Building Guidelines (Starter Deck)
A balanced starter deck of 10–15 cards might include:
- 2–3 block cards for sustained defense.
- 3–4 damage cards at various energy costs.
- 1–2 reroll cards for dice manipulation.
- 1 persistent card for long-term advantage.
- 1 high-risk/high-reward card for burst potential.

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
