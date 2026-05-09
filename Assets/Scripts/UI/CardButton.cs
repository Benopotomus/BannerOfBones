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
        /// <param name="interactable">Whether the player can click this card right now.</param>
        /// <param name="index">Zero-based position in the hand (left to right).</param>
        /// <param name="total">Total number of cards in the hand.</param>
        /// <param name="onClick">Callback invoked when the button is clicked.</param>
        public void Setup(CardData card, bool interactable, int index, int total, Action<CardData> onClick,
            bool highlighted = false, string descriptionText = null, string targetLabel = null)
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
            img.color = highlighted
                ? new Color(0.16f, 0.28f, 0.44f)
                : interactable
                    ? new Color(0.18f, 0.38f, 0.14f)
                    : new Color(0.20f, 0.20f, 0.20f);

            // ── Button ────────────────────────────────────────────────────────────
            var btn = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            btn.interactable = interactable;
            btn.onClick.RemoveAllListeners();
            var captured = card;
            btn.onClick.AddListener(() => onClick(captured));

            // ── Energy cost badge (upper left) ────────────────────────────────────
            var badgeGO = new GameObject("CostBadge");
            badgeGO.transform.SetParent(transform, false);
            var badgeRT = badgeGO.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0f, 0.72f);
            badgeRT.anchorMax = new Vector2(0.28f, 1f);
            badgeRT.offsetMin = new Vector2(3f, -3f);
            badgeRT.offsetMax = new Vector2(-1f, -3f);
            badgeGO.AddComponent<Image>().color = new Color(0.10f, 0.16f, 0.32f);

            var badgeTextGO = new GameObject("CostText");
            badgeTextGO.transform.SetParent(badgeGO.transform, false);
            var badgeTextRT = badgeTextGO.AddComponent<RectTransform>();
            badgeTextRT.anchorMin = Vector2.zero;
            badgeTextRT.anchorMax = Vector2.one;
            badgeTextRT.offsetMin = badgeTextRT.offsetMax = Vector2.zero;
            var badgeTxt = badgeTextGO.AddComponent<Text>();
            badgeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            badgeTxt.text = card.energyCost.ToString();
            badgeTxt.fontSize = 13;
            badgeTxt.color = new Color(0.9f, 0.85f, 0.30f);
            badgeTxt.alignment = TextAnchor.MiddleCenter;
            badgeTxt.fontStyle = FontStyle.Bold;
            badgeTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            badgeTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // ── Label (main card body) ─────────────────────────────────────────────
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
            txt.color = interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            string durationText = card.duration == ECardDuration.Instant ? string.Empty : $" {card.duration}";
            string targetText = string.IsNullOrEmpty(targetLabel) ? string.Empty : $"\n<color=#F8E27A>{targetLabel}</color>";
            string renderedDescription = string.IsNullOrWhiteSpace(descriptionText) ? card.description : descriptionText;
            txt.text = $"<b>{card.cardName}</b>\n{renderedDescription}{(string.IsNullOrEmpty(durationText) ? string.Empty : $"\n[{durationText}]")}{targetText}";
        }
    }
}
