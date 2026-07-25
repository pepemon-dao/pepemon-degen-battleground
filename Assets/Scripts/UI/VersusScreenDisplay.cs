using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;
using UnityEngine.UI;

public class VersusScreenDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image imgRed;
    [SerializeField] private Image bgRed;
    [SerializeField] private Image imgBlue;
    [SerializeField] private Image bgBlue;
    [SerializeField] private TMPro.TMP_Text nameRed;
    [SerializeField] private TMPro.TMP_Text nameBlue;

    /// <summary>Optional. Gives the opponent a voice instead of 3 seconds of dead air.</summary>
    [SerializeField] private TMPro.TMP_Text tauntLabel;

    [SerializeField] private float holdSeconds = 2.5f;
    [SerializeField] private float fadeSeconds = 0.6f;

    /// <summary>Ignore input briefly so a carried-over tap cannot skip the screen instantly.</summary>
    private const float SkipLockSeconds = 0.4f;

    private bool isShowing;
    private float skipUnlockedAtUnscaled;

    public void SetVersusScreen(Sprite red, Sprite blue, Sprite bgB, Sprite bgR, string blueN, string redN)
    {
        SetVersusScreen(red, blue, bgB, bgR, blueN, redN, null);
    }

    public void SetVersusScreen(Sprite red, Sprite blue, Sprite bgB, Sprite bgR, string blueN, string redN, string taunt)
    {
        imgRed.sprite = red;
        imgBlue.sprite = blue;
        if (bgR != null)
            bgRed.sprite = bgR;
        if (bgB != null)
            bgBlue.sprite = bgB;
        nameRed.text = redN;
        nameBlue.text = blueN;

        if (tauntLabel != null)
        {
            tauntLabel.text = taunt ?? string.Empty;
            tauntLabel.gameObject.SetActive(!string.IsNullOrEmpty(taunt));
        }

        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        isShowing = true;
        skipUnlockedAtUnscaled = Time.unscaledTime + SkipLockSeconds;

        var hideAt = Time.unscaledTime + Mathf.Max(0.5f, holdSeconds);

        // Skippable. This was a fixed 3.25s wait with no way past it, which is a long time to
        // stare at a static screen on a repeat run.
        while (Time.unscaledTime < hideAt && !WasSkipRequested())
        {
            yield return null;
        }

        isShowing = false;
        canvasGroup.DOFade(0f, fadeSeconds);
    }

    private bool WasSkipRequested()
    {
        if (!isShowing) return false;
        if (Time.unscaledTime < skipUnlockedAtUnscaled) return false;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
        }

        return false;
    }
}
