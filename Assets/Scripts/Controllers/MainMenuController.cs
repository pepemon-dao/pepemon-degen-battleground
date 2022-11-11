using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string creditsURL;
    public Web3Controller web3;
    public List<GameObject> menuScreens;

    public GameObject _selectDeckListLoader;

    public int defaultScreenId = 0;
    private int screenNavigationPosition = 0;
    private int[] screenNavigationHistory = new int[10];

    private int selectedLeagueId = 0;
    private int selectedDeckId = 0;

    private void Start()
    {
        ShowScreen(defaultScreenId);
        _selectDeckListLoader.GetComponent<DeckListLoader>().onItemSelected.AddListener(SelectDeck);
    }

    public async void ConnectWallet()
    {
        web3.ConnectWallet();
        await new PepemonFactoryCardCache().PreloadAll();
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
