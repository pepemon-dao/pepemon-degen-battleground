using System;
using Sirenix.OdinInspector;
using Thirdweb;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MintDeckButtonHandler : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;
    [TitleGroup("Component References"), SerializeField] GameObject _loadingMessage;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    async void OnButtonClicked()
    {
        var button = GetComponent<Button>();
        button.interactable = false;
        
        // Show loading message
        if (_loadingMessage != null)
        {
            _loadingMessage.SetActive(true);
            var textComponent = _loadingMessage.GetComponent<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "Minting new deck...";
            }
        }
        
        try
        {
            // Check if wallet is connected before attempting to mint
            if (Web3Controller.instance == null || !Web3Controller.instance.IsConnected)
            {
                Debug.LogError("Wallet is not connected. Cannot mint deck.");
                return;
            }
            
            // Verify we have a valid address
            var address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("No valid wallet address found. Cannot mint deck.");
                return;
            }
            
            Debug.Log("Starting deck creation transaction...");
            await PepemonCardDeck.CreateDeck();
            Debug.Log("Deck creation transaction completed successfully.");
            
            // Wait for blockchain state to propagate before refreshing
            await Cysharp.Threading.Tasks.UniTask.Delay(3000);
            
            // Force reload the deck list
            var deckLoader = _deckList?.GetComponent<DeckListLoader>();
            if (deckLoader != null)
            {
                deckLoader.ReloadAllDecks(true);
            }
            else
            {
                Debug.LogError("DeckListLoader component not found on assigned deck list GameObject.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to mint new deck: " + e.Message);
        }
        finally
        {
            button.interactable = true;
            
            // Hide loading message
            if (_loadingMessage != null)
            {
                _loadingMessage.SetActive(false);
            }
        }
    }
}
