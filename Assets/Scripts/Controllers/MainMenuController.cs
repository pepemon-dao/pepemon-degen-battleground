using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string creditsURL;

    public void ConnectWallet()
    {

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

    }
}
