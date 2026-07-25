using Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private SFXPlayer sfxPlayer;
    [SerializeField] private AudioClip btnSFX;
    [SerializeField] private AudioClip dealSFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip slideSFX;

    [Header("Battle emphasis")]
    // Optional. Each falls back to an existing clip when unassigned, so the hooks are usable
    // before new audio exists rather than silently doing nothing.
    [SerializeField] private AudioClip bigHitSFX;
    [SerializeField] private AudioClip lowHealthSFX;
    [SerializeField] private AudioClip roundStartSFX;

    public static SFXManager Instance;

    // Awake, not Start: CardController and GameController both call SFXManager.Instance from
    // their own Start/first frame, which previously raced this assignment.
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BtnSFX() => PlaySFX(btnSFX, 1f);

    public void DealSFX() => PlaySFX(dealSFX, 1f);

    public void HitSFX() => PlaySFX(hitSFX, 1f);

    public void SlideSFX() => PlaySFX(slideSFX, 0.05f);

    /// <summary>Heavier hit. Falls back to the normal hit clip, louder.</summary>
    public void BigHitSFX() => PlaySFX(bigHitSFX != null ? bigHitSFX : hitSFX, 1f);

    /// <summary>Played once when a combatant drops into the danger zone.</summary>
    public void LowHealthSFX() => PlaySFX(lowHealthSFX != null ? lowHealthSFX : hitSFX, 0.6f);

    public void RoundStartSFX() => PlaySFX(roundStartSFX != null ? roundStartSFX : dealSFX, 0.7f);

    private void PlaySFX(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || sfxPlayer == null) return;

        Instantiate(sfxPlayer.gameObject, transform.position, Quaternion.identity)
            .GetComponent<SFXPlayer>()
            .PlaySFXWithVolume(clip, volume);
    }
}
