using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string creditsURL;
    public Web3Controller web3;
    public List<GameObject> menuScreens;

    public int defaultScreenId = 0;
    private int screenNavigationPosition = 0;
    private int[] screenNavigationHistory = new int[10];

    private int selectedLeagueId = 0;
    private int selectedDeckId = 0;
    private int selectedEditDeckId = 0;

    private void Start()
    {
        ShowScreen(defaultScreenId);
    }

    public void ConnectWallet()
    {
        web3.ConnectWallet();
    }

    public void ShowScreen(int screenId)
    {
        if (screenId < 0)
        {
            int nextPosition = screenNavigationPosition + screenId;
            screenId = screenNavigationHistory[nextPosition % screenNavigationHistory.Length];
            screenNavigationPosition = (nextPosition - 1) % screenNavigationHistory.Length;
        }

        for(int i = 0; i < menuScreens.Count; i++)
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

    public void SelectDeck(int deckId)
    {
        selectedDeckId = deckId;
    }

    public void SelectEditDeck(int deckId)
    {
        selectedEditDeckId = deckId;
    }

    public void MintNewDeck()
    {
        
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

    }
}
