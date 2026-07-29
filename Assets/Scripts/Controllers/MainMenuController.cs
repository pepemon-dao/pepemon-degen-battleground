using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pepemon.Battle;
using Pepemon.Onboarding;
using Pepemon.Telemetry;
using Scripts.Managers.Sound;
using Sirenix.OdinInspector;
using Thirdweb;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string creditsURL;
    public Web3Controller web3;
    public List<GameObject> menuScreens;

    public GameObject _selectDeckListLoader;

    public ScreenManageDecks _screenManageDecks;
    public ScreenLeaderboard _screenLeaderboard;

    public Button _connectWalletButton;
    public Button _startGameButton;
    public Button _manageDecksButton;
    public Button _leaderboardButton;
    public Button _creditsButton;
    public Button _mintDeckButton;

    public int defaultScreenId = 0;
    private int screenNavigationPosition = 0;
    private int[] screenNavigationHistory = new int[10];

    private int selectedLeagueId = 0;
    private ulong selectedDeckId = 0;

    public static bool claimedStarterDeck = false;

    /// <summary>Optional status label for the starter-pack claim. Null-safe when unwired.</summary>
    [SerializeField] private GameObject _claimStatusMessage;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Clear any freeze left behind by leaving the battle scene mid-tutorial or mid-preview,
        // which would otherwise hold this scene at timeScale 0.
        TimeControl.ResetAll();

        Funnel.Track(Funnel.AppLoaded);

        //PostBattleScreenController.IsClaimingGift = true; //- for testing the gift mechanic with the deck manager
        // TODO: find a better way to handle re-loading the main scene

        HandleGoingBackToMenu();
        _connectWalletButton.onClick.AddListener(OnConnectWalletButtonClick);
        // Note: _mintDeckButton is intentionally not wired to OnConnectWalletButtonClick here.
        // MintDeckButtonHandler already connects the wallet as part of its own flow, and having
        // both listeners on one button fired two concurrent ConnectWallet() calls per click.
        _startGameButton.onClick.AddListener(OnStartGameButtonClick);
        _manageDecksButton.onClick.AddListener(OnManageDecksButtonClick);
        _leaderboardButton.onClick.AddListener(OnLeaderboardButtonClick);
        _creditsButton.onClick.AddListener(OpenCredits);
    }

    private void HandleGoingBackToMenu()
    {
        if (PepemonFactoryCardCache.CardsIds.Count == 0)
        {
            DeInitMainScene(true);
        }
        if (PostBattleScreenController.IsPlayingAgain)
        {
            PostBattleScreenController.IsPlayingAgain = false;
            DeInitMainScene(false);

            ShowScreen(MainSceneScreensEnum.LeagueSelection);
        }
        if (PostBattleScreenController.IsGoingFromBattle)
        {
            PostBattleScreenController.IsGoingFromBattle = false;
            DeInitMainScene(false);
        }


        if (PostBattleScreenController.IsClaimingGift)
        {
            PostBattleScreenController.IsClaimingGift = false;
            
            //claim gift
            ClaimStarterDeck();
        }
    }

    /// <summary>
    /// Mints the player's starter pack.
    ///
    /// This used to be a stub that logged an error and set the "claimed" flag anyway, so the
    /// player connected a wallet - the highest-friction step in the funnel - and received
    /// nothing, permanently marked as having claimed.
    ///
    /// Two transactions, using the same permissionless faucet the edit-deck screen already
    /// ships: mintCards() grants the cards, createDeck() grants the deck NFT to hold them.
    /// Assembling the chosen starter into that deck needs approval plus two more writes and
    /// is deferred until the on-chain card ids are confirmed to match the local ScriptableObjects.
    ///
    /// The claimed flag is only set after both transactions confirm. A rejected or reverted
    /// claim leaves the player claimable, so replaying the bot battle offers CLAIM again.
    /// </summary>
    private async void ClaimStarterDeck()
    {
        if (OnboardingState.HasClaimedStarterPack)
        {
            Debug.Log("[claim] Starter pack already claimed, nothing to do.");
            return;
        }

        Funnel.Track(Funnel.ClaimClicked);
        SetClaimStatus("Claiming your starter pack...");

        try
        {
            if (Web3Controller.instance == null)
            {
                FailClaim("no_web3_controller", "Wallet unavailable. Try again from the Deck screen.");
                return;
            }

            if (!Web3Controller.instance.IsConnected)
            {
                SetClaimStatus("Connecting wallet...");
                Funnel.Track(Funnel.WalletConnectRequested);
                await Web3Controller.instance.ConnectWallet();
            }

            if (!Web3Controller.instance.IsConnected)
            {
                Funnel.Track(Funnel.WalletConnectResult, "success", false, "error", "not_connected");
                FailClaim("wallet_not_connected", "Wallet not connected. Try again from the Deck screen.");
                return;
            }

            Funnel.Track(Funnel.WalletConnectResult, "success", true);

            var address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                FailClaim("no_address", "No wallet address found. Try again from the Deck screen.");
                return;
            }

            // The choice is read from persisted state, not Web3Controller: the post-battle
            // screen zeroes StarterPepemonID/StarterDeckID before this scene loads.
            var pepemonId = OnboardingState.PendingStarterPepemonId;
            if (pepemonId <= 0)
            {
                Debug.LogWarning("[claim] No starter Pepemon recorded, defaulting to Fafny.");
                pepemonId = 1;
            }

            var starterDeckId = (ulong)OnboardingState.PendingStarterDeckId;

            // Include inactive: the prep controller lives on a menu screen that is not the
            // one currently shown.
            var prep = FindObjectOfType<BattlePrepController>(true);
            var supportCardIds = prep != null
                ? prep.GetStarterSupportCardIds(starterDeckId)
                : new List<ulong>();

            if (prep == null)
            {
                Debug.LogWarning("[claim] BattlePrepController not found; deck will be created without support cards.");
            }

            // Runtime-built overlay: the serialized status label was never wired, so all of
            // this previously reached the console only and the player signed five wallet
            // prompts with nothing on screen telling them what any of them were for.
            var overlay = ClaimProgressOverlay.Instance;
            overlay.Show("Claiming your starter pack");

            var result = await StarterPackClaim.Run(
                (ulong)pepemonId,
                supportCardIds,
                (step, total, action, requiresSignature) =>
                {
                    overlay.SetStep(step, total, action, requiresSignature);
                    SetClaimStatus(action);
                });

            Funnel.Track(Funnel.MintResult, new Dictionary<string, object>
            {
                ["success"] = result.AssetsGranted,
                ["deck_playable"] = result.DeckIsPlayable,
                ["support_cards"] = result.SupportCardsAdded,
                ["battle_card_set"] = result.BattleCardSet,
                ["error"] = result.Error ?? string.Empty
            });

            if (!result.AssetsGranted)
            {
                // Nothing irreversible happened, so leave the claim available to retry.
                overlay.SetResult("Claim did not complete. Nothing was charged.", success: false);
                await Cysharp.Threading.Tasks.UniTask.Delay(4000, Cysharp.Threading.Tasks.DelayType.Realtime);
                overlay.Hide();

                FailClaim(result.Error ?? "unknown", "Claim failed. Try again from the Deck screen.");
                return;
            }

            // The faucet has fired and cannot be undone - re-running it would mint twice.
            // Record the claim even if assembly failed, and tell the player what is left to do.
            OnboardingState.HasClaimedStarterPack = true;
            claimedStarterDeck = true;

            var summary = result.DeckIsPlayable
                ? "Your deck is ready to battle."
                : "Cards minted. Finish your deck in the Deck screen.";

            overlay.SetResult(summary, success: true);
            SetClaimStatus(summary);

            await Cysharp.Threading.Tasks.UniTask.Delay(3500, Cysharp.Threading.Tasks.DelayType.Realtime);
            overlay.Hide();

            if (!result.DeckIsPlayable)
            {
                Debug.LogWarning($"[claim] Deck not fully assembled: battleCard={result.BattleCardSet} " +
                                 $"supportCards={result.SupportCardsAdded} error={result.Error}");
            }

            // Explicit null check rather than ?. - Unity's fake-null does not short-circuit.
            if (_screenManageDecks != null) _screenManageDecks.ReloadAllDecks();
        }
        catch (Exception e)
        {
            Funnel.Track(Funnel.MintResult, "success", false, "error", e.Message);

            var overlay = ClaimProgressOverlay.Instance;
            overlay.SetResult("Something went wrong. You can try again.", success: false);
            await Cysharp.Threading.Tasks.UniTask.Delay(4000, Cysharp.Threading.Tasks.DelayType.Realtime);
            overlay.Hide();

            FailClaim(e.Message, "Claim failed. Try again from the Deck screen.");
        }
    }

    private void FailClaim(string reason, string userMessage)
    {
        Debug.LogError($"[claim] Starter pack claim failed: {reason}");
        SetClaimStatus(userMessage);
    }

    private void SetClaimStatus(string message)
    {
        Debug.Log($"[claim] {message}");

        if (_claimStatusMessage == null) return;

        _claimStatusMessage.SetActive(!string.IsNullOrEmpty(message));

        var label = _claimStatusMessage.GetComponent<TMPro.TMP_Text>();
        if (label != null) label.text = message;
    }

    private async void DeInitMainScene(bool toLoadScreen)
    {
        // assume that when no account was selected and his scene loads, its because the game just launched

        /*
        if (Web3Controller.instance == null || Web3Controller.instance.SelectedAccountAddress == null)
        {
            _startGameButton.interactable = true;
            ShowScreen(defaultScreenId);
        }
        // assume that when an account was already selected, this scene was loaded after a battle that just ended
        else
        {
            ShowScreen(MainSceneScreensEnum.Menu);
            _startGameButton.interactable = true;
            _manageDecksButton.interactable = true;
            _leaderboardButton.interactable = true;
        }

        */

        // Show load screen in order to load cards into the PepemonFactoryCardCache.
        // only necessary when launching the game from the battle scene
        if (toLoadScreen)
        {
            _startGameButton.interactable = true;
            ShowScreen(defaultScreenId);
        }
        else
        {
            ShowScreen(MainSceneScreensEnum.Menu);
            _startGameButton.interactable = true;
            if (Web3Controller.instance != null && await ThirdwebManager.Instance.SDK.Wallet.GetAddress() != null)
            {
                _manageDecksButton.interactable = true;
                _leaderboardButton.interactable = true;
            }
        }
    }

    // TODO: use this method for all ShowScreen calls
    public void ShowScreen(MainSceneScreensEnum screen) => ShowScreen((int) screen);

    public void ShowScreen(int screenId)
    {
        if (screenId < 0)
        {
            int nextPosition = screenNavigationPosition + screenId;
            screenId = screenNavigationHistory[nextPosition % screenNavigationHistory.Length];
            screenNavigationPosition = (nextPosition - 1) % screenNavigationHistory.Length;
        }
        
        if (screenId == (int)MainSceneScreensEnum.LeagueSelection && !Web3Controller.instance.IsConnected)
        {
            screenId = (int)MainSceneScreensEnum.Tutorial;
        }

        for (int i = 0; i < menuScreens.Count; i++)
        {
            menuScreens[i].SetActive(i == screenId);
        }

        // Populate the battle deck picker.
        //
        // Nothing ever did this: _selectDeckListLoader was declared and never referenced, and
        // BattlePrepController only subscribes to the list's onSelectDeck event without ever
        // filling it. The PvE/PvP deck selection screen was therefore empty by construction -
        // no decks to pick, so no battle could ever be started.
        //
        // Done here rather than at a call site so every route into the screen is covered,
        // including the ShowScreen calls wired directly to buttons in the scene.
        if (screenId == (int)MainSceneScreensEnum.DeckSelection)
        {
            // Search the screen being shown, and treat _selectDeckListLoader only as a fallback.
            //
            // The order matters, and getting it wrong is what kept this broken. The field is not
            // unassigned: it points at a DeckList that sits at the scene root with no parent, and
            // whose _deckList output target is inside Screen_5_ManageDecksNew. Every deck it
            // loaded was parented into the Mint Deck screen's list, which is why that screen
            // filled up while the picker stayed empty. Preferring the field meant the search
            // below never ran, because a wrong reference is not a null one.
            //
            // The loader that actually lives under Screen_4_DeckSelection is already wired
            // correctly, so finding it is the whole fix and the scene needs no surgery.
            DeckListLoader selectionLoader = null;
            if (screenId < menuScreens.Count && menuScreens[screenId] != null)
            {
                selectionLoader = menuScreens[screenId].GetComponentInChildren<DeckListLoader>(true);
            }

            if (selectionLoader == null && _selectDeckListLoader != null)
            {
                selectionLoader = _selectDeckListLoader.GetComponent<DeckListLoader>();
            }

            if (selectionLoader != null)
            {
                // Select mode, never Edit: this screen exists to pick a deck to fight with.
                selectionLoader.SetEditMode(false);
                selectionLoader.ReloadAllDecks(force: true);
            }
            else
            {
                Debug.LogError("[decks] No DeckListLoader found on the deck selection screen; " +
                               "the battle deck picker cannot be filled.");
            }
        }

        screenNavigationPosition = (screenNavigationPosition + 1) % screenNavigationHistory.Length;
        screenNavigationHistory[screenNavigationPosition] = screenId;
    }

    public void SelectLeague(int leagueId)
    {
        selectedLeagueId = leagueId;
    }

    public void StartGame()
    {
        // Matchmaking
        // 

        Debug.Log($"Start matchmaking with league: {selectedLeagueId} and deck {selectedDeckId}");
    }

    public void ProceedToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void OpenCredits()
    {
        Application.OpenURL(creditsURL);
    }

    public void ToggleAudio(bool enable)
    {
        ThemePlayer.Instance.ToggleAudio(enable);
    }

    public async void OnConnectWalletButtonClick()
    {
        if (!Web3Controller.instance.IsConnected)
        {
            await Web3Controller.instance.ConnectWallet();
        }
    }

    public void OnStartGameButtonClick()
    {
        Funnel.Track(
            Funnel.StartPressed,
            "connected", Web3Controller.instance != null && Web3Controller.instance.IsConnected);

        ShowScreen(MainSceneScreensEnum.LeagueSelection);
    }

    public void OnManageDecksButtonClick()
    {
        _screenManageDecks.ReloadAllDecks();
        ShowScreen(MainSceneScreensEnum.ManageDecks);
    }

    public void OnLeaderboardButtonClick()
    {
        _screenLeaderboard.ReloadDefaultLeaderboard();
        ShowScreen(MainSceneScreensEnum.Leaderboard);
    }

    public void UnMuteThemePlayer()
    {
        ThemePlayer.Instance.UnMute();
    }
}
