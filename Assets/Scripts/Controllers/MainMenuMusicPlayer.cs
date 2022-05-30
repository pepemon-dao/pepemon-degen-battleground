using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FadeOut()
    {
        // fade out
        StartCoroutine(AudioFadeOut.FadeOut(audioSource, 1.0f));

        // stop after fade out
        Invoke("SetInactive", 1.0f);
    }

    private void SetInactive()
    {
        gameObject.SetActive(false);
    }
}
