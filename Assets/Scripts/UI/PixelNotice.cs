using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pepemon.UI
{
    /// <summary>
    /// Shared on-brand notice panel for status, empty and error states.
    ///
    /// Replaces bare centred text drawn straight over gameplay, which read as unfinished. It
    /// also carries an optional action button, so a wallet-gated empty state can offer the way
    /// out instead of only describing it - previously the deck screen told players to connect
    /// a wallet without giving them anything to press.
    ///
    /// Built in code so it needs no scene wiring and cannot silently fail on a missing
    /// reference. It borrows the game's own TMP pixel font at runtime rather than shipping a
    /// second font, so it matches the surrounding UI.
    /// </summary>
    public class PixelNotice : MonoBehaviour
    {
        private static readonly Color PanelFill = new Color32(0x1B, 0x10, 0x35, 0xF2);
        private static readonly Color PanelBorder = new Color32(0x7C, 0xE0, 0x4B, 0xFF);
        private static readonly Color TitleColor = new Color32(0x9B, 0xF2, 0x6B, 0xFF);
        private static readonly Color BodyColor = new Color32(0xF2, 0xF2, 0xF2, 0xFF);
        private static readonly Color ButtonFill = new Color32(0x3F, 0xA9, 0x2E, 0xFF);

        private RectTransform _panel;
        private TMP_Text _title;
        private TMP_Text _body;
        private Button _action;
        private TMP_Text _actionLabel;

        private Action _onAction;
        private float _hideAtUnscaled = float.MaxValue;

        private static PixelNotice _instance;

        public static PixelNotice Instance
        {
            get
            {
                if (_instance == null) _instance = Build();
                return _instance;
            }
        }

        /// <summary>The game's own pixel font, so notices match the rest of the UI.</summary>
        private static TMP_FontAsset GameFont()
        {
            if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
            return Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
        }

        private static PixelNotice Build()
        {
            var root = new GameObject("PixelNotice");

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            var notice = root.AddComponent<PixelNotice>();
            var font = GameFont();

            // Border, then inset fill: a chunky two-tone frame reads as pixel art without
            // needing a sliced sprite.
            var border = Panel(root.transform, PanelBorder, new Vector2(980, 300));
            notice._panel = border;

            var fill = Panel(border, PanelFill, Vector2.zero);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = new Vector2(6, 6);
            fill.offsetMax = new Vector2(-6, -6);

            notice._title = Label(fill, font, 44, TitleColor, new Vector2(0.5f, 0.80f), new Vector2(900, 60));
            notice._body = Label(fill, font, 30, BodyColor, new Vector2(0.5f, 0.52f), new Vector2(900, 110));

            notice._action = ActionButton(fill, font, out notice._actionLabel);
            notice._action.onClick.AddListener(() =>
            {
                var cb = notice._onAction;
                notice.Hide();
                cb?.Invoke();
            });

            DontDestroyOnLoad(root);
            root.SetActive(false);
            return notice;
        }

        private static RectTransform Panel(Transform parent, Color color, Vector2 size)
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            if (size != Vector2.zero) rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return rt;
        }

        private static TMP_Text Label(Transform parent, TMP_FontAsset font, int size, Color color,
            Vector2 anchor, Vector2 box)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = box;

            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            return t;
        }

        private static Button ActionButton(Transform parent, TMP_FontAsset font, out TMP_Text label)
        {
            var go = new GameObject("ActionButton", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.18f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(420, 76);

            var img = go.AddComponent<Image>();
            img.color = ButtonFill;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            label = Label(rt, font, 32, Color.white, new Vector2(0.5f, 0.5f), new Vector2(400, 60));
            return btn;
        }

        /// <param name="actionLabel">Button caption. Null hides the button.</param>
        /// <param name="autoHideSeconds">0 keeps it up until dismissed or replaced.</param>
        public void Show(string title, string body, string actionLabel = null, Action onAction = null,
            float autoHideSeconds = 0f)
        {
            gameObject.SetActive(true);

            if (_title != null) _title.text = title ?? string.Empty;
            if (_body != null) _body.text = body ?? string.Empty;

            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionLabel) && onAction != null;
            if (_action != null) _action.gameObject.SetActive(hasAction);
            if (hasAction && _actionLabel != null) _actionLabel.text = actionLabel;

            // Realtime, so a notice cannot be held on screen by a frozen timescale.
            _hideAtUnscaled = autoHideSeconds > 0f
                ? Time.unscaledTime + autoHideSeconds
                : float.MaxValue;
        }

        public void Hide()
        {
            _hideAtUnscaled = float.MaxValue;
            _onAction = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Time.unscaledTime >= _hideAtUnscaled) Hide();
        }
    }
}
