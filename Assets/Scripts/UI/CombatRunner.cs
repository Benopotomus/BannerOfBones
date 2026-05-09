using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Drop this prefab (or component) into any scene and hit Play.
    /// It constructs a full runtime combat UI for the prototype encounter.
    /// </summary>
    public class CombatRunner : MonoBehaviour
    {
        [Header("Player Settings")]
        public int playerHealth = 30;
        public int playerEnergy = 3;

        [Header("Prefabs")]
        [Tooltip("Optional CardButton prefab. Assign for custom card styling; leave empty to use the built-in fallback.")]
        [SerializeField] private GameObject _cardButtonPrefab;

        private CombatManager _combat;

        private RectTransform _enemyGroupContainer;
        private Text _playerHpText;
        private Text _playerEnergyText;
        private Text _playerBlockText;
        private Text _playerDiceText;
        private Text _stateText;
        private Text _logText;
        private RectTransform _playerDiceButtonsContainer;
        private RectTransform _handContainer;
        private Button _focusButton;
        private Button _braceButton;
        private Button _scoutButton;
        private Button _retainButton;
        private Button _confirmDiceButton;
        private Button _endTurnButton;
        private Text _actionTooltipText;

        private RectTransform _pileViewPanel;
        private Text _pileViewText;

        private readonly List<string> _log = new List<string>();
        private const int MaxLogLines = 8;

        private void Start()
        {
            var enemies = EnemyCatalog.CreateEncounterGroup();
            var deck = CardCatalog.CreateStarterDeck();

            _combat = new CombatManager();
            _combat.OnStateChanged += _ => RefreshUI();
            _combat.OnRoundStarted += OnRoundStarted;
            _combat.OnCombatEnded += OnCombatEnded;
            _combat.OnLogMessage += Log;

            BuildUI();

            _combat.StartCombat(enemies, deck, playerHealth, playerEnergy);

            Log($"=== {enemies.Count} enemies appear! ===");
            foreach (var enemy in enemies)
                Log($"• {enemy.enemyName}: {enemy.description}");

            RefreshUI();
        }

        private void BuildUI()
        {
            EnsureEventSystem();

            var cgo = new GameObject("CombatCanvas");
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            cgo.AddComponent<GraphicRaycaster>();

            var root = canvas.GetComponent<RectTransform>();

            MkPanel(root, "Bg", C(0.08f, 0.07f, 0.12f), 0f, 0f, 1f, 1f);

            var ep = MkPanel(root, "EnemyArea", C(0.18f, 0.08f, 0.08f), 0f, 0.66f, 1f, 1f);
            var enemyLabel = MkText(ep, 18, C(1f, 0.40f, 0.40f), TextAnchor.MiddleCenter, 0f, 0.84f, 1f, 1f);
            enemyLabel.text = "— E N E M I E S —";
            _enemyGroupContainer = MkContainer(ep, "EnemyGroup", 0f, 0f, 1f, 0.84f, 8f, 6f, -8f, -6f);

            var pp = MkPanel(root, "PlayerArea", C(0.08f, 0.12f, 0.18f), 0f, 0.41f, 1f, 0.66f);
            _playerHpText = MkText(pp, 16, C(0.40f, 1f, 0.40f), TextAnchor.UpperLeft, 0f, 0.68f, 0.35f, 1f);
            _playerEnergyText = MkText(pp, 14, C(0.40f, 0.6f, 1f), TextAnchor.UpperLeft, 0f, 0.42f, 0.35f, 0.68f);
            _playerBlockText = MkText(pp, 14, C(0.50f, 0.8f, 1f), TextAnchor.UpperLeft, 0f, 0.16f, 0.35f, 0.42f);
            _stateText = MkText(pp, 10, C(0.7f, 0.7f, 0.7f), TextAnchor.UpperLeft, 0.35f, 0f, 1f, 1f);

            MkPanel(root, "PlayerDiceBg", C(0.06f, 0.06f, 0.10f), 0.12f, 0.35f, 0.88f, 0.41f);
            _playerDiceText = MkText(root, 12, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0.12f, 0.385f, 0.88f, 0.41f);
            _playerDiceButtonsContainer = MkContainer(root, "PlayerDiceButtons", 0.18f, 0.355f, 0.82f, 0.385f);

            MkPanel(root, "HandBg", C(0.07f, 0.12f, 0.07f), 0f, 0.17f, 1f, 0.35f);
            var handLabel = MkText(root, 12, Color.white, TextAnchor.MiddleCenter, 0f, 0.33f, 1f, 0.35f);
            handLabel.text = "— H A N D —";
            _handContainer = MkContainer(root, "HandCards", 0f, 0.17f, 1f, 0.33f, 4f, 2f, -4f, -2f);

            MkPanel(root, "ActionBar", C(0.04f, 0.04f, 0.04f), 0f, 0.08f, 1f, 0.17f);
            MkButton(root, "View Deck", new Vector2(0.03f, 0.13f), new Vector2(0.18f, 0.165f), C(0.15f, 0.35f, 0.55f), OnViewDeckClicked);
            MkButton(root, "View Discard", new Vector2(0.20f, 0.13f), new Vector2(0.35f, 0.165f), C(0.35f, 0.20f, 0.45f), OnViewDiscardClicked);
            _focusButton = MkButton(root, "Focus", new Vector2(0.37f, 0.13f), new Vector2(0.50f, 0.165f), C(0.18f, 0.38f, 0.14f), OnFocusClicked);
            _braceButton = MkButton(root, "Brace", new Vector2(0.52f, 0.13f), new Vector2(0.65f, 0.165f), C(0.18f, 0.38f, 0.14f), OnBraceClicked);
            _scoutButton = MkButton(root, "Scout", new Vector2(0.67f, 0.13f), new Vector2(0.80f, 0.165f), C(0.18f, 0.38f, 0.14f), OnScoutClicked);
            _retainButton = MkButton(root, "Retain", new Vector2(0.18f, 0.085f), new Vector2(0.38f, 0.12f), C(0.45f, 0.34f, 0.10f), OnRetainClicked);
            _confirmDiceButton = MkButton(root, "Confirm Dice", new Vector2(0.40f, 0.085f), new Vector2(0.60f, 0.12f), C(0.16f, 0.28f, 0.44f), OnConfirmDiceClicked);
            _endTurnButton = MkButton(root, "End Turn", new Vector2(0.62f, 0.085f), new Vector2(0.82f, 0.12f), C(0.60f, 0.15f, 0.10f), OnEndTurnClicked);
            _actionTooltipText = MkText(root, 11, C(0.9f, 0.9f, 0.95f), TextAnchor.MiddleCenter, 0.03f, 0.12f, 0.97f, 0.13f);
            _actionTooltipText.text = string.Empty;
            AttachHoverTooltip(_focusButton, "Focus: Spend 1 energy to reroll 1 player die.");
            AttachHoverTooltip(_braceButton, "Brace: Spend 1 energy to gain 2 block.");
            AttachHoverTooltip(_scoutButton, "Scout: Spend 2 energy to discard 1 card, then draw 2.");

            MkPanel(root, "LogBg", C(0.04f, 0.04f, 0.07f), 0f, 0f, 1f, 0.08f);
            _logText = MkText(root, 11, C(0.75f, 0.75f, 0.75f), TextAnchor.UpperLeft, 0f, 0f, 1f, 0.08f);

            _pileViewPanel = MkPanel(root, "PileViewPanel", C(0.05f, 0.05f, 0.10f), 0.1f, 0.15f, 0.9f, 0.90f);
            _pileViewPanel.gameObject.SetActive(false);
            _pileViewText = MkText(_pileViewPanel, 13, Color.white, TextAnchor.UpperLeft, 0f, 0.06f, 1f, 1f);
            MkButton(_pileViewPanel, "Close", new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.07f), C(0.50f, 0.15f, 0.10f), ClosePileView);
        }

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

        private static RectTransform MkContainer(RectTransform parent, string name,
            float ax0, float ay0, float ax1, float ay1,
            float offsetMinX = 0f, float offsetMinY = 0f, float offsetMaxX = 0f, float offsetMaxY = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(ax0, ay0);
            rt.anchorMax = new Vector2(ax1, ay1);
            rt.offsetMin = new Vector2(offsetMinX, offsetMinY);
            rt.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
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
            txt.fontSize = 15;
            txt.color = Color.white;
            return btn;
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b);

        private void RefreshUI()
        {
            if (_combat == null) return;
            RefreshEnemies();
            RefreshPlayer();
            RefreshHand();
            RefreshLog();
        }

        private void RefreshEnemies()
        {
            ClearContainer(_enemyGroupContainer);

            var enemies = _combat.Enemies;
            if (enemies == null || enemies.Count == 0) return;

            float width = 1f / Mathf.Max(1, enemies.Count);
            for (int i = 0; i < enemies.Count; i++)
            {
                int enemyIndex = i;
                var enemy = enemies[i];
                bool selectable = _combat.CanSelectEnemy(i);

                var panel = MkPanel(_enemyGroupContainer, $"Enemy{i}",
                    enemy.IsAlive
                        ? selectable ? C(0.48f, 0.23f, 0.13f) : C(0.28f, 0.12f, 0.12f)
                        : C(0.14f, 0.10f, 0.10f),
                    i * width + 0.01f, 0.02f, (i + 1) * width - 0.01f, 0.98f);

                if (selectable)
                {
                    var button = panel.gameObject.AddComponent<Button>();
                    button.onClick.AddListener(() => OnEnemyClicked(enemyIndex));
                }

                var nameText = MkText(panel, 14, Color.white, TextAnchor.UpperLeft, 0f, 0.72f, 1f, 1f);
                nameText.text = enemy.IsAlive
                    ? enemy.Data.enemyName
                    : $"{enemy.Data.enemyName} (Defeated)";

                var hpText = MkText(panel, 12, enemy.IsAlive ? C(1f, 0.80f, 0.80f) : C(0.65f, 0.65f, 0.65f),
                    TextAnchor.UpperLeft, 0f, 0.58f, 1f, 0.74f);
                hpText.text = $"HP  {enemy.CurrentHealth} / {enemy.Data.maxHealth}";

                var diceText = MkText(panel, 10, C(1f, 0.90f, 0.30f), TextAnchor.UpperLeft, 0f, 0.46f, 1f, 0.60f);
                string enemyHandLabel = BuildDiceHandLabel(enemy.Dice.CurrentRoll);
                diceText.text = string.IsNullOrEmpty(enemyHandLabel) ? "Dice" : $"Dice  — {enemyHandLabel}";

                var diceContainer = MkContainer(panel, $"EnemyDice{i}", 0f, 0.26f, 1f, 0.46f, 2f, 0f, -2f, 0f);
                RefreshDiceButtons(diceContainer, enemy.Dice.CurrentRoll, ECardTarget.EnemyDice, C(0.45f, 0.15f, 0.15f), i);

                var passivesText = MkText(panel, 9, C(0.86f, 0.72f, 0.72f), TextAnchor.UpperLeft, 0f, 0f, 1f, 0.28f);
                var sb = new StringBuilder();
                foreach (var passive in enemy.Data.passiveEffects)
                    sb.AppendLine($"• {passive.description}");
                passivesText.text = sb.ToString().TrimEnd();
            }
        }

        private void RefreshPlayer()
        {
            var p = _combat.Player;
            if (p == null) return;

            string retained = p.Deck.RetainedCard != null ? p.Deck.RetainedCard.cardName : "—";
            string prompt = string.IsNullOrEmpty(_combat.PendingPrompt) ? "Choose a card or baseline action." : _combat.PendingPrompt;

            _playerHpText.text = $"HP      {p.CurrentHealth} / {p.MaxHealth}";
            _playerEnergyText.text = $"Energy  {p.Energy.CurrentEnergy} / {p.Energy.MaxEnergy}";
            _playerBlockText.text = $"Block   {p.Block}";
            string playerHandLabel = BuildDiceHandLabel(p.Dice.CurrentRoll);
            _playerDiceText.text = string.IsNullOrEmpty(playerHandLabel)
                ? "Player Dice"
                : $"Player Dice  — {playerHandLabel}";
            _stateText.text =
                $"[{_combat.State}]  Enemies {CountAliveEnemies()} / {_combat.Enemies.Count}  Draw {p.Deck.DrawPile.Count}  Discard {p.Deck.DiscardPile.Count}  Exhaust {p.Deck.ExhaustPile.Count}\n" +
                $"Retain: {retained}  |  Wagers: {p.ActiveWagers.Count}\n" +
                $"{prompt}";

            bool canUseActions = _combat.CanUseBaselineActions();
            _focusButton.interactable = canUseActions && p.Energy.CanAfford(1);
            _braceButton.interactable = canUseActions && p.Energy.CanAfford(1);
            _scoutButton.interactable = canUseActions && p.Energy.CanAfford(2) && p.Deck.Hand.Count > 0;
            bool canRetainSelect = _combat.State == ECombatState.PlayerTurn
                                   && !_combat.IsAwaitingEnemySelection
                                   && !_combat.IsAwaitingDiceSelection
                                   && !_combat.IsAwaitingHandSelection
                                   && !_combat.IsAwaitingCardConfirmation
                                   && p.Deck.Hand.Count > 0;
            _retainButton.interactable = _combat.HasPendingCardPlay || canRetainSelect;

            bool showConfirmButton = _combat.IsAwaitingDiceSelection || _combat.IsAwaitingCardConfirmation;
            _confirmDiceButton.gameObject.SetActive(showConfirmButton);
            _confirmDiceButton.interactable = _combat.IsAwaitingDiceSelection
                ? _combat.SelectedDiceCount > 0 && _combat.PendingDiceSelectionLimit > 1
                : _combat.IsAwaitingCardConfirmation;
            _endTurnButton.interactable = _combat.State == ECombatState.PlayerTurn && !_combat.HasPendingChoice;
            SetButtonLabel(_retainButton, _combat.HasPendingCardPlay ? "Cancel Card" : _combat.IsSelectingRetain ? "Cancel Retain" : "Retain");
            SetButtonLabel(_confirmDiceButton, _combat.IsAwaitingCardConfirmation ? "Confirm Card" : "Confirm Dice");

            RefreshDiceButtons(_playerDiceButtonsContainer, p.Dice.CurrentRoll, ECardTarget.PlayerDice, C(0.15f, 0.25f, 0.45f));
        }

        private void RefreshHand()
        {
            ClearContainer(_handContainer);

            var hand = _combat.Player.Deck.Hand;
            if (hand.Count == 0) return;

            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                bool retained = _combat.Player.Deck.RetainedCard == card;
                bool interactable;
                bool highlighted;

                if (_combat.IsSelectingRetain)
                {
                    interactable = true;
                    highlighted = retained;
                }
                else if (_combat.IsAwaitingHandSelection)
                {
                    interactable = _combat.CanSelectHandCard(card);
                    highlighted = interactable;
                }
                else
                {
                    interactable = _combat.CanPlayCard(card);
                    highlighted = false;
                }

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

                cardGO.GetComponent<CardButton>().Setup(
                    card,
                    interactable,
                    i,
                    hand.Count,
                    OnHandCardClicked,
                    retained,
                    highlighted,
                    BuildCardDescriptionText(card),
                    BuildCardTargetText(card));
            }
        }

        private void RefreshDiceButtons(RectTransform container, int[] roll, ECardTarget target, Color baseColor, int enemyIndex = -1)
        {
            ClearContainer(container);
            if (roll == null || roll.Length == 0) return;

            // Sort display by value ascending so matching dice appear together
            int[] sortedIndices = new int[roll.Length];
            for (int i = 0; i < roll.Length; i++) sortedIndices[i] = i;
            System.Array.Sort(sortedIndices, (a, b) => roll[a].CompareTo(roll[b]));

            float width = 1f / Mathf.Max(1, roll.Length);
            for (int slot = 0; slot < roll.Length; slot++)
            {
                int originalIndex = sortedIndices[slot];
                bool interactable = _combat.CanSelectDie(target, originalIndex, enemyIndex);
                bool selected = _combat.IsDieSelected(target, originalIndex, enemyIndex);

                var go = new GameObject($"Die{originalIndex}");
                go.transform.SetParent(container, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(slot * width + 0.01f, 0.05f);
                rt.anchorMax = new Vector2((slot + 1) * width - 0.01f, 0.95f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                var image = go.AddComponent<Image>();
                image.color = selected
                    ? C(0.85f, 0.72f, 0.20f)
                    : interactable
                        ? C(baseColor.r + 0.05f, baseColor.g + 0.05f, baseColor.b + 0.05f)
                        : baseColor;

                var button = go.AddComponent<Button>();
                button.interactable = interactable;
                button.onClick.AddListener(() => OnDieClicked(target, originalIndex, enemyIndex));

                var txt = MkText(rt, 16, Color.white, TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);
                txt.text = roll[originalIndex].ToString();
            }
        }

        private void RefreshLog()
        {
            _logText.text = string.Join("\n", _log);
        }

        private static void ClearContainer(RectTransform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private void OnRoundStarted()
        {
            Log($"── Round: Player {FormatDice(_combat.Player.Dice.CurrentRoll)}");
            foreach (var enemy in _combat.Enemies)
                Log($"   {enemy.Data.enemyName}: {FormatDice(enemy.Dice.CurrentRoll)}  HP {enemy.CurrentHealth}");
            RefreshUI();
        }

        private void OnCombatEnded(bool playerWon)
        {
            Log(playerWon ? "★  VICTORY! ★" : "✕  DEFEATED");
            ClearContainer(_handContainer);
            RefreshLog();
        }

        private void OnEndTurnClicked()
        {
            _combat.EndPlayerTurn();
            RefreshUI();
        }

        private void OnFocusClicked()
        {
            if (_combat.TryUseFocus())
                RefreshUI();
        }

        private void OnBraceClicked()
        {
            if (_combat.TryUseBrace())
                RefreshUI();
        }

        private void OnScoutClicked()
        {
            if (_combat.TryUseScout())
                RefreshUI();
        }

        private void OnRetainClicked()
        {
            if (_combat.HasPendingCardPlay)
            {
                if (_combat.CancelPendingCardPlay())
                    RefreshUI();
                return;
            }

            if (_combat.ToggleRetainSelection())
                RefreshUI();
        }

        private void OnConfirmDiceClicked()
        {
            if (_combat.ConfirmPendingChoice())
                RefreshUI();
        }

        private void OnHandCardClicked(CardData card)
        {
            bool selectingRetain = _combat.IsSelectingRetain;
            if (!_combat.TryHandleHandCardClick(card))
                return;

            if (selectingRetain)
            {
                var retained = _combat.Player.Deck.RetainedCard;
                Log(retained == null ? "Retain cleared." : $"Retaining {retained.cardName} for next round.");
            }

            RefreshUI();
        }

        private void OnEnemyClicked(int enemyIndex)
        {
            if (_combat.TrySelectEnemy(enemyIndex))
                RefreshUI();
        }

        private void OnDieClicked(ECardTarget target, int dieIndex, int enemyIndex)
        {
            if (_combat.TogglePendingDieSelection(target, dieIndex, enemyIndex))
                RefreshUI();
        }

        private void OnViewDeckClicked()
        {
            ShowPileView("Draw Pile", _combat.Player.Deck.DrawPile);
        }

        private void OnViewDiscardClicked()
        {
            ShowPileView("Discard Pile", _combat.Player.Deck.DiscardPile);
        }

        private void ShowPileView(string title, IReadOnlyList<CardData> pile)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{title}  ({pile.Count} cards)");
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

        private void Log(string line)
        {
            _log.Add(line);
            if (_log.Count > MaxLogLines)
                _log.RemoveAt(0);
        }

        private static void SetButtonLabel(Button button, string text)
        {
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
        }

        private void AttachHoverTooltip(Button button, string tooltipText)
        {
            if (button == null)
                return;

            var trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            if (trigger.triggers == null)
                trigger.triggers = new List<EventTrigger.Entry>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => _actionTooltipText.text = tooltipText);
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => _actionTooltipText.text = string.Empty);
            trigger.triggers.Add(exit);
        }

        private string BuildCardDescriptionText(CardData card)
        {
            var sb = new StringBuilder(card.description);

            int totalDamage = EstimateCardDamage(card);
            int totalBlock = EstimateCardBlock(card);

            if (totalDamage > 0)
                sb.Append($"\nDamage ({totalDamage})");

            if (totalBlock > 0)
                sb.Append($"\nBlock ({totalBlock})");

            return sb.ToString();
        }

        private string BuildCardTargetText(CardData card)
        {
            bool requiresEnemyTarget = CardEffectProcessor.CardRequiresEnemyTarget(card);
            if (requiresEnemyTarget && !card.targetsAllEnemies)
                return "To one target";

            if (requiresEnemyTarget && card.targetsAllEnemies)
                return "All Enemies";

            return string.Empty;
        }

        private int EstimateCardDamage(CardData card)
        {
            int total = 0;
            foreach (var effect in card.effects)
            {
                switch (effect.effectType)
                {
                    case EEffectType.DealDamage:
                    case EEffectType.ConditionalDamage:
                    {
                        if (effect.diceTarget == ECardTarget.PlayerDice)
                        {
                            total += EvaluateDamageForRoll(effect, _combat.Player.Dice.CurrentRoll);
                        }
                        else if (card.targetsAllEnemies)
                        {
                            foreach (var enemy in _combat.Enemies)
                            {
                                if (!enemy.IsAlive) continue;
                                total += EvaluateDamageForRoll(effect, enemy.Dice.CurrentRoll);
                            }
                        }
                        else
                        {
                            var enemy = _combat.Enemy;
                            if (enemy != null && enemy.IsAlive)
                                total += EvaluateDamageForRoll(effect, enemy.Dice.CurrentRoll);
                        }

                        break;
                    }
                }
            }

            return total;
        }

        private static int EvaluateDamageForRoll(CardEffectData effect, int[] diceRoll)
        {
            int triggers = PokerEvaluator.EvaluateTriggerCount(
                effect.triggerOn, diceRoll, effect.dieValue, effect.valueThreshold);

            return effect.effectType == EEffectType.ConditionalDamage
                ? triggers > 0 ? effect.magnitude : 0
                : triggers * effect.magnitude;
        }

        private int EstimateCardBlock(CardData card)
        {
            int total = 0;
            foreach (var effect in card.effects)
            {
                if (effect.effectType != EEffectType.GainBlock)
                    continue;

                int triggers = PokerEvaluator.EvaluateTriggerCount(
                    effect.triggerOn, _combat.Player.Dice.CurrentRoll, effect.dieValue, effect.valueThreshold);
                total += triggers * effect.magnitude;
            }

            return total;
        }

        private int CountAliveEnemies()
        {
            int alive = 0;
            foreach (var enemy in _combat.Enemies)
            {
                if (enemy.IsAlive)
                    alive++;
            }

            return alive;
        }

        private static string FormatDice(int[] roll)
        {
            if (roll == null || roll.Length == 0) return "(-)";
            int[] sorted = (int[])roll.Clone();
            System.Array.Sort(sorted);
            return "[" + string.Join("][", sorted) + "]";
        }

        /// <summary>
        /// Returns a short label describing the best hand in the dice pool
        /// (e.g. "Pair of 3s", "Triple 5s", "Full House"), or an empty string if
        /// no recognisable pattern is present.
        /// </summary>
        private static string BuildDiceHandLabel(int[] roll)
        {
            if (roll == null || roll.Length < 2) return string.Empty;

            if (PokerEvaluator.CountFiveOfAKind(roll) > 0)
                return $"Five {FindGroupValue(roll, 5)}s!";

            if (PokerEvaluator.CountFourOfAKind(roll) > 0)
                return $"Four {FindGroupValue(roll, 4)}s";

            if (PokerEvaluator.CountFullHouses(roll) > 0)
                return "Full House";

            if (PokerEvaluator.CountTriples(roll) > 0)
                return $"Triple {FindGroupValue(roll, 3)}s";

            int pairs = PokerEvaluator.CountPairs(roll);
            if (pairs >= 2)
                return "Two Pairs";
            if (pairs == 1)
                return $"Pair of {FindGroupValue(roll, 2)}s";

            if (PokerEvaluator.HasStraight(roll))
                return "Straight";

            return string.Empty;
        }

        /// <summary>Returns the highest die value whose frequency is >= <paramref name="groupSize"/>.</summary>
        private static int FindGroupValue(int[] roll, int groupSize)
        {
            int[] freq = new int[7];
            foreach (int d in roll)
                if (d >= 1 && d <= 6) freq[d]++;
            for (int i = 6; i >= 1; i--)
                if (freq[i] >= groupSize) return i;
            return 0;
        }
    }
}
