using UnityEngine;
using UnityEngine.UI;

namespace Pepemon.Onboarding
{
    /// <summary>
    /// Full-screen progress overlay for the starter-pack claim.
    ///
    /// Built entirely in code rather than authored in a scene. The claim runs on the menu
    /// scene right after a scene load, and the existing status label was never wired up, so
    /// every progress and failure message went to the console only - the player signed five
    /// wallet transactions with nothing on screen explaining any of them.
    ///
    /// Uses legacy uGUI Text with a built-in font so it has no serialized dependencies and
    /// cannot silently fail the way a missing TMP font asset would.
    /// </summary>
    public class ClaimProgressOverlay : MonoBehaviour
    {
        private Text _title;
        private Text _step;
        private Text _hint;

        private static ClaimProgressOverlay _instance;

        public static ClaimProgressOverlay Instance
        {
            get
            {
                if (_instance == null) _instance = Create();
                return _instance;
            }
        }

        private static ClaimProgressOverlay Create()
        {
            var root = new GameObject("ClaimProgressOverlay");

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above every gameplay canvas, including the tutorial panel at 15.
            canvas.sortingOrder = 500;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            var overlay = root.AddComponent<ClaimProgressOverlay>();

            // Dim the game behind so it is obvious the claim owns the screen.
            var backdrop = NewChild(root.transform, "Backdrop");
            var backdropImage = backdrop.gameObject.AddComponent<Image>();
            backdropImage.color = new Color(0f, 0f, 0f, 0.82f);
            Stretch(backdrop);

            var font = BuiltinFont();

            overlay._title = NewLabel(root.transform, "Title", font, 46, FontStyle.Bold,
                new Vector2(0.5f, 0.62f), new Vector2(1400, 70));
            overlay._step = NewLabel(root.transform, "Step", font, 34, FontStyle.Normal,
                new Vector2(0.5f, 0.50f), new Vector2(1400, 120));
            overlay._hint = NewLabel(root.transform, "Hint", font, 26, FontStyle.Italic,
                new Vector2(0.5f, 0.38f), new Vector2(1400, 60));

            overlay._hint.color = new Color(1f, 1f, 1f, 0.7f);

            DontDestroyOnLoad(root);
            root.SetActive(false);

            return overlay;
        }

        private static Font BuiltinFont()
        {
            // LegacyRuntime.ttf is the built-in font from 2021.2 onward; Arial.ttf is the
            // older name. Try both so this cannot end up with an invisible label.
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Text NewLabel(Transform parent, string name, Font font, int size,
            FontStyle style, Vector2 anchor, Vector2 size2d)
        {
            var rt = NewChild(parent, name);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size2d;

            var text = rt.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        /// <summary>Shows the overlay and sets the headline.</summary>
        public void Show(string title)
        {
            gameObject.SetActive(true);
            if (_title != null) _title.text = title;
            if (_step != null) _step.text = string.Empty;
            if (_hint != null) _hint.text = string.Empty;
        }

        /// <summary>
        /// Names the action being performed and whether the player is expected to sign.
        /// The player must always be able to tell what a wallet prompt is for.
        /// </summary>
        public void SetStep(int index, int total, string action, bool requiresSignature)
        {
            gameObject.SetActive(true);

            if (_step != null) _step.text = $"Step {index} of {total}\n{action}";
            if (_hint != null)
            {
                _hint.text = requiresSignature
                    ? "Confirm in your wallet to continue"
                    : "Working - no signature needed";
            }
        }

        public void SetResult(string message, bool success)
        {
            gameObject.SetActive(true);

            if (_title != null) _title.text = success ? "Starter pack claimed" : "Claim unfinished";
            if (_step != null) _step.text = message;
            if (_hint != null) _hint.text = success ? string.Empty : "You can retry from the Deck screen";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
