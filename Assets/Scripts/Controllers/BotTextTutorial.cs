using System.Collections.Generic;
using Pepemon.Battle;
using Pepemon.Onboarding;
using Pepemon.Telemetry;
using TMPro;
using UnityEngine;

/// <summary>
/// Drives the first-battle tutorial beats.
///
/// Two behaviours changed here that matter:
///
/// 1. Advancing used to require the spacebar. On touch there is no spacebar and the panel
///    froze the game, so mobile players were permanently stuck; the same happened on desktop
///    WebGL whenever the canvas lost keyboard focus. Any of key / mouse / touch now advances.
///
/// 2. Short beats are toasts that never freeze the battle. Only the intro and victory beats
///    are modal. Previously every beat froze the game and all of them fired inside round 1.
///
/// The copy itself lives in <see cref="TutorialScript"/> rather than in scene YAML.
/// </summary>
public class BotTextTutorial : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private TMP_Text tutorialText;

    /// <summary>Optional "tap to continue" hint. Hidden for toasts, which dismiss themselves.</summary>
    [SerializeField] private GameObject continuePrompt;

    /// <summary>Ignore input for a moment after a beat appears, so the click that dismissed
    /// the previous beat cannot fall through and dismiss this one too.</summary>
    private const float InputLockSeconds = 0.35f;

    private bool isBeatVisible;
    private bool isModal;
    private int currentBeatId;

    /// <summary>Beats shown this session, so a beat cannot repeat before it is persisted.</summary>
    private readonly HashSet<int> shownThisSession = new HashSet<int>();

    private float inputLockedUntilUnscaled;
    private float toastHideAtUnscaled;
    private float beatShownAtUnscaled;

    public static BotTextTutorial Instance;

    /// <summary>
    /// True only while a modal beat is blocking the battle.
    ///
    /// Deliberately excludes toasts: callers use this to suppress gameplay VFX and input,
    /// and a toast is ordinary gameplay.
    /// </summary>
    public bool IsInTutorial => isBeatVisible && isModal;

    /// <summary>True while any beat - modal or toast - is on screen.</summary>
    public bool IsBeatVisible => isBeatVisible;

    /// <summary>True if the player saw any tutorial beat during this session.</summary>
    public bool wasInTutorial { get; private set; }

    private void Awake()
    {
        // Assigned in Awake, not Start: GameController and PostBattleScreenController both
        // dereference Instance and their Start order is not guaranteed.
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        // Leaving the scene mid-beat must not leave the next scene frozen.
        TimeControl.Unfreeze(TimeControl.HolderTutorial);
    }

    private void Update()
    {
        if (!isBeatVisible) return;

        if (isModal)
        {
            if (TryReadAdvanceInput(out var method)) Dismiss(method);
            return;
        }

        // Toasts are dismissible early but expire on their own.
        if (TryReadAdvanceInput(out var toastMethod))
        {
            Dismiss(toastMethod);
            return;
        }

        if (Time.unscaledTime >= toastHideAtUnscaled) Dismiss("auto");
    }

    /// <summary>
    /// Any of keyboard, mouse or touch advances a beat.
    /// Uses unscaled time throughout - Time.time does not advance while a modal beat holds
    /// the game at timeScale 0.
    /// </summary>
    private bool TryReadAdvanceInput(out string method)
    {
        method = null;

        if (Time.unscaledTime < inputLockedUntilUnscaled) return false;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            method = "key";
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                method = "touch";
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            method = "mouse";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Hides any visible beat immediately and releases the freeze.
    ///
    /// Called before the post-battle screen appears: that screen animates itself in and polls
    /// its Animator, which cannot advance while a modal beat holds timeScale at 0.
    /// </summary>
    public void ForceDismiss()
    {
        if (!isBeatVisible) return;
        Dismiss("forced");
    }

    /// <summary>Hook for a UI Button, so the prompt works even if raw input is swallowed.</summary>
    public void Advance()
    {
        if (!isBeatVisible) return;
        if (Time.unscaledTime < inputLockedUntilUnscaled) return;
        Dismiss("button");
    }

    /// <summary>
    /// Shows beat <paramref name="beatId"/> if the player has not already seen it.
    /// Ids are defined in <see cref="TutorialScript"/>.
    /// </summary>
    public void TriggerTutorialEvent(int beatId)
    {
        if (shownThisSession.Contains(beatId)) return;
        if (OnboardingState.HasSeenBeat(beatId)) return;
        if (!TutorialScript.TryGetBeat(beatId, out var beat) || beat == null) return;

        // A newer beat supersedes whatever is on screen rather than being dropped.
        if (isBeatVisible) Dismiss("superseded");

        shownThisSession.Add(beatId);
        currentBeatId = beatId;
        Show(beat);
    }

    private void Show(TutorialBeat beat)
    {
        isBeatVisible = true;
        isModal = beat.Mode == TutorialBeatMode.Modal;
        wasInTutorial = true;

        beatShownAtUnscaled = Time.unscaledTime;
        inputLockedUntilUnscaled = Time.unscaledTime + InputLockSeconds;
        toastHideAtUnscaled = isModal
            ? float.MaxValue
            : Time.unscaledTime + Mathf.Max(1f, beat.ToastSeconds);

        if (tutorialText != null) tutorialText.text = beat.Text;
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (continuePrompt != null) continuePrompt.SetActive(isModal);

        // The quit button stays available at all times - the player must never be trapped.
        if (quitButton != null) quitButton.SetActive(true);

        if (isModal) TimeControl.Freeze(TimeControl.HolderTutorial);

        Funnel.Track(
            Funnel.TutorialBeatShown,
            "beat", beat.Id,
            "mode", isModal ? "modal" : "toast");
    }

    private void Dismiss(string inputMethod)
    {
        if (!isBeatVisible) return;

        var msShown = Mathf.RoundToInt((Time.unscaledTime - beatShownAtUnscaled) * 1000f);

        isBeatVisible = false;
        isModal = false;
        toastHideAtUnscaled = float.MaxValue;

        TimeControl.Unfreeze(TimeControl.HolderTutorial);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (quitButton != null) quitButton.SetActive(true);

        // Persist only once the beat has actually been consumed, so an interrupted beat
        // is shown again next time.
        OnboardingState.MarkBeatSeen(currentBeatId);

        Funnel.Track(
            Funnel.TutorialBeatDismissed,
            new Dictionary<string, object>
            {
                ["beat"] = currentBeatId,
                ["ms_shown"] = msShown,
                ["input_method"] = inputMethod
            });
    }
}
