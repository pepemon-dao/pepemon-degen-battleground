using System;
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
        try
        {
            await PepemonCardDeck.CreateDeck();
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to mint new deck: " + e.Message);
        }

        GetComponent<Button>().interactable = true;
        _deckList.GetComponent<DeckListLoader>().ReloadAllDecks();
    }
}
