using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Drop this prefab (or component) into any scene and hit Play.
    /// On Start it picks a random enemy from <see cref="EnemyCatalog"/>, deals the
    /// starter deck from <see cref="CardCatalog"/>, starts <see cref="CombatManager"/>,
    /// and builds a full UGUI combat screen at runtime — no additional scene setup needed.
    /// </summary>
    public class CombatRunner : MonoBehaviour
    {
        [Header("Player Settings")]
        public int playerHealth = 30;
        public int playerEnergy = 3;

        [Header("Prefabs")]
        [Tooltip("Optional CardButton prefab. Assign for custom card styling; leave empty to use the built-in fallback.")]
        [SerializeField] private GameObject _cardButtonPrefab;

        // ── Runtime state ─────────────────────────────────────────────────────────
        private CombatManager _combat;

        // ── UI labels ─────────────────────────────────────────────────────────────
        private Text _enemyNameText;
        private Text _enemyHpText;
        private Text _enemyDiceText;
        private Text _enemyPassivesText;
        private Text _playerHpText;
        private Text _playerEnergyText;
        private Text _playerBlockText;
        private Text _playerDiceText;
        private Text _stateText;
        private Text _logText;
        private RectTransform _handContainer;
        private Button _endTurnButton;

        private RectTransform _pileViewPanel;
        private Text _pileViewText;

        private readonly List<string> _log = new List<string>();
        private const int MaxLogLines = 7;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Start()
        {
            // 1. Pick a random enemy
            var enemies = EnemyCatalog.CreateAllEnemies();
            var enemy = enemies[Random.Range(0, enemies.Count)];

            // 2. Deal the starter deck
            var deck = CardCatalog.CreateStarterDeck();

            // 3. Wire events
            _combat = new CombatManager();
            _combat.OnStateChanged += _ => RefreshUI();
            _combat.OnRoundStarted += OnRoundStarted;
            _combat.OnCombatEnded  += OnCombatEnded;

            // 4. Build UI
            BuildUI();

            // 5. Start combat
            _combat.StartCombat(enemy, deck, playerHealth, playerEnergy);

            Log($"=== {enemy.enemyName} appears! ===");
            Log(enemy.description);
            RefreshUI();
        }

        // ── UI construction ───────────────────────────────────────────────────────

        private void BuildUI()
        {
            EnsureEventSystem();

            // Root canvas (Screen Space – Overlay, works without camera setup)
            var cgo = new GameObject("CombatCanvas");
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            cgo.AddComponent<GraphicRaycaster>();

            var root = canvas.GetComponent<RectTransform>();

            // Full-screen background
            MkPanel(root, "Bg", C(0.08f, 0.07f, 0.12f), 0f, 0f, 1f, 1f);

            // ── Enemy section (top 28%) ───────────────────────────────────────────
            var ep = MkPanel(root, "EnemyArea", C(0.18f, 0.08f, 0.08f), 0f, 0.72f, 1f, 1f);
            _enemyNameText     = MkText(ep, 20, C(1f, 0.40f, 0.40f), TextAnchor.UpperLeft,  0f, 0.73f, 1f, 1f);
            _enemyHpText       = MkText(ep, 14, Color.white,           TextAnchor.UpperLeft,  0f, 0.48f, 0.5f, 0.73f);
            _enemyDiceText     = MkText(ep, 13, C(1f, 0.90f, 0.30f),  TextAnchor.UpperLeft,  0f, 0.24f, 0.5f, 0.48f);
            _enemyPassivesText = MkText(ep, 10, C(0.8f, 0.6f, 0.6f), TextAnchor.UpperLeft,  0f, 0f,    1f,   0.24f);

            // ── Player section (45%–72%) ──────────────────────────────────────────
            var pp = MkPanel(root, "PlayerArea", C(0.08f, 0.12f, 0.18f), 0f, 0.45f, 1f, 0.72f);
            _playerHpText     = MkText(pp, 16, C(0.40f, 1f, 0.40f), TextAnchor.UpperLeft, 0f, 0.72f, 0.5f, 1f);
            _playerEnergyText = MkText(pp, 14, C(0.40f, 0.6f, 1f),  TextAnchor.UpperLeft, 0f, 0.44f, 0.5f, 0.72f);
            _playerBlockText  = MkText(pp, 14, C(0.50f, 0.8f, 1f),  TextAnchor.UpperLeft, 0f, 0.16f, 0.5f, 0.44f);
            _stateText        = MkText(pp, 10, C(0.7f, 0.7f, 0.7f), TextAnchor.UpperLeft, 0.5f, 0f, 1f, 1f);

            // ── Player dice: bottom-center, just above the hand cards ─────────────
            MkPanel(root, "PlayerDiceBg", C(0.06f, 0.06f, 0.10f), 0.2f, 0.43f, 0.8f, 0.47f);
            _playerDiceText = MkText(root, 14, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0.2f, 0.43f, 0.8f, 0.47f);

            // ── Hand section (24%–45%) ────────────────────────────────────────────
            MkPanel(root, "HandBg", C(0.07f, 0.12f, 0.07f), 0f, 0.24f, 1f, 0.43f);

            var handLabel = MkText(root, 12, Color.white, TextAnchor.MiddleCenter, 0f, 0.41f, 1f, 0.43f);
            handLabel.text = "— H A N D —";

            var hc = new GameObject("HandCards");
            hc.transform.SetParent(root, false);
            var hcRT = hc.AddComponent<RectTransform>();
            hcRT.anchorMin = new Vector2(0f, 0.24f);
            hcRT.anchorMax = new Vector2(1f, 0.41f);
            hcRT.offsetMin = new Vector2(4f, 2f);
            hcRT.offsetMax = new Vector2(-4f, -2f);
            _handContainer = hcRT;

            // ── Action bar (13%–24%) ──────────────────────────────────────────────
            MkPanel(root, "ActionBar", C(0.04f, 0.04f, 0.04f), 0f, 0.13f, 1f, 0.24f);
            _endTurnButton = MkButton(root, "End Turn",
                new Vector2(0.38f, 0.145f), new Vector2(0.62f, 0.23f),
                C(0.60f, 0.15f, 0.10f), OnEndTurnClicked);
            MkButton(root, "View Deck",
                new Vector2(0.05f, 0.145f), new Vector2(0.22f, 0.23f),
                C(0.15f, 0.35f, 0.55f), OnViewDeckClicked);
            MkButton(root, "View Discard",
                new Vector2(0.24f, 0.145f), new Vector2(0.36f, 0.23f),
                C(0.35f, 0.20f, 0.45f), OnViewDiscardClicked);

            // ── Log section (0%–13%) ──────────────────────────────────────────────
            MkPanel(root, "LogBg", C(0.04f, 0.04f, 0.07f), 0f, 0f, 1f, 0.13f);
            _logText = MkText(root, 11, C(0.75f, 0.75f, 0.75f), TextAnchor.UpperLeft, 0f, 0f, 1f, 0.13f);

            // ── Pile-view overlay (hidden by default) ─────────────────────────────
            _pileViewPanel = MkPanel(root, "PileViewPanel", C(0.05f, 0.05f, 0.10f), 0.1f, 0.15f, 0.9f, 0.90f);
            _pileViewPanel.gameObject.SetActive(false);
            _pileViewText = MkText(_pileViewPanel, 13, Color.white, TextAnchor.UpperLeft, 0f, 0.06f, 1f, 1f);
            MkButton(_pileViewPanel, "Close",
                new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.07f),
                C(0.50f, 0.15f, 0.10f), ClosePileView);
        }

        // ── UI helpers ────────────────────────────────────────────────────────────

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esGO.AddComponent<StandaloneInputModule>();
#endif
        }

        private static RectTransform MkPanel(RectTransform parent, string name, Color bg,
            float ax0, float ay0, float ax1, float ay1)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(ax0, ay0);
            rt.anchorMax = new Vector2(ax1, ay1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = bg;
            return rt;
        }

        private static Text MkText(RectTransform parent, int fontSize, Color color,
            TextAnchor align, float ax0, float ay0, float ax1, float ay1)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(ax0, ay0);
            rt.anchorMax = new Vector2(ax1, ay1);
            rt.offsetMin = new Vector2(6f, 3f);
            rt.offsetMax = new Vector2(-6f, -3f);
            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private static Button MkButton(RectTransform parent, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color bg,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = bg;
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;
            var txt = textGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 16;
            txt.color = Color.white;
            return btn;
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b);

        // ── UI refresh ────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            if (_combat == null) return;
            RefreshEnemy();
            RefreshPlayer();
            RefreshHand();
            RefreshLog();
        }

        private void RefreshEnemy()
        {
            var e = _combat.Enemy;
            if (e == null) return;
            _enemyNameText.text = $"Enemy:  {e.Data.enemyName}";
            _enemyHpText.text   = $"HP  {e.CurrentHealth} / {e.Data.maxHealth}";
            _enemyDiceText.text = "Dice  " + FormatDice(e.Dice.CurrentRoll);

            var sb = new StringBuilder();
            foreach (var p in e.Data.passiveEffects)
                sb.Append($"· {p.description}   ");
            _enemyPassivesText.text = sb.ToString();
        }

        private void RefreshPlayer()
        {
            var p = _combat.Player;
            if (p == null) return;
            _playerHpText.text     = $"HP      {p.CurrentHealth} / {p.MaxHealth}";
            _playerEnergyText.text = $"Energy  {p.Energy.CurrentEnergy} / {p.Energy.MaxEnergy}";
            _playerBlockText.text  = $"Block   {p.Block}";
            _playerDiceText.text   = "Dice  " + FormatDice(p.Dice.CurrentRoll);
            _stateText.text        = $"[{_combat.State}]  Draw {p.Deck.DrawPile.Count}  Discard {p.Deck.DiscardPile.Count}";
            _endTurnButton.interactable = _combat.State == ECombatState.PlayerTurn;
        }

        private void RefreshHand()
        {
            ClearHandContainer();

            var hand = _combat.Player.Deck.Hand;
            if (hand.Count == 0) return;

            bool isPlayerTurn = _combat.State == ECombatState.PlayerTurn;

            for (int i = 0; i < hand.Count; i++)
            {
                var card    = hand[i];
                bool playable = isPlayerTurn && _combat.Player.Energy.CanAfford(card.energyCost);

                // Instantiate from prefab or create from scratch
                GameObject cardGO;
                if (_cardButtonPrefab != null)
                {
                    cardGO = Instantiate(_cardButtonPrefab, _handContainer);
                }
                else
                {
                    cardGO = new GameObject("CardBtn");
                    cardGO.transform.SetParent(_handContainer, false);
                    cardGO.AddComponent<RectTransform>();
                    cardGO.AddComponent<CardButton>();
                }

                cardGO.GetComponent<CardButton>().Setup(card, playable, i, hand.Count, PlayCard);
            }
        }

        private void RefreshLog()
        {
            _logText.text = string.Join("\n", _log);
        }

        private void ClearHandContainer()
        {
            for (int i = _handContainer.childCount - 1; i >= 0; i--)
                Destroy(_handContainer.GetChild(i).gameObject);
        }

        // ── Combat event handlers ─────────────────────────────────────────────────

        private void OnRoundStarted()
        {
            var deck = _combat.Player.Deck;
            Log($"── Round: Player {FormatDice(_combat.Player.Dice.CurrentRoll)}" +
                $"  vs  Enemy {FormatDice(_combat.Enemy.Dice.CurrentRoll)}");
            Log($"   Draw pile: {deck.DrawPile.Count}  |  Discard pile: {deck.DiscardPile.Count}");
            RefreshUI();
        }

        private void OnCombatEnded(bool playerWon)
        {
            Log(playerWon ? "★  VICTORY! ★" : "✕  DEFEATED");
            ClearHandContainer();
            _endTurnButton.interactable = false;
            RefreshLog();
        }

        private void OnEndTurnClicked()
        {
            if (_combat.State != ECombatState.PlayerTurn) return;
            Log("─ End Turn ─");
            _combat.EndPlayerTurn();
        }

        private void PlayCard(CardData card)
        {
            if (!_combat.TryPlayCard(card)) return;
            Log($"Played  {card.cardName}  →  Enemy HP {_combat.Enemy.CurrentHealth}");
            RefreshUI();
        }

        private void OnViewDeckClicked()
        {
            var pile = _combat.Player.Deck.DrawPile;
            var sb = new StringBuilder();
            sb.AppendLine($"Draw Pile  ({pile.Count} cards)");
            sb.AppendLine("──────────────────");
            foreach (var c in pile)
                sb.AppendLine($"· {c.cardName}");
            _pileViewText.text = sb.ToString();
            _pileViewPanel.gameObject.SetActive(true);
        }

        private void OnViewDiscardClicked()
        {
            var pile = _combat.Player.Deck.DiscardPile;
            var sb = new StringBuilder();
            sb.AppendLine($"Discard Pile  ({pile.Count} cards)");
            sb.AppendLine("──────────────────");
            foreach (var c in pile)
                sb.AppendLine($"· {c.cardName}");
            _pileViewText.text = sb.ToString();
            _pileViewPanel.gameObject.SetActive(true);
        }

        private void ClosePileView()
        {
            _pileViewPanel.gameObject.SetActive(false);
        }

        // ── Utility ───────────────────────────────────────────────────────────────

        private void Log(string line)
        {
            _log.Add(line);
            if (_log.Count > MaxLogLines) _log.RemoveAt(0);
        }

        private static string FormatDice(int[] roll)
        {
            if (roll == null || roll.Length == 0) return "(-)";
            return "[" + string.Join("][", roll) + "]";
        }
    }
}
