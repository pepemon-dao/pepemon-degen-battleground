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
        Debug.Log("MintDeckButtonHandler: Button clicked, starting mint process...");
        GetComponent<Button>().interactable = false;
        try
        {
            Debug.Log("MintDeckButtonHandler: Calling CreateDeck()...");
            await PepemonCardDeck.CreateDeck();
            Debug.Log("MintDeckButtonHandler: CreateDeck() completed successfully!");
            
            // Invalidate cache and force reload after successful mint
            PepemonCardDeck.InvalidateCache();
            Debug.Log($"MintDeckButtonHandler: Calling ReloadAllDecks(force=true) on {_deckList.name}...");
            _deckList.GetComponent<DeckListLoader>().ReloadAllDecks(true);
            Debug.Log("MintDeckButtonHandler: Forced ReloadAllDecks called!");
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to mint new deck: " + e.Message);
        }

        GetComponent<Button>().interactable = true;
    }
}
