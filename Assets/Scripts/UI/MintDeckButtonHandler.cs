using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MintDeckButtonHandler : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    async void OnButtonClicked()
    {
        GetComponent<Button>().interactable = false;
        var tx = await PepemonCardDeck.CreateDeck();
        // TODO: show error label on failure
        if (!string.IsNullOrEmpty(tx))
        {
            // TODO: enum screens
            FindObjectOfType<MainMenuController>().ShowScreen(5);
        }
        GetComponent<Button>().interactable = true;

        _deckList.GetComponent<DeckListLoader>().ReloadAllDecks();
    }
}
