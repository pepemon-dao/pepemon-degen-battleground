using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pepemon.Store;
using Pepemon.Telemetry;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pepemon.UI
{
    /// <summary>
    /// In-game booster pack store.
    ///
    /// Built entirely in code, like PixelNotice, so it needs no scene wiring. That is not a
    /// stylistic choice: every hard-to-find bug in this project so far has been a serialized
    /// reference left unassigned or a checkbox set the wrong way, none of which is visible when
    /// reading C#, and each of which cost a full cloud build to discover. A screen with no scene
    /// references cannot fail that way.
    /// </summary>
    public class BoosterStoreScreen : MonoBehaviour
    {
        private static readonly Color Scrim = new Color32(0x08, 0x04, 0x14, 0xE6);
        private static readonly Color PanelFill = new Color32(0x1B, 0x10, 0x35, 0xFF);
        private static readonly Color PanelBorder = new Color32(0x7C, 0xE0, 0x4B, 0xFF);
        private static readonly Color CardFill = new Color32(0x24, 0x16, 0x45, 0xFF);
        private static readonly Color TitleColor = new Color32(0x9B, 0xF2, 0x6B, 0xFF);
        private static readonly Color BodyColor = new Color32(0xE8, 0xE8, 0xE8, 0xFF);
        private static readonly Color MutedColor = new Color32(0x9A, 0x8F, 0xB8, 0xFF);
        private static readonly Color BuyFill = new Color32(0x3F, 0xA9, 0x2E, 0xFF);
        private static readonly Color BuyDisabled = new Color32(0x4A, 0x44, 0x5C, 0xFF);
        private static readonly Color CloseFill = new Color32(0x5A, 0x2A, 0x3A, 0xFF);

        private static readonly Color[] RarityColors =
        {
            new Color32(0xFF, 0xD5, 0x4A, 0xFF), // Pepemon
            new Color32(0xC9, 0xC9, 0xC9, 0xFF), // Common
            new Color32(0x6B, 0xC5, 0xF2, 0xFF), // Rare
            new Color32(0xE0, 0x6B, 0xF2, 0xFF), // Epic
        };

        private RectTransform _packRow;
        private RectTransform _revealPanel;
        private RectTransform _revealGrid;
        private TMP_Text _status;
        private TMP_Text _revealTitle;
        private TMP_Text _revealSummary;
        private TMP_FontAsset _font;

        private bool _busy;
        private readonly List<Button> _buyButtons = new List<Button>();

        private static BoosterStoreScreen _instance;

        public static BoosterStoreScreen Instance
        {
            get
            {
                if (_instance == null) _instance = Build();
                return _instance;
            }
        }

        #region construction

        private static TMP_FontAsset GameFont()
        {
            if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            return fonts != null && fonts.Length > 0 ? fonts[0] : null;
        }

        private static BoosterStoreScreen Build()
        {
            var root = new GameObject("BoosterStore");

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the game UI but below PixelNotice, so an error notice still reads on top.
            canvas.sortingOrder = 350;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            var screen = root.AddComponent<BoosterStoreScreen>();
            screen._font = GameFont();

            // Full-screen scrim, so clicks cannot reach the menu behind the store.
            var scrim = Stretch(root.transform, "Scrim");
            scrim.gameObject.AddComponent<Image>().color = Scrim;

            var border = Panel(root.transform, PanelBorder, new Vector2(1560, 860));
            var fill = Stretch(border, "Fill", 6);
            fill.gameObject.AddComponent<Image>().color = PanelFill;

            Label(fill, screen._font, 54, TitleColor, new Vector2(0.5f, 0.93f), new Vector2(1200, 70), "BOOSTER PACKS");
            Label(fill, screen._font, 26, MutedColor, new Vector2(0.5f, 0.86f), new Vector2(1300, 50),
                "Cards mint straight to your wallet. Build them into a deck from the Deck screen.");

            screen._packRow = Row(fill);
            screen._status = Label(fill, screen._font, 30, BodyColor, new Vector2(0.5f, 0.5f), new Vector2(1200, 200), "");

            var close = TextButton(fill, screen._font, CloseFill, new Vector2(0.94f, 0.93f), new Vector2(90, 64), "X", 34);
            close.onClick.AddListener(screen.Close);

            screen.BuildReveal(root.transform);

            DontDestroyOnLoad(root);
            root.SetActive(false);
            return screen;
        }

        private void BuildReveal(Transform parent)
        {
            var border = Panel(parent, PanelBorder, new Vector2(1400, 800));
            _revealPanel = border;

            var fill = Stretch(border, "Fill", 6);
            fill.gameObject.AddComponent<Image>().color = PanelFill;

            _revealTitle = Label(fill, _font, 56, TitleColor, new Vector2(0.5f, 0.92f), new Vector2(1200, 70), "");
            _revealSummary = Label(fill, _font, 30, BodyColor, new Vector2(0.5f, 0.85f), new Vector2(1200, 50), "");

            var gridGo = new GameObject("Grid", typeof(RectTransform));
            gridGo.transform.SetParent(fill, false);
            _revealGrid = (RectTransform)gridGo.transform;
            _revealGrid.anchorMin = new Vector2(0.5f, 0.45f);
            _revealGrid.anchorMax = new Vector2(0.5f, 0.45f);
            _revealGrid.pivot = new Vector2(0.5f, 0.5f);
            _revealGrid.sizeDelta = new Vector2(1240, 520);

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140, 200);
            grid.spacing = new Vector2(14, 14);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;

            var done = TextButton(fill, _font, BuyFill, new Vector2(0.5f, 0.09f), new Vector2(420, 76), "NICE", 32);
            done.onClick.AddListener(() =>
            {
                _revealPanel.gameObject.SetActive(false);
                Refresh();
            });

            border.gameObject.SetActive(false);
        }

        #endregion

        #region ui primitives

        private static RectTransform Stretch(Transform parent, string name, float inset = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            return rt;
        }

        private static RectTransform Panel(Transform parent, Color color, Vector2 size)
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return rt;
        }

        private static RectTransform Row(Transform parent)
        {
            var go = new GameObject("PackRow", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.46f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1440, 560);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 32;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            return rt;
        }

        private static TMP_Text Label(Transform parent, TMP_FontAsset font, int size, Color color,
            Vector2 anchor, Vector2 box, string text)
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
            t.text = text ?? string.Empty;
            return t;
        }

        private static Button TextButton(Transform parent, TMP_FontAsset font, Color fill, Vector2 anchor,
            Vector2 size, string caption, int fontSize)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = fill;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            Label(rt, font, fontSize, Color.white, new Vector2(0.5f, 0.5f), size, caption);
            return btn;
        }

        #endregion

        #region flow

        public void Open()
        {
            gameObject.SetActive(true);
            _revealPanel.gameObject.SetActive(false);
            Funnel.Track(Funnel.StoreOpened);
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private async void Refresh()
        {
            ClearPacks();

            if (!PepemonBoosterPack.IsConfigured)
            {
                // Honest empty state. The contract may simply not be deployed on this network
                // yet, and that is not the player's problem to decode from an exception.
                SetStatus("Booster packs are not live on this network yet.\nCheck back soon.");
                return;
            }

            string account = null;
            try
            {
                account = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[store] Could not read wallet address: {ex.Message}");
            }

            if (string.IsNullOrEmpty(account))
            {
                SetStatus("Connect your wallet to buy packs.");
                PixelNotice.Instance.Show(
                    "No wallet connected",
                    "Connect your wallet to buy booster packs.",
                    "CONNECT WALLET",
                    async () =>
                    {
                        if (Web3Controller.instance == null) return;
                        await Web3Controller.instance.ConnectWallet();
                        if (Web3Controller.instance.IsConnected) Refresh();
                    });
                return;
            }

            SetStatus("Loading packs...");

            List<PepemonBoosterPack.Tier> tiers;
            try
            {
                tiers = await PepemonBoosterPack.GetTiers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[store] Could not load tiers: {ex.Message}");
                SetStatus("Could not reach the store. Please retry.");
                return;
            }

            if (tiers == null || tiers.Count == 0)
            {
                SetStatus("No packs are on sale right now.");
                return;
            }

            SetStatus("");
            foreach (var tier in tiers) AddPackCard(tier);
        }

        private void ClearPacks()
        {
            _buyButtons.Clear();
            if (_packRow == null) return;

            for (var i = _packRow.childCount - 1; i >= 0; i--)
            {
                Destroy(_packRow.GetChild(i).gameObject);
            }
        }

        private void SetStatus(string message)
        {
            if (_status == null) return;
            _status.text = message ?? string.Empty;
            _status.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private void AddPackCard(PepemonBoosterPack.Tier tier)
        {
            var go = new GameObject($"Pack{tier.Id}", typeof(RectTransform));
            go.transform.SetParent(_packRow, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(440, 520);
            go.AddComponent<Image>().color = CardFill;

            Label(rt, _font, 38, TitleColor, new Vector2(0.5f, 0.88f), new Vector2(400, 60), tier.Name);
            Label(rt, _font, 24, MutedColor, new Vector2(0.5f, 0.78f), new Vector2(380, 60), tier.Tagline);
            Label(rt, _font, 46, BodyColor, new Vector2(0.5f, 0.62f), new Vector2(380, 70), $"{tier.CardCount} CARDS");
            Label(rt, _font, 24, BodyColor, new Vector2(0.5f, 0.48f), new Vector2(390, 100), tier.Contents);
            Label(rt, _font, 40, TitleColor, new Vector2(0.5f, 0.28f), new Vector2(380, 60), tier.PriceEth);

            var buy = TextButton(rt, _font, BuyFill, new Vector2(0.5f, 0.12f), new Vector2(360, 76), "BUY", 32);
            var captured = tier;
            buy.onClick.AddListener(() => Buy(captured));
            _buyButtons.Add(buy);

            Funnel.Track(Funnel.PackTierViewed, "tier", tier.Id, "price_wei", tier.PriceWei.ToString());
        }

        private void SetBuyEnabled(bool enabled)
        {
            _busy = !enabled;
            foreach (var button in _buyButtons)
            {
                if (button == null) continue;
                button.interactable = enabled;
                var image = button.targetGraphic as Image;
                if (image != null) image.color = enabled ? BuyFill : BuyDisabled;
            }
        }

        private async void Buy(PepemonBoosterPack.Tier tier)
        {
            if (_busy) return;

            SetBuyEnabled(false);
            SetStatus($"Opening {tier.Name}... confirm in your wallet.");
            Funnel.Track(Funnel.PackPurchaseStarted, "tier", tier.Id);

            var cardIds = new List<ulong>();
            try
            {
                var account = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

                // Balances are snapshotted around the purchase and diffed. The PackOpened event
                // carries the same information, but decoding logs differs between the WebGL
                // bridge and native, and balance reads are already used all over the game.
                var probeIds = await AllCardIds();
                var before = await PepemonFactory.GetOwnedCards(account, probeIds);

                await PepemonBoosterPack.MintPack(tier.Id, tier.PriceWei);

                SetStatus("Opening pack...");
                var after = await PepemonFactory.GetOwnedCards(account, probeIds);
                cardIds = PackLoot.Gained(before, after);

                Funnel.Track(Funnel.PackPurchaseResult, "tier", tier.Id, "success", true, "cards", cardIds.Count);
            }
            catch (Exception ex)
            {
                var message = ex.Message ?? string.Empty;
                var rejected = message.IndexOf("reject", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               message.IndexOf("denied", StringComparison.OrdinalIgnoreCase) >= 0;

                Debug.LogError($"[store] Pack purchase failed: {message}");
                Funnel.Track(Funnel.PackPurchaseResult, "tier", tier.Id, "success", false, "error", message);

                SetStatus("");
                SetBuyEnabled(true);

                PixelNotice.Instance.Show(
                    rejected ? "Purchase cancelled" : "Purchase failed",
                    rejected
                        ? "You cancelled the transaction in your wallet. Nothing was charged."
                        : "The transaction did not go through. You have not been charged for a pack you did not receive.",
                    "OK",
                    () => { });
                return;
            }

            SetBuyEnabled(true);
            SetStatus("");

            if (cardIds.Count == 0)
            {
                // The transaction succeeded but no new balance showed up. Rather than claim a
                // reward we cannot see, say so and point at the deck editor.
                PixelNotice.Instance.Show(
                    "Pack opened",
                    "Your pack went through, but the new cards have not shown up yet. They should appear in the Deck screen shortly.",
                    "OK",
                    () => { });
                return;
            }

            ShowReveal(tier, cardIds);
        }

        /// <summary>Card ids to probe when diffing balances, read from the factory rather than assumed.</summary>
        private static async Task<List<ulong>> AllCardIds()
        {
            ulong last = 49;
            try
            {
                last = await PepemonFactory.GetLastCardId();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[store] Could not read last card id, assuming {last}: {ex.Message}");
            }

            var ids = new List<ulong>();
            for (ulong id = 1; id <= last; id++) ids.Add(id);
            return ids;
        }

        #endregion

        #region reveal

        private void ShowReveal(PepemonBoosterPack.Tier tier, List<ulong> cardIds)
        {
            Funnel.Track(Funnel.PackRevealed, "tier", tier.Id, "cards", cardIds.Count,
                "summary", PackLoot.Summarise(cardIds));

            _revealTitle.text = "YOU PULLED";
            _revealSummary.text = PackLoot.Summarise(cardIds);

            for (var i = _revealGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(_revealGrid.GetChild(i).gameObject);
            }

            foreach (var cardId in cardIds) AddRevealCard(cardId);

            _revealPanel.gameObject.SetActive(true);
        }

        private void AddRevealCard(ulong cardId)
        {
            var go = new GameObject($"Card{cardId}", typeof(RectTransform));
            go.transform.SetParent(_revealGrid, false);

            var rarity = PackLoot.RarityOf(cardId);
            go.AddComponent<Image>().color = RarityColors[(int)rarity];

            var inner = Stretch(go.transform, "Inner", 4);
            var art = inner.gameObject.AddComponent<Image>();
            art.color = CardFill;

            var texture = SafeImage(cardId);
            if (texture != null)
            {
                art.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                art.color = Color.white;
            }

            var name = CardName(cardId);
            Label(inner, _font, 15, BodyColor, new Vector2(0.5f, 0.07f), new Vector2(128, 40), name);
        }

        private static Texture2D SafeImage(ulong cardId)
        {
            try
            {
                return PepemonFactoryCardCache.GetImage(cardId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CardName(ulong cardId)
        {
            try
            {
                var metadata = PepemonFactoryCardCache.GetMetadata(cardId);
                if (metadata != null && !string.IsNullOrEmpty(metadata.Value.name)) return metadata.Value.name;
            }
            catch (Exception)
            {
                // Falls through to the id, which is still better than a blank card.
            }

            return $"#{cardId}";
        }

        #endregion
    }
}
