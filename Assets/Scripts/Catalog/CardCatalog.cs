using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Programmatic catalog of the prototype player cards.
    /// Call <see cref="CreateAllCards"/> to obtain a list of runtime <see cref="CardData"/> instances.
    /// In the Unity Editor these instances can be saved as .asset files via the CardGame menu.
    /// </summary>
    public static class CardCatalog
    {
        /// <summary>
        /// Returns a balanced starter deck that samples the expanded action set.
        /// </summary>
        public static List<CardData> CreateStarterDeck()
        {
            var deck = new List<CardData>();
            for (int i = 0; i < 4; i++) deck.Add(CreateStrike());
            for (int i = 0; i < 2; i++) deck.Add(CreateIronShield());
            for (int i = 0; i < 2; i++) deck.Add(CreateBoneWard());
            deck.Add(CreateTwinFangs());
            deck.Add(CreateLuckyReroll());
            deck.Add(CreateTacticalPivot());
            deck.Add(CreateScatterShot());
            return deck;
        }

        public static List<CardData> CreateAllCards()
        {
            return new List<CardData>
            {
                // ── Damage ────────────────────────────────────────────────────────
                CreateStrike(),
                CreateIronShield(),
                CreateSwiftSlash(),
                CreateFocusedStrike(),
                CreateGamblersBlade(),
                CreateDragonsRoar(),
                CreateScatterShot(),
                CreateSpiritStrike(),
                CreateTwinFangs(),
                CreateCrushingBlow(),
                CreatePrecisionStrike(),
                CreateAceHigh(),
                CreateLowSweep(),
                CreateBerserkersRage(),
                CreateChainLightning(),
                CreateDeathsGamble(),

                // ── Defense ───────────────────────────────────────────────────────
                CreateBoneWard(),
                CreateTowerShield(),
                CreateRuneShield(),

                // ── Dice Manipulation ─────────────────────────────────────────────
                CreateLuckyReroll(),
                CreateFullSend(),
                CreateHexCurse(),
                CreateCursedDice(),
                CreateWarDrums(),
                CreateCursedAura(),
                CreateLuckyCharm(),
                CreateTacticalPivot(),
                CreateLoadedBet(),
                CreateLastStand(),
                CreateSacrifice(),

                // ── Die Type Manipulation ─────────────────────────────────────────
                CreateRuneForge(),
                CreateCorrosiveTouch(),
                CreateBorrowedDie(),
                CreateCrystalConduit(),
            };
        }

        // ── Card 01 ───────────────────────────────────────────────────────────────

        private static CardData CreateStrike()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Strike";
            card.description = "Deal 1 damage for every die showing 5+.";
            card.energyCost = 1;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn = EPokerHandType.PerHighDie,
                    valueThreshold = 5,
                    magnitude = 1,
                },
            };
            return card;
        }

        // ── Card 02 ───────────────────────────────────────────────────────────────

        private static CardData CreateIronShield()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Iron Shield";
            card.description = "Block 1 damage for each [4] rolled.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType     = EEffectType.GainBlock,
                    diceTarget     = ECardTarget.PlayerDice,
                    triggerOn      = EPokerHandType.PerDieValue,
                    dieValue       = 4,
                    magnitude      = 1,
                },
            };
            return card;
        }

        // ── Card 03 ───────────────────────────────────────────────────────────────

        private static CardData CreateSwiftSlash()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Swift Slash";
            card.description = "Deal 1 damage for each [1] rolled.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerDieValue,
                    dieValue   = 1,
                    magnitude  = 1,
                },
            };
            return card;
        }

        // ── Card 04 ───────────────────────────────────────────────────────────────

        private static CardData CreateFocusedStrike()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Focused Strike";
            card.description = "Deal 4 damage for each triple rolled.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerTriple,
                    magnitude  = 4,
                },
            };
            return card;
        }

        // ── Card 05 ───────────────────────────────────────────────────────────────

        private static CardData CreateGamblersBlade()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Gambler's Blade";
            card.description = "If you have a straight, deal 5 damage. Otherwise, take 1 damage.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType   = EEffectType.ConditionalDamage,
                    diceTarget   = ECardTarget.PlayerDice,
                    triggerOn    = EPokerHandType.IfStraight,
                    magnitude    = 5,
                    altMagnitude = 1,
                },
            };
            return card;
        }

        // ── Card 06 ───────────────────────────────────────────────────────────────

        private static CardData CreateDragonsRoar()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Dragon's Roar";
            card.description = "Deal 6 damage for each five-of-a-kind rolled.";
            card.energyCost  = 3;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerFiveOfAKind,
                    magnitude  = 6,
                },
            };
            return card;
        }

        // ── Card 07 ───────────────────────────────────────────────────────────────

        private static CardData CreateScatterShot()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Scatter Shot";
            card.description = "Deal 1 damage to all enemies for each unique die value showing.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.targetsAllEnemies = true;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerUniqueDieValue,
                    magnitude  = 1,
                },
            };
            return card;
        }

        // ── Card 08 ───────────────────────────────────────────────────────────────

        private static CardData CreateSpiritStrike()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Spirit Strike";
            card.description = "Deal 3 damage for each pair rolled.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerPair,
                    magnitude  = 3,
                },
            };
            return card;
        }

        // ── Card 09 ───────────────────────────────────────────────────────────────

        private static CardData CreateTwinFangs()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Twin Fangs";
            card.description = "Deal 2 damage for each pair rolled.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerPair,
                    magnitude  = 2,
                },
            };
            return card;
        }

        // ── Card 10 ───────────────────────────────────────────────────────────────

        private static CardData CreateCrushingBlow()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Crushing Blow";
            card.description = "Deal 4 damage per full house (triple + pair) rolled.";
            card.energyCost  = 0;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerFullHouse,
                    magnitude  = 4,
                },
            };
            return card;
        }

        // ── Card 11 ───────────────────────────────────────────────────────────────

        private static CardData CreatePrecisionStrike()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Precision Strike";
            card.description = "Deal 5 damage for each four-of-a-kind rolled.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerFourOfAKind,
                    magnitude  = 5,
                },
            };
            return card;
        }

        // ── Card 12 ───────────────────────────────────────────────────────────────

        private static CardData CreateAceHigh()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Ace High";
            card.description = "Deal 3 damage for each [6] rolled.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerDieValue,
                    dieValue   = 6,
                    magnitude  = 3,
                },
            };
            return card;
        }

        // ── Card 13 ───────────────────────────────────────────────────────────────

        private static CardData CreateLowSweep()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Low Sweep";
            card.description = "Deal 1 damage for each die showing [1] or [2].";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType     = EEffectType.DealDamage,
                    diceTarget     = ECardTarget.PlayerDice,
                    triggerOn      = EPokerHandType.PerLowDie,
                    valueThreshold = 2,
                    magnitude      = 1,
                },
            };
            return card;
        }

        // ── Card 14 ───────────────────────────────────────────────────────────────

        private static CardData CreateBerserkersRage()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Berserker's Rage";
            card.description = "Deal 1 damage for each odd die rolled ([1], [3], [5]).";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerOddDie,
                    magnitude  = 1,
                },
            };
            return card;
        }

        // ── Card 15 ───────────────────────────────────────────────────────────────

        private static CardData CreateChainLightning()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Chain Lightning";
            card.description = "Deal 2 damage to every enemy for each pair found in their dice.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.targetsAllEnemies = true;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.EnemyDice,
                    triggerOn  = EPokerHandType.PerPair,
                    magnitude  = 2,
                },
            };
            return card;
        }

        // ── Card 16 ───────────────────────────────────────────────────────────────

        private static CardData CreateDeathsGamble()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Death's Gamble";
            card.description = "If all 5 dice match, deal 8 damage. Otherwise, take 2 damage.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType   = EEffectType.ConditionalDamage,
                    diceTarget   = ECardTarget.PlayerDice,
                    triggerOn    = EPokerHandType.PerFiveOfAKind,
                    magnitude    = 8,
                    altMagnitude = 2,
                },
            };
            return card;
        }

        // ── Card 17 ───────────────────────────────────────────────────────────────

        private static CardData CreateBoneWard()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Bone Ward";
            card.description = "Block damage equal to the value of your highest die.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.GainBlock,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.HighestDieValue,
                    magnitude  = 1,
                },
            };
            return card;
        }

        // ── Card 18 ───────────────────────────────────────────────────────────────

        private static CardData CreateTowerShield()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Tower Shield";
            card.description = "Each round, block 1 damage for each die showing 5 or 6.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Persistent;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType     = EEffectType.GainBlock,
                    diceTarget     = ECardTarget.PlayerDice,
                    triggerOn      = EPokerHandType.PerHighDie,
                    valueThreshold = 5,
                    magnitude      = 1,
                },
            };
            return card;
        }

        // ── Card 19 ───────────────────────────────────────────────────────────────

        private static CardData CreateRuneShield()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Rune Shield";
            card.description = "Each round, block 2 damage for each [6] rolled.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Persistent;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.GainBlock,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn  = EPokerHandType.PerDieValue,
                    dieValue   = 6,
                    magnitude  = 2,
                },
            };
            return card;
        }

        // ── Card 20 ───────────────────────────────────────────────────────────────

        private static CardData CreateLuckyReroll()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Lucky Reroll";
            card.description = "Choose up to 3 of your dice to reroll.";
            card.energyCost  = 1;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollDice,
                    diceTarget = ECardTarget.PlayerDice,
                    count      = 3,
                },
            };
            return card;
        }

        // ── Card 21 ───────────────────────────────────────────────────────────────

        private static CardData CreateFullSend()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Full Send";
            card.description = "Reroll all of your dice.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollAllDice,
                    diceTarget = ECardTarget.PlayerDice,
                },
            };
            return card;
        }

        // ── Card 22 ───────────────────────────────────────────────────────────────

        private static CardData CreateHexCurse()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Hex Curse";
            card.description = "Choose an enemy. Reroll up to 2 of their dice.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollDice,
                    diceTarget = ECardTarget.EnemyDice,
                    count      = 2,
                },
            };
            return card;
        }

        // ── Card 23 ───────────────────────────────────────────────────────────────

        private static CardData CreateCursedDice()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Cursed Dice";
            card.description = "Force all enemies to reroll all of their dice.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Instant;
            card.targetsAllEnemies = true;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollAllDice,
                    diceTarget = ECardTarget.EnemyDice,
                },
            };
            return card;
        }

        // ── Card 24 ───────────────────────────────────────────────────────────────

        private static CardData CreateWarDrums()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "War Drums";
            card.description = "Reroll all enemy dice. Then deal 1 damage to each enemy for every unique value in their new roll.";
            card.energyCost  = 3;
            card.duration    = ECardDuration.Instant;
            card.targetsAllEnemies = true;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollAllDice,
                    diceTarget = ECardTarget.EnemyDice,
                },
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.EnemyDice,
                    triggerOn  = EPokerHandType.PerUniqueDieValue,
                    magnitude  = 1,
                },
            };
            return card;
        }

        // ── Card 25 ───────────────────────────────────────────────────────────────

        private static CardData CreateCursedAura()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Cursed Aura";
            card.description = "At the start of each round, force the chosen enemy to reroll 1 of their dice.";
            card.energyCost  = 2;
            card.duration    = ECardDuration.Persistent;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RerollDice,
                    diceTarget = ECardTarget.EnemyDice,
                    count      = 1,
                },
            };
            return card;
        }

        // ── Card 26 ───────────────────────────────────────────────────────────────

        private static CardData CreateLuckyCharm()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Lucky Charm";
            card.description = "Add 1 die to your pool for the rest of combat.";
            card.energyCost  = 3;
            card.duration    = ECardDuration.Persistent;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.AddDie,
                    diceTarget = ECardTarget.PlayerDice,
                    count      = 1,
                },
            };
            return card;
        }

        private static CardData CreateTacticalPivot()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Tactical Pivot";
            card.description = "Discard another card. Draw 2 cards.";
            card.energyCost = 1;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.CycleHand,
                    drawCount = 2,
                },
            };
            return card;
        }

        private static CardData CreateLoadedBet()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Loaded Bet";
            card.description = "Next round, if you roll a pair, deal 6 damage to the chosen enemy.";
            card.energyCost = 1;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.AddWager,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn = EPokerHandType.PerPair,
                    magnitude = 6,
                },
            };
            return card;
        }

        private static CardData CreateLastStand()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Last Stand";
            card.description = "Exhaust. Deal damage equal to 3× your highest die.";
            card.energyCost = 0;
            card.duration = ECardDuration.Exhaust;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn = EPokerHandType.HighestDieValue,
                    magnitude = 3,
                },
            };
            return card;
        }

        private static CardData CreateSacrifice()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Sacrifice";
            card.description = "Exhaust. Remove 1 die from your pool, then deal 10 damage.";
            card.energyCost = 1;
            card.duration = ECardDuration.Exhaust;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.RemoveDie,
                    diceTarget = ECardTarget.PlayerDice,
                },
                new CardEffectData
                {
                    effectType = EEffectType.DealDamage,
                    diceTarget = ECardTarget.PlayerDice,
                    triggerOn = EPokerHandType.Always,
                    magnitude = 10,
                },
            };
            return card;
        }

        // ── Die Type Manipulation Cards ────────────────────────────────────────────

        private static CardData CreateRuneForge()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Rune Forge";
            card.description = "Upgrade one of your dice to the next tier (d6→d8→d10→d12).";
            card.energyCost = 2;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.UpgradeDie,
                    diceTarget = ECardTarget.PlayerDice,
                },
            };
            return card;
        }

        private static CardData CreateCorrosiveTouch()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Corrosive Touch";
            card.description = "Downgrade one enemy die to the previous tier.";
            card.energyCost = 1;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.DowngradeDie,
                    diceTarget = ECardTarget.EnemyDice,
                },
            };
            return card;
        }

        private static CardData CreateBorrowedDie()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Borrowed Die";
            card.description = "Add a temporary d8 to your pool. It is removed at the start of the next round.";
            card.energyCost = 1;
            card.duration = ECardDuration.Instant;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.AddTemporaryDie,
                    diceTarget = ECardTarget.PlayerDice,
                    dieSides = 8,
                },
            };
            return card;
        }

        private static CardData CreateCrystalConduit()
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = card.cardName = "Crystal Conduit";
            card.description = "Persistent. At the start of each round, add a temporary d10 to your pool.";
            card.energyCost = 3;
            card.duration = ECardDuration.Persistent;
            card.effects = new List<CardEffectData>
            {
                new CardEffectData
                {
                    effectType = EEffectType.AddTemporaryDie,
                    diceTarget = ECardTarget.PlayerDice,
                    dieSides = 10,
                },
            };
            return card;
        }
    }
}
