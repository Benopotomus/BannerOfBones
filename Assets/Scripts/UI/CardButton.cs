using System;
using UnityEngine;
using UnityEngine.UI;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Lightweight MonoBehaviour placed on each card UI element in the player's hand.
    /// CombatRunner instantiates the CardButton prefab and calls <see cref="Setup"/> to
    /// configure size, colour, and click behaviour.  All UGUI components are added at
    /// runtime so the prefab itself stays free of package-specific assets.
    /// </summary>
    [DisallowMultipleComponent]
    public class CardButton : MonoBehaviour
    {
        public CardData Card { get; private set; }

        /// <summary>
        /// Positions this card within the hand container and wires up visuals + click.
        /// </summary>
        /// <param name="card">The card this button represents.</param>
        /// <param name="playable">Whether the player can currently afford and play this card.</param>
        /// <param name="index">Zero-based position in the hand (left to right).</param>
        /// <param name="total">Total number of cards in the hand.</param>
        /// <param name="onClick">Callback invoked when the button is clicked.</param>
        public void Setup(CardData card, bool playable, int index, int total, Action<CardData> onClick)
        {
            Card = card;

            // ── Size / position ───────────────────────────────────────────────────
            var rt = GetComponent<RectTransform>();
            float w = 1f / Mathf.Max(1, total);
            rt.anchorMin = new Vector2(index * w + 0.005f, 0.05f);
            rt.anchorMax = new Vector2((index + 1) * w - 0.005f, 0.95f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // ── Background ────────────────────────────────────────────────────────
            var img = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            img.color = playable
                ? new Color(0.18f, 0.38f, 0.14f)
                : new Color(0.20f, 0.20f, 0.20f);

            // ── Button ────────────────────────────────────────────────────────────
            var btn = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            btn.interactable = playable;
            var captured = card;
            btn.onClick.AddListener(() => onClick(captured));

            // ── Label ─────────────────────────────────────────────────────────────
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(4, 3);
            textRT.offsetMax = new Vector2(-4, -3);

            var txt = textGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 11;
            txt.color = playable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.text = $"<b>{card.cardName}</b>\n{card.description}\n[{card.energyCost}E]";
        }
    }
}
