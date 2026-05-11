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
        public enum RunMode
        {
            SingleCombat,
            Progression,
        }

        [Header("Player Settings")]
        public int playerHealth = 30;
        public int playerEnergy = 3;

        [Header("Run Settings")]
        [Tooltip("Set a fixed seed for reproducible runs. Leave 0 to keep an existing external seed, or use a random seed if none exists.")]
        public int seed = 0;
        [Tooltip("SingleCombat starts one fight. Progression chains fights and offers path choices after each victory.")]
        public RunMode runMode = RunMode.SingleCombat;
        [Min(1)]
        [Tooltip("How many combats to clear in progression mode.")]
        public int progressionCombatCount = 3;
        [Range(2, 4)]
        [Tooltip("How many path options to offer between combats in progression mode.")]
        public int progressionPathChoices = 3;

        [Header("Prefabs")]
        [Tooltip("Optional CardButton prefab. Assign for custom card styling; leave empty to use the built-in fallback.")]
        [SerializeField] private GameObject _cardButtonPrefab;

        private CombatManager _combat;

        private RectTransform _enemyGroupContainer;
        private RectTransform _enemyAreaPanel;
        private RectTransform _playerAreaPanel;
        private Text _playerHpText;
        private Text _playerEnergyText;
        private Text _playerBlockText;
        private Text _playerDiceText;
        private Text _stateText;
        private Text _logText;
        private RectTransform _playerDiceButtonsContainer;
        private RectTransform _handContainer;
        private RectTransform _dragLayer;
        private Button _focusButton;
        private Button _braceButton;
        private Button _scoutButton;
        private Button _tuneButton;
        private Button _cancelButton;
        private Button _confirmDiceButton;
        private Button _endTurnButton;
        private Text _actionTooltipText;

        private RectTransform _pileViewPanel;
        private Text _pileViewTitleText;
        private RectTransform _pileViewCardsContainer;
        private ScrollRect _pileViewScrollRect;
        private RectTransform _progressionPanel;
        private Text _progressionTitleText;
        private RectTransform _progressionOptionsContainer;
        private RectTransform _optionsPanel;
        private RectTransform _closeGameConfirmPanel;

        private readonly List<string> _log = new List<string>();
        private readonly List<ProgressionOption> _progressionOptions = new List<ProgressionOption>();
        private readonly List<RectTransform> _enemyDropTargets = new List<RectTransform>();
        private const int MaxLogLines = 8;
        private static readonly Vector2 DragPreviewSize = new Vector2(240f, 180f);

        private GameObject _activeDragCard;

        private int _runCombatIndex;
        private int _runPlayerHealth;

        private struct ProgressionOption
        {
            public string Label;
            public int MaxEnemies;
        }

        private void Start()
        {
            if (seed != 0)
                BoBRandom.Init(seed);
            else if (!BoBRandom.IsSeeded)
                BoBRandom.InitRandom();

            BuildUI();
            _runCombatIndex = 0;
            _runPlayerHealth = Mathf.Max(1, playerHealth);

            StartEncounter(EnemyCatalog.CreateEncounterGroup(GetInitialEncounterMaxEnemies()));
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

            _enemyAreaPanel = MkPanel(root, "EnemyArea", C(0.18f, 0.08f, 0.08f), 0.04f, 0.56f, 0.96f, 0.98f);
            var enemyLabel = MkText(_enemyAreaPanel, 18, C(1f, 0.40f, 0.40f), TextAnchor.MiddleCenter, 0f, 0.86f, 1f, 1f);
            enemyLabel.text = "— E N E M I E S —";
            _enemyGroupContainer = MkContainer(_enemyAreaPanel, "EnemyGroup", 0f, 0f, 1f, 0.86f, 10f, 8f, -10f, -8f);

            _playerAreaPanel = MkPanel(root, "PlayerArea", C(0.08f, 0.12f, 0.18f), 0.04f, 0.40f, 0.34f, 0.54f);
            _playerHpText = MkText(_playerAreaPanel, 16, C(0.40f, 1f, 0.40f), TextAnchor.UpperLeft, 0f, 0.48f, 0.50f, 1f);
            _playerBlockText = MkText(_playerAreaPanel, 14, C(0.50f, 0.8f, 1f), TextAnchor.UpperLeft, 0.50f, 0.48f, 1f, 1f);
            _playerEnergyText = MkText(_playerAreaPanel, 14, C(0.40f, 0.6f, 1f), TextAnchor.UpperLeft, 0f, 0.22f, 1f, 0.54f);
            _stateText = MkText(_playerAreaPanel, 10, C(0.7f, 0.7f, 0.7f), TextAnchor.UpperLeft, 0f, 0f, 1f, 0.30f);

            var playerDicePanel = MkPanel(root, "PlayerDiceBg", C(0.06f, 0.06f, 0.10f), 0.36f, 0.40f, 0.96f, 0.54f);
            _playerDiceText = MkText(playerDicePanel, 13, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0f, 0.72f, 1f, 1f);
            _playerDiceButtonsContainer = MkContainer(playerDicePanel, "PlayerDiceButtons", 0f, 0f, 1f, 0.74f, 10f, 6f, -10f, -6f);

            MkPanel(root, "HandBg", C(0.07f, 0.12f, 0.07f), 0.04f, 0.12f, 0.96f, 0.38f);
            var handLabel = MkText(root, 12, Color.white, TextAnchor.MiddleCenter, 0.04f, 0.34f, 0.96f, 0.38f);
            handLabel.text = "— H A N D —";
            _handContainer = MkContainer(root, "HandCards", 0.04f, 0.12f, 0.96f, 0.34f, 6f, 2f, -6f, -2f);

            MkPanel(root, "ActionBar", C(0.04f, 0.04f, 0.04f), 0.04f, 0.03f, 0.96f, 0.11f);
            MkButton(root, "View Deck", new Vector2(0.05f, 0.04f), new Vector2(0.17f, 0.10f), C(0.15f, 0.35f, 0.55f), OnViewDeckClicked);
            MkButton(root, "View Discard", new Vector2(0.18f, 0.04f), new Vector2(0.30f, 0.10f), C(0.35f, 0.20f, 0.45f), OnViewDiscardClicked);
            _focusButton = MkButton(root, "Focus", new Vector2(0.31f, 0.04f), new Vector2(0.38f, 0.10f), C(0.18f, 0.38f, 0.14f), OnFocusClicked);
            _braceButton = MkButton(root, "Brace", new Vector2(0.39f, 0.04f), new Vector2(0.46f, 0.10f), C(0.18f, 0.38f, 0.14f), OnBraceClicked);
            _scoutButton = MkButton(root, "Scout", new Vector2(0.47f, 0.04f), new Vector2(0.54f, 0.10f), C(0.18f, 0.38f, 0.14f), OnScoutClicked);
            _tuneButton = MkButton(root, "Tune", new Vector2(0.55f, 0.04f), new Vector2(0.62f, 0.10f), C(0.18f, 0.38f, 0.14f), OnTuneClicked);
            _cancelButton = MkButton(root, "Cancel", new Vector2(0.63f, 0.04f), new Vector2(0.70f, 0.10f), C(0.50f, 0.15f, 0.10f), OnCancelClicked);
            MkButton(root, "Options", new Vector2(0.71f, 0.04f), new Vector2(0.79f, 0.10f), C(0.22f, 0.22f, 0.30f), OnOptionsClicked);
            _confirmDiceButton = MkButton(root, "Confirm Dice", new Vector2(0.80f, 0.04f), new Vector2(0.90f, 0.10f), C(0.16f, 0.28f, 0.44f), OnConfirmDiceClicked);
            _endTurnButton = MkButton(root, "End Turn", new Vector2(0.91f, 0.03f), new Vector2(0.96f, 0.11f), C(0.60f, 0.15f, 0.10f), OnEndTurnClicked);
            _actionTooltipText = MkText(root, 11, C(0.9f, 0.9f, 0.95f), TextAnchor.MiddleCenter, 0.04f, 0.105f, 0.96f, 0.125f);
            _actionTooltipText.text = string.Empty;
            AttachHoverTooltip(_focusButton, "Focus: Spend 1 energy to reroll up to 3 player dice.");
            AttachHoverTooltip(_braceButton, "Brace: Spend 1 energy to gain 2 block.");
            AttachHoverTooltip(_scoutButton, "Scout: Spend 2 energy to discard 1 card, then draw 2.");
            AttachHoverTooltip(_tuneButton, $"Tune: Spend {CombatManager.TuneEnergyCost} energy to raise up to {CombatManager.TuneMaxDiceTargets} player dice by 1.");

            MkPanel(root, "LogBg", C(0.04f, 0.04f, 0.07f), 0.04f, 0f, 0.96f, 0.03f);
            _logText = MkText(root, 11, C(0.75f, 0.75f, 0.75f), TextAnchor.UpperLeft, 0.04f, 0f, 0.96f, 0.03f);

            _pileViewPanel = MkPanel(root, "PileViewPanel", C(0.05f, 0.05f, 0.10f), 0.1f, 0.15f, 0.9f, 0.90f);
            _pileViewPanel.gameObject.SetActive(false);
            _pileViewTitleText = MkText(_pileViewPanel, 16, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0f, 0.92f, 1f, 1f);
            (_pileViewScrollRect, _pileViewCardsContainer) = MkScrollView(_pileViewPanel, "PileCards", 0f, 0.08f, 1f, 0.92f, 8f, 4f, -8f, -4f);
            MkButton(_pileViewPanel, "Close", new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.07f), C(0.50f, 0.15f, 0.10f), ClosePileView);

            _progressionPanel = MkPanel(root, "ProgressionPanel", C(0.05f, 0.05f, 0.10f), 0.20f, 0.20f, 0.80f, 0.78f);
            _progressionPanel.gameObject.SetActive(false);
            _progressionTitleText = MkText(_progressionPanel, 18, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0f, 0.86f, 1f, 1f);
            _progressionOptionsContainer = MkContainer(_progressionPanel, "ProgressionOptions", 0.06f, 0.12f, 0.94f, 0.84f);

            _optionsPanel = MkPanel(root, "OptionsPanel", C(0.05f, 0.05f, 0.10f), 0.34f, 0.24f, 0.66f, 0.52f);
            _optionsPanel.gameObject.SetActive(false);
            var optionsTitle = MkText(_optionsPanel, 18, C(1f, 0.90f, 0.30f), TextAnchor.MiddleCenter, 0f, 0.70f, 1f, 1f);
            optionsTitle.text = "Options";
            MkButton(_optionsPanel, "Close Game", new Vector2(0.20f, 0.30f), new Vector2(0.80f, 0.56f), C(0.55f, 0.18f, 0.14f), OnCloseGameSelected);
            MkButton(_optionsPanel, "Back", new Vector2(0.20f, 0.08f), new Vector2(0.80f, 0.24f), C(0.22f, 0.22f, 0.30f), CloseOptionsPanel);

            _closeGameConfirmPanel = MkPanel(root, "CloseGameConfirmPanel", C(0f, 0f, 0f), 0f, 0f, 1f, 1f);
            _closeGameConfirmPanel.gameObject.SetActive(false);
            _closeGameConfirmPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);
            var confirmDialog = MkPanel(_closeGameConfirmPanel, "CloseGameDialog", C(0.12f, 0.08f, 0.10f), 0.30f, 0.34f, 0.70f, 0.66f);
            var confirmText = MkText(confirmDialog, 16, Color.white, TextAnchor.MiddleCenter, 0.08f, 0.44f, 0.92f, 0.88f);
            confirmText.text = "Close the game?";
            MkButton(confirmDialog, "Cancel", new Vector2(0.10f, 0.12f), new Vector2(0.46f, 0.34f), C(0.25f, 0.25f, 0.30f), HideCloseGameConfirmation);
            MkButton(confirmDialog, "Close Game", new Vector2(0.54f, 0.12f), new Vector2(0.90f, 0.34f), C(0.55f, 0.18f, 0.14f), ConfirmCloseGame);

            _dragLayer = MkContainer(root, "DragLayer", 0f, 0f, 1f, 1f);
            _dragLayer.SetAsLastSibling();
        }

        private int GetRunTargetCombatCount()
        {
            return runMode == RunMode.Progression
                ? Mathf.Max(1, progressionCombatCount)
                : 1;
        }

        private int GetInitialEncounterMaxEnemies()
        {
            return runMode == RunMode.Progression ? 2 : 4;
        }

        private void StartEncounter(List<EnemyData> enemies)
        {
            if (_combat != null)
            {
                _combat.OnStateChanged -= OnCombatStateChanged;
                _combat.OnRoundStarted -= OnRoundStarted;
                _combat.OnCombatEnded -= OnCombatEnded;
                _combat.OnLogMessage -= Log;
            }

            var deck = CardCatalog.CreateStarterDeck();
            _combat = new CombatManager();
            _combat.OnStateChanged += OnCombatStateChanged;
            _combat.OnRoundStarted += OnRoundStarted;
            _combat.OnCombatEnded += OnCombatEnded;
            _combat.OnLogMessage += Log;

            _runCombatIndex++;
            HideProgressionSelection();
            _combat.StartCombat(enemies, deck, _runPlayerHealth, playerEnergy);

            Log($"=== Combat {_runCombatIndex}/{GetRunTargetCombatCount()} — {enemies.Count} enemies appear! ===");
            foreach (var enemy in enemies)
                Log($"• {enemy.enemyName}: {enemy.description}");
            RefreshUI();
        }

        private void OnCombatStateChanged(ECombatState _)
        {
            RefreshUI();
        }

        private void ShowProgressionSelection()
        {
            if (runMode != RunMode.Progression || _progressionPanel == null)
                return;

            int remainingCombats = Mathf.Max(0, GetRunTargetCombatCount() - _runCombatIndex);
            if (remainingCombats <= 0)
                return;

            _progressionTitleText.text = $"Choose your path ({remainingCombats} combat{(remainingCombats == 1 ? string.Empty : "s")} left)";
            _progressionOptions.Clear();

            int choices = Mathf.Clamp(progressionPathChoices, 2, 4);
            int baseThreat = Mathf.Clamp(2 + (_runCombatIndex / 2), 2, 4);
            for (int i = 0; i < choices; i++)
            {
                int maxEnemies = Mathf.Clamp(baseThreat + i - 1, 2, 4);
                string threatLabel = maxEnemies <= 2 ? "Skirmish" : maxEnemies == 3 ? "Battle" : "Gauntlet";
                _progressionOptions.Add(new ProgressionOption
                {
                    Label = $"Path {i + 1}: {threatLabel} ({Mathf.Max(1, maxEnemies - 1)}-{maxEnemies} foes)",
                    MaxEnemies = maxEnemies,
                });
            }

            BuildProgressionOptionButtons();
            _progressionPanel.gameObject.SetActive(true);
        }

        private void BuildProgressionOptionButtons()
        {
            ClearContainer(_progressionOptionsContainer);

            int count = Mathf.Max(1, _progressionOptions.Count);
            for (int i = 0; i < _progressionOptions.Count; i++)
            {
                int optionIndex = i;
                float yTop = 1f - (i * (1f / count));
                float yBottom = 1f - ((i + 1) * (1f / count)) + 0.03f;

                MkButton(_progressionOptionsContainer,
                    _progressionOptions[i].Label,
                    new Vector2(0f, yBottom),
                    new Vector2(1f, yTop),
                    C(0.20f, 0.26f, 0.40f),
                    () => OnProgressionOptionSelected(optionIndex));
            }
        }

        private void OnProgressionOptionSelected(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= _progressionOptions.Count)
                return;

            var option = _progressionOptions[optionIndex];
            Log($"Path chosen: {option.Label}");
            StartEncounter(EnemyCatalog.CreateEncounterGroup(option.MaxEnemies));
        }

        private void HideProgressionSelection()
        {
            if (_progressionPanel == null)
                return;

            _progressionPanel.gameObject.SetActive(false);
            if (_progressionOptionsContainer != null)
                ClearContainer(_progressionOptionsContainer);
            _progressionOptions.Clear();
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

        // Creates a vertical ScrollRect. Returns (scrollRect, contentRectTransform).
        // The content RectTransform is what you parent card items to; its height is
        // set at population time via SetSizeWithCurrentAnchors.
        private static (ScrollRect, RectTransform) MkScrollView(RectTransform parent, string name,
            float ax0, float ay0, float ax1, float ay1,
            float offsetMinX = 0f, float offsetMinY = 0f, float offsetMaxX = 0f, float offsetMaxY = 0f)
        {
            // Scroll root (holds the ScrollRect component + a background image)
            var scrollGO = new GameObject(name);
            scrollGO.transform.SetParent(parent, false);
            var scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(ax0, ay0);
            scrollRT.anchorMax = new Vector2(ax1, ay1);
            scrollRT.offsetMin = new Vector2(offsetMinX, offsetMinY);
            scrollRT.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
            scrollGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // transparent

            // Viewport (masks content)
            var vpGO = new GameObject("Viewport");
            vpGO.transform.SetParent(scrollGO.transform, false);
            var vpRT = vpGO.AddComponent<RectTransform>();
            const float scrollbarWidth = 16f;
            const float viewportScrollbarGap = 2f;
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = new Vector2(-(scrollbarWidth + viewportScrollbarGap), 0f);
            vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            vpGO.AddComponent<Mask>().showMaskGraphic = false;

            // Content (grows vertically to fit all cards)
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(vpGO.transform, false);
            var contentRT = contentGO.AddComponent<RectTransform>();
            // Anchor to top-left; width stretches full viewport; height is set programmatically
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;

            // ScrollRect wiring
            var sr = scrollGO.AddComponent<ScrollRect>();
            sr.viewport = vpRT;
            sr.content = contentRT;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 20f;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // Vertical scrollbar
            var scrollbarGO = new GameObject("ScrollbarVertical");
            scrollbarGO.transform.SetParent(scrollGO.transform, false);
            var scrollbarRT = scrollbarGO.AddComponent<RectTransform>();
            scrollbarRT.anchorMin = new Vector2(1f, 0f);
            scrollbarRT.anchorMax = new Vector2(1f, 1f);
            scrollbarRT.pivot = new Vector2(0.5f, 0.5f);
            scrollbarRT.offsetMin = new Vector2(-scrollbarWidth, 0f);
            scrollbarRT.offsetMax = Vector2.zero;
            scrollbarGO.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 0.9f);

            var slidingAreaGO = new GameObject("Sliding Area");
            slidingAreaGO.transform.SetParent(scrollbarGO.transform, false);
            var slidingAreaRT = slidingAreaGO.AddComponent<RectTransform>();
            slidingAreaRT.anchorMin = Vector2.zero;
            slidingAreaRT.anchorMax = Vector2.one;
            slidingAreaRT.offsetMin = new Vector2(2f, 2f);
            slidingAreaRT.offsetMax = new Vector2(-2f, -2f);

            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(slidingAreaGO.transform, false);
            var handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.anchorMin = Vector2.zero;
            handleRT.anchorMax = Vector2.one;
            handleRT.offsetMin = handleRT.offsetMax = Vector2.zero;
            var handleImage = handleGO.AddComponent<Image>();
            handleImage.color = new Color(0.7f, 0.7f, 0.8f, 1f);

            var scrollbar = scrollbarGO.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRT;
            sr.verticalScrollbar = scrollbar;

            return (sr, contentRT);
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
            _enemyDropTargets.Clear();

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
                _enemyDropTargets.Add(panel);

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
                hpText.text = enemy.Block > 0
                    ? $"HP  {enemy.CurrentHealth} / {enemy.Data.maxHealth}   Block: {enemy.Block}"
                    : $"HP  {enemy.CurrentHealth} / {enemy.Data.maxHealth}";

                var passivesText = MkText(panel, 11, C(0.95f, 0.80f, 0.80f), TextAnchor.UpperLeft, 0f, 0f, 1f, 0.58f);
                passivesText.supportRichText = true;
                passivesText.text = BuildEnemyActionText(enemy);
            }
        }

        private void RefreshPlayer()
        {
            var p = _combat.Player;
            if (p == null) return;

            string prompt = string.IsNullOrEmpty(_combat.PendingPrompt) ? "Choose a card or baseline action." : _combat.PendingPrompt;

            _playerHpText.text = $"HP      {p.CurrentHealth} / {p.MaxHealth}";
            _playerEnergyText.text = $"Energy  {p.Energy.CurrentEnergy} / {p.Energy.MaxEnergy}";
            _playerBlockText.text = $"Block   {p.Block}";
            string playerHandLabel = BuildDiceHandLabel(p.Dice.CurrentRoll);
            _playerDiceText.text = string.IsNullOrEmpty(playerHandLabel)
                ? "Player Dice"
                : $"Player Dice  — {playerHandLabel}";
            _stateText.text =
                $"[{_combat.State}]  Run {_runCombatIndex}/{GetRunTargetCombatCount()}  Enemies {CountAliveEnemies()} / {_combat.Enemies.Count}  Draw {p.Deck.DrawPile.Count}  Discard {p.Deck.DiscardPile.Count}  Exhaust {p.Deck.ExhaustPile.Count}  Wagers {p.ActiveWagers.Count}\n" +
                $"{prompt}";

            bool canUseActions = _combat.CanUseBaselineActions();
            _focusButton.interactable = canUseActions && p.Energy.CanAfford(1);
            _braceButton.interactable = canUseActions && p.Energy.CanAfford(1);
            _scoutButton.interactable = canUseActions && p.Energy.CanAfford(2) && p.Deck.Hand.Count > 0;
            _tuneButton.interactable = canUseActions && p.Energy.CanAfford(CombatManager.TuneEnergyCost);

            _cancelButton.gameObject.SetActive(_combat.HasPendingCardPlay);
            _cancelButton.interactable = _combat.HasPendingCardPlay;

            bool showConfirmButton = _combat.IsAwaitingDiceSelection || _combat.IsAwaitingCardConfirmation;
            _confirmDiceButton.gameObject.SetActive(showConfirmButton);
            _confirmDiceButton.interactable = _combat.IsAwaitingDiceSelection
                ? _combat.SelectedDiceCount > 0 && _combat.PendingDiceSelectionLimit > 1
                : _combat.IsAwaitingCardConfirmation;
            _endTurnButton.interactable = _combat.State == ECombatState.PlayerTurn && !_combat.HasPendingChoice;
            SetButtonLabel(_confirmDiceButton, _combat.IsAwaitingCardConfirmation ? "Confirm Card" : "Confirm Dice");

            RefreshPlayerDiceButtons(_playerDiceButtonsContainer, p.Dice.Pool, ECardTarget.PlayerDice);
        }

        private void RefreshHand()
        {
            ClearContainer(_handContainer);

            var hand = _combat.Player.Deck.Hand;
            if (hand.Count == 0) return;

            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                bool interactable;
                bool highlighted;

                if (_combat.IsAwaitingHandSelection)
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
                    highlighted,
                    BuildCardDescriptionText(card),
                    BuildCardTargetText(card),
                    OnHandCardDragStarted,
                    OnHandCardDragged,
                    OnHandCardDragEnded);
            }
        }

        private void RefreshPlayerDiceButtons(RectTransform container, IReadOnlyList<Die> pool, ECardTarget target)
        {
            ClearContainer(container);
            if (pool == null || pool.Count == 0) return;

            float width = 1f / Mathf.Max(1, pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                int dieIndex = i;
                var die = pool[i];
                bool interactable = _combat.CanSelectDie(target, i);
                bool selected = _combat.IsDieSelected(target, i);

                var go = new GameObject($"Die{i}");
                go.transform.SetParent(container, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(i * width + 0.01f, 0.05f);
                rt.anchorMax = new Vector2((i + 1) * width - 0.01f, 0.95f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                Color baseColor = die.IsTemporary
                    ? TintColor(DieTypeColor(die.Sides), 0.75f)
                    : DieTypeColor(die.Sides);

                var image = go.AddComponent<Image>();
                image.color = selected
                    ? C(0.85f, 0.72f, 0.20f)
                    : interactable
                        ? C(baseColor.r + 0.05f, baseColor.g + 0.05f, baseColor.b + 0.05f)
                        : baseColor;

                var button = go.AddComponent<Button>();
                button.interactable = interactable;
                button.onClick.AddListener(() => OnDieClicked(target, dieIndex));

                // Die type label (small, top of button)
                bool showType = die.Sides != DiceManager.DefaultDieSides || die.IsTemporary;
                if (showType)
                {
                    var typeTxt = MkText(rt, 8, C(1f, 1f, 0.60f), TextAnchor.UpperCenter, 0f, 0.55f, 1f, 1f);
                    typeTxt.text = die.TypeLabel;
                }

                // Die value (large, centred)
                var valTxt = MkText(rt, 16, Color.white, TextAnchor.MiddleCenter, 0f, 0f, 1f, showType ? 0.60f : 1f);
                valTxt.text = die.Value.ToString();
            }
        }

        private static Color DieTypeColor(int sides)
        {
            switch (sides)
            {
                case 4:  return C(0.20f, 0.42f, 0.20f); // green  — weaker die
                case 6:  return C(0.15f, 0.25f, 0.45f); // blue   — standard
                case 8:  return C(0.18f, 0.30f, 0.52f); // lighter blue
                case 10: return C(0.28f, 0.28f, 0.48f); // silver-blue
                case 12: return C(0.35f, 0.18f, 0.52f); // purple — strongest die
                default: return C(0.15f, 0.25f, 0.45f);
            }
        }

        private static Color TintColor(Color c, float factor) =>
            new Color(c.r * factor, c.g * factor, c.b * factor, c.a);

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
            Log($"── Round: Player {FormatDicePool(_combat.Player.Dice.Pool)}");
            foreach (var enemy in _combat.Enemies)
                Log($"   {enemy.Data.enemyName}: HP {enemy.CurrentHealth}  Block {enemy.Block}  {enemy.GetIntentSummary(_combat.Player.Dice.CurrentRoll)}");
            RefreshUI();
        }

        private void OnCombatEnded(bool playerWon)
        {
            Log(playerWon ? "★  VICTORY! ★" : "✕  DEFEATED");
            DestroyActiveDragCard();
            ClearContainer(_handContainer);

            if (!playerWon)
            {
                HideProgressionSelection();
                RefreshLog();
                return;
            }

            if (runMode != RunMode.Progression)
            {
                HideProgressionSelection();
                RefreshLog();
                return;
            }

            _runPlayerHealth = Mathf.Max(1, _combat.Player.CurrentHealth);
            if (_runCombatIndex >= GetRunTargetCombatCount())
            {
                HideProgressionSelection();
                Log("★★ RUN COMPLETE! ★★");
                RefreshLog();
                return;
            }

            ShowProgressionSelection();
            Log("Choose a path to begin the next combat.");
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

        private void OnTuneClicked()
        {
            if (_combat.TryUseTune())
                RefreshUI();
        }

        private void OnCancelClicked()
        {
            if (_combat.CancelPendingCardPlay())
                RefreshUI();
        }

        private void OnConfirmDiceClicked()
        {
            if (_combat.ConfirmPendingChoice())
                RefreshUI();
        }

        private void OnOptionsClicked()
        {
            var optionsPanel = _optionsPanel;
            if (optionsPanel == null)
                return;

            HideCloseGameConfirmation();
            bool showPanel = !optionsPanel.gameObject.activeSelf;
            optionsPanel.gameObject.SetActive(showPanel);
        }

        private void CloseOptionsPanel()
        {
            if (_optionsPanel != null)
                _optionsPanel.gameObject.SetActive(false);

            HideCloseGameConfirmation();
        }

        private void OnCloseGameSelected()
        {
            if (_closeGameConfirmPanel != null)
                _closeGameConfirmPanel.gameObject.SetActive(true);
        }

        private void HideCloseGameConfirmation()
        {
            if (_closeGameConfirmPanel != null)
                _closeGameConfirmPanel.gameObject.SetActive(false);
        }

        private void ConfirmCloseGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnHandCardClicked(CardData card)
        {
            if (!_combat.TryHandleHandCardClick(card))
                return;

            RefreshUI();
        }

        private void OnHandCardDragStarted(CardData card, PointerEventData eventData)
        {
            DestroyActiveDragCard();
            _activeDragCard = CreateDragPreview(card);
            UpdateDragPreviewPosition(eventData.position, GetEventCamera(eventData));
        }

        private void OnHandCardDragged(PointerEventData eventData)
        {
            if (_activeDragCard == null)
                return;

            UpdateDragPreviewPosition(eventData.position, GetEventCamera(eventData));
        }

        private void OnHandCardDragEnded(CardData card, PointerEventData eventData)
        {
            try
            {
                if (_combat == null || !_combat.CanPlayCard(card))
                    return;

                var eventCamera = GetEventCamera(eventData);
                if (!RectTransformUtility.RectangleContainsScreenPoint(_enemyAreaPanel, eventData.position, eventCamera))
                    return;

                if (TryResolveDraggedCard(card, eventData.position, eventCamera))
                    RefreshUI();
            }
            finally
            {
                DestroyActiveDragCard();
            }
        }

        private void OnEnemyClicked(int enemyIndex)
        {
            if (_combat.TrySelectEnemy(enemyIndex))
                RefreshUI();
        }

        private void OnDieClicked(ECardTarget target, int dieIndex)
        {
            if (_combat.TogglePendingDieSelection(target, dieIndex))
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
            _pileViewPanel.gameObject.SetActive(true);
            _pileViewTitleText.text = $"{title}  ({pile.Count} cards)";

            ClearContainer(_pileViewCardsContainer);

            const int cols = 5;
            const float cardSpacing = 8f;
            const int sidePadding = 8;
            float availableWidth = _pileViewScrollRect.viewport.rect.width - (sidePadding * 2f) - ((cols - 1) * cardSpacing);
            float cardWidth = availableWidth > 0f ? availableWidth / cols : 120f;
            float cardHeight = cardWidth * 0.66f;
            int rows = Mathf.CeilToInt(pile.Count / (float)cols);
            float totalHeight = rows > 0
                ? (sidePadding * 2f) + (rows * cardHeight) + ((rows - 1) * cardSpacing)
                : 0f;

            // Resize the content rect so ScrollRect knows how tall the list is
            _pileViewCardsContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            var grid = _pileViewCardsContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = _pileViewCardsContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            grid.spacing = new Vector2(cardSpacing, cardSpacing);
            grid.padding = new RectOffset(sidePadding, sidePadding, sidePadding, sidePadding);
            grid.cellSize = new Vector2(cardWidth, cardHeight);
            grid.childAlignment = TextAnchor.UpperLeft;

            for (int i = 0; i < pile.Count; i++)
            {
                var card = pile[i];
                var cardGO = new GameObject($"PileCard{i}");
                cardGO.transform.SetParent(_pileViewCardsContainer, false);
                var cardRT = cardGO.AddComponent<RectTransform>();

                var cardPanel = cardGO;
                cardPanel.AddComponent<Image>().color = C(0.18f, 0.18f, 0.18f);

                // Energy cost badge (upper left)
                var badgeGO = new GameObject("CostBadge");
                badgeGO.transform.SetParent(cardRT, false);
                var badgeRT = badgeGO.AddComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(0f, 0.72f);
                badgeRT.anchorMax = new Vector2(0.24f, 1f);
                badgeRT.offsetMin = new Vector2(3f, -3f);
                badgeRT.offsetMax = new Vector2(-1f, -3f);
                badgeGO.AddComponent<Image>().color = new Color(0.10f, 0.16f, 0.32f);
                var badgeTxt = MkText(badgeRT, 13, new Color(0.9f, 0.85f, 0.30f), TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);
                badgeTxt.fontStyle = FontStyle.Bold;
                badgeTxt.text = card.energyCost.ToString();

                // Card body text
                string durationText = card.duration == ECardDuration.Instant ? string.Empty : $"\n[{card.duration}]";
                string targetText = BuildCardTargetText(card);
                string targetLine = string.IsNullOrEmpty(targetText) ? string.Empty : $"\n<color=#F8E27A>{targetText}</color>";
                string body = $"<b>{card.cardName}</b>\n{BuildCardDescriptionText(card)}{durationText}{targetLine}";

                var bodyTxt = MkText(cardRT, 11, Color.white, TextAnchor.UpperLeft, 0f, 0f, 1f, 0.72f);
                bodyTxt.supportRichText = true;
                bodyTxt.text = body;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_pileViewCardsContainer);
            _pileViewScrollRect.verticalNormalizedPosition = 1f;
        }

        private void ClosePileView()
        {
            _pileViewPanel.gameObject.SetActive(false);
            ClearContainer(_pileViewCardsContainer);
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

        private bool TryResolveDraggedCard(CardData card, Vector2 screenPosition, Camera eventCamera)
        {
            bool requiresEnemyTarget = CardEffectProcessor.CardRequiresEnemyTarget(card);
            bool targetsAllEnemies = requiresEnemyTarget && card.targetsAllEnemies;

            if (!requiresEnemyTarget || targetsAllEnemies)
            {
                if (!_combat.TryPlayCard(card))
                    return false;

                return !_combat.IsAwaitingCardConfirmation || _combat.ConfirmPendingChoice();
            }

            int targetEnemyIndex = GetEnemyDropTargetIndex(screenPosition, eventCamera);
            if (targetEnemyIndex < 0)
            {
                targetEnemyIndex = GetSingleAliveEnemyIndex();
                if (targetEnemyIndex < 0)
                    return false;
            }

            if (!_combat.TryPlayCard(card))
                return false;

            return !_combat.IsAwaitingEnemySelection || _combat.TrySelectEnemy(targetEnemyIndex);
        }

        private int GetEnemyDropTargetIndex(Vector2 screenPosition, Camera eventCamera)
        {
            for (int i = 0; i < _enemyDropTargets.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(_enemyDropTargets[i], screenPosition, eventCamera))
                    return i;
            }

            return -1;
        }

        private GameObject CreateDragPreview(CardData card)
        {
            var preview = new GameObject("DraggedCard");
            preview.transform.SetParent(_dragLayer, false);

            var canvasGroup = preview.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            var rectTransform = preview.AddComponent<RectTransform>();
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = DragPreviewSize;

            var background = preview.AddComponent<Image>();
            background.color = new Color(0.18f, 0.38f, 0.14f, 0.95f);

            var costBadge = new GameObject("CostBadge");
            costBadge.transform.SetParent(preview.transform, false);
            var costBadgeRect = costBadge.AddComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(0f, 1f);
            costBadgeRect.anchorMax = new Vector2(0.24f, 1f);
            costBadgeRect.pivot = new Vector2(0f, 1f);
            costBadgeRect.anchoredPosition = new Vector2(6f, -6f);
            costBadgeRect.sizeDelta = new Vector2(0f, 28f);
            costBadge.AddComponent<Image>().color = new Color(0.10f, 0.16f, 0.32f, 0.95f);

            var costText = MkText(costBadgeRect, 13, new Color(0.9f, 0.85f, 0.30f), TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);
            costText.fontStyle = FontStyle.Bold;
            costText.text = card.energyCost.ToString();

            var body = MkText(rectTransform, 11, Color.white, TextAnchor.UpperLeft, 0f, 0f, 1f, 1f);
            body.supportRichText = true;
            body.text = BuildDragPreviewText(card);

            return preview;
        }

        private string BuildDragPreviewText(CardData card)
        {
            string targetText = BuildCardTargetText(card);
            string durationText = card.duration == ECardDuration.Instant ? string.Empty : $"\n[{card.duration}]";
            string targetLine = string.IsNullOrEmpty(targetText) ? string.Empty : $"\n<color=#F8E27A>{targetText}</color>";
            return $"<b>{card.cardName}</b>\n{BuildCardDescriptionText(card)}{durationText}{targetLine}";
        }

        private void UpdateDragPreviewPosition(Vector2 screenPosition, Camera eventCamera)
        {
            if (_activeDragCard == null)
                return;

            var rectTransform = _activeDragCard.GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer, screenPosition, eventCamera, out var localPoint))
                return;

            rectTransform.anchoredPosition = localPoint + new Vector2(0f, 40f);
        }

        private void DestroyActiveDragCard()
        {
            if (_activeDragCard == null)
                return;

            Destroy(_activeDragCard);
            _activeDragCard = null;
        }

        private static Camera GetEventCamera(PointerEventData eventData)
        {
            return eventData?.pressEventCamera ?? eventData?.enterEventCamera;
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
                        total += EvaluateDamageForRoll(effect, _combat.Player.Dice.CurrentRoll);
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

        private int GetSingleAliveEnemyIndex()
        {
            int foundIndex = -1;
            for (int i = 0; i < _combat.Enemies.Count; i++)
            {
                if (!_combat.Enemies[i].IsAlive)
                    continue;

                if (foundIndex >= 0)
                    return -1;

                foundIndex = i;
            }

            return foundIndex;
        }

        private string BuildEnemyActionText(EnemyCombatant enemy)
        {
            var passives = enemy?.Data?.passiveEffects;
            if (enemy.CurrentIntent != null)
            {
                var playerDiceRoll = _combat?.Player?.Dice?.CurrentRoll;
                var sb = new StringBuilder();
                sb.AppendLine($"<color=#FFB0B0>{FormatIntentHeadline(enemy, enemy.CurrentIntent, playerDiceRoll)}</color>");
                sb.AppendLine($"  {enemy.GetIntentSummary(playerDiceRoll)}");

                if (enemy.NextIntent != null)
                    sb.AppendLine($"<color=#AAAAAA>Next: {FormatIntentHeadline(enemy, enemy.NextIntent, playerDiceRoll)}</color>");

                AppendPassiveText(sb, passives);

                return sb.ToString().TrimEnd();
            }

            if (passives != null && passives.Count > 0)
            {
                var sb = new StringBuilder();
                AppendPassiveText(sb, passives);
                return sb.ToString().TrimEnd();
            }

            return "No action.";
        }

        private static void AppendPassiveText(StringBuilder sb, IList<EnemyPassiveEffectData> passives)
        {
            if (sb == null || passives == null || passives.Count == 0)
                return;

            sb.AppendLine("<color=#DDB8FF>Passives:</color>");
            foreach (var passive in passives)
            {
                if (passive == null)
                    continue;

                string passiveText = string.IsNullOrWhiteSpace(passive.description)
                    ? passive.effectType.ToString()
                    : passive.description;
                sb.AppendLine($"  • {passiveText}");
            }
        }

        private static string FormatIntentHeadline(EnemyCombatant enemy, EnemyIntentData intent, int[] playerDiceRoll)
        {
            int amount = Mathf.Max(0, intent.magnitude);
            switch (intent.intentType)
            {
                case EEnemyIntentType.AttackFlat:
                {
                    int damage = enemy.CurrentIntent == intent
                        ? enemy.CalculateIntentDamage(playerDiceRoll)
                        : amount;
                    return $"⚔ {intent.intentName} ({damage} dmg)";
                }
                case EEnemyIntentType.Guard:
                    return $"🛡 {intent.intentName} (+{amount} block)";
                case EEnemyIntentType.ShredPlayerBlock:
                    return $"🪓 {intent.intentName} (-{amount} block)";
                case EEnemyIntentType.SapPlayerEnergy:
                    return amount == 1
                        ? $"⚡ {intent.intentName} (-1 die next turn)"
                        : $"⚡ {intent.intentName} (-{amount} dice next turn)";
                default:
                    return $"→ {intent.intentName}";
            }
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

        /// <summary>
        /// Formats a typed dice pool for the log, e.g. "[3][d8:7*][5]".
        /// Standard d6 dice are shown as plain values; other types show the die label.
        /// Temporary dice are marked with *.
        /// </summary>
        private static string FormatDicePool(IReadOnlyList<Die> pool)
        {
            if (pool == null || pool.Count == 0) return "(-)";
            var sb = new StringBuilder();
            for (int i = 0; i < pool.Count; i++)
            {
                var d = pool[i];
                sb.Append('[');
                if (d.Sides != DiceManager.DefaultDieSides || d.IsTemporary)
                    sb.Append($"{d.TypeLabel}:{d.Value}");
                else
                    sb.Append(d.Value);
                sb.Append(']');
            }
            return sb.ToString();
        }
    }
}
