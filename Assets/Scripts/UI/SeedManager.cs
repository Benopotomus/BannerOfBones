using UnityEngine;
using UnityEngine.UI;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Place this component in the sample scene alongside <see cref="CombatRunner"/>.
    /// It renders a small seed-input overlay that lets you type a Banner of Bones seed
    /// before combat begins.  Clicking "Start" (or leaving the field empty for a random
    /// seed) calls <see cref="BoBRandom.Init"/> so that every downstream dice roll and
    /// deck shuffle is fully reproducible.
    ///
    /// The overlay destroys itself once combat is started so it doesn't interfere with
    /// the combat UI.
    /// </summary>
    [RequireComponent(typeof(CombatRunner))]
    public class SeedManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────

        [Header("Seed Settings")]
        [Tooltip("Pre-fill a fixed seed here (non-zero). Leave 0 to show the input UI at runtime.")]
        public int inspectorSeed = 0;

        // ── Private ───────────────────────────────────────────────────────────────

        private const string BuiltinFont = "LegacyRuntime.ttf";

        private CombatRunner _runner;
        private Canvas        _overlayCanvas;
        private InputField    _seedInputField;
        private Text          _activeSeedLabel;
        private GameObject    _overlayBox;

        private void Awake()
        {
            _runner = GetComponent<CombatRunner>();

            // If a seed was already baked into the Inspector, apply it immediately
            // and let CombatRunner.Start() proceed without showing the overlay.
            if (inspectorSeed != 0)
            {
                BoBRandom.Init(inspectorSeed);
                return;
            }

            // Block CombatRunner from starting until the player confirms the seed.
            _runner.enabled = false;
            BuildOverlay();
        }

        // ── Overlay Construction ──────────────────────────────────────────────────

        private void BuildOverlay()
        {
            var cgo = new GameObject("SeedOverlayCanvas");
            _overlayCanvas = cgo.AddComponent<Canvas>();
            _overlayCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            _overlayCanvas.sortingOrder = 100; // always on top

            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            cgo.AddComponent<GraphicRaycaster>();

            var root = _overlayCanvas.GetComponent<RectTransform>();

            // Dark semi-transparent background
            var bg = MkPanel(root, "OverlayBg", new Color(0f, 0f, 0f, 0.85f), 0f, 0f, 1f, 1f);

            // Card-style box in the centre
            var box = MkPanel(bg, "Box", new Color(0.10f, 0.10f, 0.18f, 1f), 0.30f, 0.35f, 0.70f, 0.65f);
            _overlayBox = box.gameObject;

            // Title
            var title = MkText(box, "Title", 22, new Color(0.95f, 0.80f, 0.30f), TextAnchor.MiddleCenter,
                               0f, 0.72f, 1f, 1f);
            title.text = "Banner of Bones — Enter Seed";

            // Helper
            var hint = MkText(box, "Hint", 12, new Color(0.75f, 0.75f, 0.75f), TextAnchor.MiddleCenter,
                              0f, 0.56f, 1f, 0.72f);
            hint.text = "Enter an integer seed for a reproducible run.\nLeave blank for a random seed.";

            // Input field
            _seedInputField = MkInputField(box, "SeedInput", 0.10f, 0.40f, 0.90f, 0.56f);
            _seedInputField.placeholder.GetComponent<Text>().text = "e.g. 12345";
            _seedInputField.contentType = InputField.ContentType.IntegerNumber;

            // Start button
            MkButton(box, "Start", 0.25f, 0.10f, 0.75f, 0.32f,
                     new Color(0.15f, 0.50f, 0.20f), OnStartClicked);

            // Active-seed readout (shown after a run starts, for reference)
            _activeSeedLabel = MkText(root, "ActiveSeedLabel", 11, new Color(0.55f, 0.55f, 0.55f),
                                      TextAnchor.LowerRight, 0f, 0f, 1f, 0.04f);
            _activeSeedLabel.text = string.Empty;
        }

        // ── Button Handler ────────────────────────────────────────────────────────

        private void OnStartClicked()
        {
            string text = _seedInputField != null ? _seedInputField.text.Trim() : string.Empty;

            if (!string.IsNullOrEmpty(text) && int.TryParse(text, out int parsed))
                BoBRandom.Init(parsed);
            else
                BoBRandom.InitRandom();

            // Show persistent seed label so the player always knows the active seed.
            if (_activeSeedLabel != null)
                _activeSeedLabel.text = $"Seed: {BoBRandom.Seed}";

            // Hide the input box then hand control back to CombatRunner.
            if (_overlayBox != null) _overlayBox.SetActive(false);

            _runner.enabled = true;
            // CombatRunner.Start() will be called by Unity on the next frame now that it is enabled.
        }

        // ── UI Helpers ────────────────────────────────────────────────────────────

        private static RectTransform MkPanel(RectTransform parent, string name, Color color,
                                             float x0, float y0, float x1, float y1)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private static Text MkText(RectTransform parent, string name, int fontSize, Color color,
                                   TextAnchor anchor, float x0, float y0, float x1, float y1)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t   = go.AddComponent<Text>();
            t.font      = Resources.GetBuiltinResource<Font>(BuiltinFont);
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = anchor;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return t;
        }

        private static InputField MkInputField(RectTransform parent, string name,
                                               float x0, float y0, float x1, float y1)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.26f);
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Text child
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textComp = textGo.AddComponent<Text>();
            textComp.font      = Resources.GetBuiltinResource<Font>(BuiltinFont);
            textComp.fontSize  = 18;
            textComp.color     = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4f, 0f);
            textRt.offsetMax = new Vector2(-4f, 0f);

            // Placeholder child
            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(go.transform, false);
            var phComp = phGo.AddComponent<Text>();
            phComp.font      = Resources.GetBuiltinResource<Font>(BuiltinFont);
            phComp.fontSize  = 18;
            phComp.color     = new Color(0.6f, 0.6f, 0.6f);
            phComp.alignment = TextAnchor.MiddleCenter;
            phComp.fontStyle = FontStyle.Italic;
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(4f, 0f);
            phRt.offsetMax = new Vector2(-4f, 0f);

            var field            = go.AddComponent<InputField>();
            field.textComponent  = textComp;
            field.placeholder    = phComp;

            return field;
        }

        private static void MkButton(RectTransform parent, string label,
                                     float x0, float y0, float x1, float y1,
                                     Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go  = new GameObject(label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.AddComponent<Text>();
            t.font      = Resources.GetBuiltinResource<Font>(BuiltinFont);
            t.text      = label;
            t.fontSize  = 16;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            var textRt  = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            btn.onClick.AddListener(onClick);
        }
    }
}
