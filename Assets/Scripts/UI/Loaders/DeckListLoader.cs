using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.Web3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeckListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;
    [TitleGroup("Component References"), SerializeField] GameObject _loadingMessage;
    // Whether or not display the pen + transparency fade
    [TitleGroup("Deck display options"), SerializeField] bool _deckEditMode;

    [ReadOnly] public UnityEvent<ulong> onItemSelected;
    private bool loadingInProgress = false;

    /// <summary>
    /// Removes all elements in _deckList and loads all decks using _deckPrefab.
    /// Each deck element invokes onItemSelected when clicked, the deckId is passed as a parameter of this event
    /// </summary>
    public async void ReloadAllDecks()
    {
        // prevent re-reloading things over and over again with old async calls if
        // the user decides to go back and forth very quickly between screens
        if (loadingInProgress) 
            return;

        loadingInProgress = true;

        _loadingMessage.SetActive(true);
        var loadingMessageLabel = _loadingMessage.GetComponent<Text>();
        loadingMessageLabel.text = "Loading decks...";

        // destroy before re-creating
        foreach (var deck in _deckList.GetComponentsInChildren<Button>())
        {
            Destroy(deck.gameObject);
        }

        // should not happen, but if it happens then it won't crash the game
        var account = FindObjectOfType<MainMenuController>().web3.SelectedAccountAddress;
        if (string.IsNullOrEmpty(account))
        {
            loadingMessageLabel.text = "Error: No account selected";
            return;
        }

        // load all decks
        var decks = await PepemonCardDeck.GetPlayerDecks(FindObjectOfType<MainMenuController>().web3.SelectedAccountAddress);

        var loadingTasks = new List<Task>();

        decks.ForEach((deckId) => {
            var deckInstance = Instantiate(_deckPrefab);
            deckInstance.transform.SetParent(_deckList.transform, false);

            // show or hide the pen overlay
            deckInstance.GetComponent<DeckController>().DisplayDeckEditMode = _deckEditMode;
            deckInstance.GetComponent<DeckController>().onEditButtonClicked.AddListener(
                delegate { 
                    onItemSelected?.Invoke(deckId);
                });

            // this should set each deck detail in parallel
            loadingTasks.Add(deckInstance.GetComponent<DeckController>().LoadDeckInfo(deckId));
        });

        await Task.WhenAll(loadingTasks);
        _loadingMessage.SetActive(false);
        loadingInProgress = false;
    }
}
