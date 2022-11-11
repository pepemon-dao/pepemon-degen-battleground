using System.Collections;
using System.Collections.Generic;
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
        _editDeckListLoader.GetComponent<DeckListLoader>().onItemSelected.AddListener(SelectEditDeck);
    }

    public void SelectEditDeck(int deckId)
    {
        Debug.Log("Editing deck: " + deckId);
        _screenEditDeck.GetComponent<ScreenEditDeck>().LoadAllCards();
        FindObjectOfType<MainMenuController>().ShowScreen(5);
    }

    public void ReloadAllDecks()
    {
        _editDeckListLoader.GetComponent<DeckListLoader>().ReloadAllDecks();
    }
}
