using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// MonoBehaviour for Screen_4_ManageDecks
/// </summary>
public class ScreenManageDecks : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] public GameObject _editDeckListLoader;
    [TitleGroup("Component References"), SerializeField] public GameObject _screenEditDeck;
    [TitleGroup("Component References"), SerializeField] public GameObject _refreshButton;

    private void Start()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().onEditDeck.AddListener(SelectEditDeck);
        
        // Note: The refresh button should have a RefreshDeckListButton component attached in the Inspector,
        // with the loader field assigned to reference _editDeckListLoader's DeckListLoader component.
        // This is the recommended approach for the UI team.
    }

    public void SelectEditDeck(ulong deckId)
    {
        FindObjectOfType<MainMenuController>().ShowScreen(MainSceneScreensEnum.EditDeck);
        _screenEditDeck.GetComponent<ScreenEditDeck>().LoadAllCards(deckId, FilterController.Instance.currentFilter);
    }

    public void ReloadAllDecks()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().ReloadAllDecks();
    }
    
    /// <summary>
    /// Public method to handle refresh button clicks.
    /// This provides a clean interface for the UI team to hook the refresh button.
    /// </summary>
    public void OnRefreshBtn()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().ReloadAllDecks(true);
    }
}
