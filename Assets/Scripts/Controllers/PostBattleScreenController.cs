using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Scripts.Managers.Sound;
using Pepemon.Battle;
using Pepemon.Onboarding;
using Pepemon.Telemetry;
using Cysharp.Threading.Tasks.Triggers;

[RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(Animator))]
public class PostBattleScreenController : MonoBehaviour
{
    #region Enums
    protected enum ScreenState { SHOWN, HIDDEN }
    #endregion

    #region Constants
    protected const string VICTORY_TEXT = "VICTORY!";
    protected const string YOU_WIN_TEXT = "YOU WIN";

    protected const string DEFEAT_TEXT = "DEFEAT";
    protected const string YOU_LOSE_TEXT = "YOU LOSE";

    /// <summary>How long to wait for the wallet before letting the player retry the claim.</summary>
    protected const float ClaimConnectTimeoutSeconds = 45f;
    #endregion

    private float _claimWaitStartedAtUnscaled;

    #region Editor Exposed Data
    [Title("Screen Settings")]
    [SerializeField] bool _startHidden = false;

    [SerializeField] CardPreview _pepemon;
    [SerializeField] TextReveal _victoryDefeat;
    [SerializeField] TextReveal _youWinLose;
    [SerializeField] GameObject _rewardDisplay;
    [SerializeField] GameObject _winDisplay;
    [SerializeField] GameObject _loseDisplay;
    [SerializeField] GameObject _winMsg;
    [SerializeField] GameObject _loseMsg;
    [SerializeField] GameObject _starterPackMsg;
    [SerializeField] GameObject _starterPackMsg2;
    [SerializeField] Button _btnShowMenu;
    [SerializeField] Button _btnPlayAgain;
    [SerializeField] Button _btnClaimGift;

    [Title("Screen Events")]
    private UnityEvent OnShown;
    private UnityEvent OnHidden;

    [Title("Debugging")]
    [Button(Name = "DEBUG Show lose")] private void Debug_Show_Lose() { SetResult(false); Show(); }
    [Button(Name = "DEBUG Show win")] private void Debug_Show_Win() { SetResult(true); Show(); }
    [Button(Name = "DEBUG Hide")] private void Debug_Hide() => Hide();
    #endregion

    #region Object Data
    protected ScreenState _state = ScreenState.SHOWN;

    protected bool _animating = false;
    #endregion

    #region Onboard Components
    protected CanvasGroup _canvasGroup;
    protected Animator _animator;
    #endregion

    #region EndTransitionBackToMenu
    public static bool IsGoingFromBattle = false;
    public static bool IsPlayingAgain = false;
    public static bool IsClaimingGift = false;
    #endregion

    public void SetResult(bool win)
    {
        //_victoryDefeat.SetText(win ? VICTORY_TEXT : DEFEAT_TEXT);
        //_youWinLose.SetText(win ? YOU_WIN_TEXT : YOU_LOSE_TEXT);
        _winDisplay.SetActive(win);
        _loseDisplay.SetActive(!win);

        bool isBotMatch = BattlePrepController.battleData.isBotMatch;
        bool claimedStarterPack = OnboardingState.HasClaimedStarterPack;
        bool starterPackOnOffer = isBotMatch && !claimedStarterPack;

        // The reward line previously read a hardcoded "Gained +10pts". No ranking is written
        // on-chain for bot matches, and the client never reads the real ELO delta for PvP
        // either, so that number was invented. Show only what is actually true: for the first
        // battle the reward is the starter pack, which is now really minted.
        SetRewardText(win && starterPackOnOffer ? "Starter Pack unlocked" : string.Empty);

        var winText = isBotMatch ? TutorialScript.WinHeadline + " " : "You won! ";
        var loseText = isBotMatch ? TutorialScript.RivalName + " took that one. " : "You lost! ";

        if (starterPackOnOffer)
        {
            winText += TutorialScript.WinSubline;
            loseText += TutorialScript.LoseSubline;
        }
        else
        {
            winText += "- Score will be updated in Leaderboards";
            loseText += "- Better luck next time";
        }

        SetLabel(_winMsg, winText);
        SetLabel(_loseMsg, loseText);

        _btnClaimGift.gameObject.SetActive(starterPackOnOffer);
        _btnShowMenu.gameObject.SetActive(true);
        _btnPlayAgain.gameObject.SetActive(!starterPackOnOffer);

        if (ThemePlayer.Instance != null) ThemePlayer.Instance.PlayGameOverSong(win);
    }

    private static void SetLabel(GameObject host, string text)
    {
        if (host == null) return;

        var label = host.GetComponent<TMPro.TMP_Text>();
        if (label != null) label.text = text;
    }

    private void SetRewardText(string text)
    {
        if (_rewardDisplay == null) return;

        var hasText = !string.IsNullOrEmpty(text);

        // Set the text before hiding: GetComponentInChildren skips inactive objects by
        // default, so the previous order wrote to a null reference whenever the display
        // had already been deactivated.
        var reveal = _rewardDisplay.GetComponentInChildren<TextReveal>(true);
        if (reveal != null) reveal.SetText(text);

        _rewardDisplay.SetActive(hasText);
    }

    public void LoadPepemonDisplay(ulong cardId)
    {
        _pepemon.LoadCardData(cardId, false);
    }

    public void OnBtnPlayAgainClick()
    {
        bool isBotMatch = BattlePrepController.battleData.isBotMatch;
        if (isBotMatch)
        {
            ThemePlayer.Instance.SkipEndGameMusic();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            IsPlayingAgain = true;
            OnBtnShowMenuClick();
        }
        
    }

    public void OnBtnShowMenuClick()
    {
        Funnel.Track(Funnel.ReturnedToMenu);
        GoToMenuScene();
    }

    public void OnBtnClaimGiftClick()
    {
        IsGoingFromBattle = true;
        IsClaimingGift = true;

        Funnel.Track(Funnel.ClaimClicked, "from", "post_battle");

        // Guard against a second click while the wallet prompt is open.
        if (_btnClaimGift != null) _btnClaimGift.interactable = false;

        if (Web3Controller.instance != null && !Web3Controller.instance.IsConnected)
        {
            Funnel.Track(Funnel.WalletConnectRequested);
            Web3Controller.instance.ConnectWallet();
        }

        _claimWaitStartedAtUnscaled = Time.unscaledTime;
        InvokeRepeating(nameof(CheckIfWalletConnected), 0.5f, 0.3f);
    }

    /// <summary>
    /// Polls for the wallet connection kicked off by the claim button.
    ///
    /// This used to run forever: it never cancelled itself, so once connected it called
    /// LoadScene every 0.3s until the scene finally unloaded, and if the player dismissed
    /// the wallet prompt it polled for the rest of the session with no way out.
    /// </summary>
    private void CheckIfWalletConnected()
    {
        if (Web3Controller.instance != null && Web3Controller.instance.IsConnected)
        {
            CancelInvoke(nameof(CheckIfWalletConnected));
            Funnel.Track(Funnel.WalletConnectResult, "success", true);
            GoToMenuScene();
            return;
        }

        if (Time.unscaledTime - _claimWaitStartedAtUnscaled < ClaimConnectTimeoutSeconds) return;

        CancelInvoke(nameof(CheckIfWalletConnected));

        // Stay on this screen so the claim is still reachable rather than silently lost.
        IsClaimingGift = false;
        IsGoingFromBattle = false;

        Funnel.Track(Funnel.WalletConnectResult, "success", false, "error", "timeout");
        SetLabel(_winMsg, "Wallet not connected - tap CLAIM to try again.");

        if (_btnClaimGift != null) _btnClaimGift.interactable = true;
    }

    private void GoToMenuScene()
    {
        IsGoingFromBattle = true;

        // Reset the bot battle values so the next START goes to the real game.
        if (Web3Controller.instance != null)
        {
            Web3Controller.instance.StarterDeckID = 0;
            Web3Controller.instance.StarterPepemonID = 0;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex - 1);
    }

    #region INIT
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        _state = _startHidden ? ScreenState.HIDDEN : ScreenState.SHOWN;
        _btnShowMenu.onClick.AddListener(OnBtnShowMenuClick);
        _btnPlayAgain.onClick.AddListener(OnBtnPlayAgainClick);

        _btnClaimGift.onClick.AddListener(OnBtnClaimGiftClick);
    }

    private void OnValidate()
    {
        var cg = GetComponent<CanvasGroup>();
        if (_startHidden)
        {
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        else
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
    #endregion

    #region State Management Logic
    protected async virtual void UpdateState(ScreenState newState)
    {
        if (_animating) return;
        if (newState == _state) return;
        _state = newState;
        _animating = true;

        switch (_state)
        {
            case ScreenState.SHOWN:
                await Show_Task();
                OnShown?.Invoke();
                break;
            case ScreenState.HIDDEN:
                await Hide_Task();
                OnHidden?.Invoke();
                break;
        }

        _animating = false;
    }

    public virtual void Show() => UpdateState(ScreenState.SHOWN);
    public virtual void Hide() => UpdateState(ScreenState.HIDDEN);
    #endregion

    #region Show/Hide Logic
    protected async virtual Task Show_Task()
    {
        _animator.SetTrigger("OnShow");

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Show Complete"))
            await Task.Delay(100);
    }
    protected async virtual Task Hide_Task()
    {
        _animator.SetTrigger("OnHide");

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Hide Complete"))
            await Task.Delay(100);
    }
    #endregion
}
