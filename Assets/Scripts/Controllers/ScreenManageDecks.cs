using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// MonoBehaviour for Screen_4_ManageDecks
/// </summary>
public class ScreenManageDecks : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] public GameObject _editDeckListLoader;
    [TitleGroup("Component References"), SerializeField] public GameObject _screenEditDeck;

    private void Start()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().onEditDeck.AddListener(SelectEditDeck);
    }

    public void SelectEditDeck(ulong deckId)
    {
        FindObjectOfType<MainMenuController>().ShowScreen(MainSceneScreensEnum.EditDeck);
        // forceRefresh: opening the editor must always re-read ownership from chain. Without
        // it, re-entering the same deck reused the cached owned-card list, so cards minted
        // since the last visit never appeared.
        _screenEditDeck.GetComponent<ScreenEditDeck>()
            .LoadAllCards(deckId, FilterController.Instance.currentFilter, forceRefresh: true);
    }

    public void ReloadAllDecks()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().ReloadAllDecks();
    }
}
