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

            var result = await StarterPackClaim.Run((ulong)pepemonId, supportCardIds, SetClaimStatus);

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
                FailClaim(result.Error ?? "unknown", "Claim failed. Try again from the Deck screen.");
                return;
            }

            // The faucet has fired and cannot be undone - re-running it would mint twice.
            // Record the claim even if assembly failed, and tell the player what is left to do.
            OnboardingState.HasClaimedStarterPack = true;
            claimedStarterDeck = true;

            SetClaimStatus(result.DeckIsPlayable
                ? "Starter pack claimed - your deck is ready!"
                : "Cards minted. Finish your deck in the Deck screen.");

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
