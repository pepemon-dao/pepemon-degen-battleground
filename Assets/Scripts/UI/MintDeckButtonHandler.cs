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

    /// <summary>
    /// Falls back to the deck list's own loading label when no message object is wired.
    ///
    /// The live Mint New Deck button has no _loadingMessage assigned - the only wired instance
    /// sits on an inactive legacy screen - so every progress and failure string this handler
    /// produced went nowhere. Minting a deck showed no feedback at all while the wallet was
    /// waiting on a signature.
    /// </summary>
    private GameObject ResolveLoadingMessage()
    {
        if (_loadingMessage != null) return _loadingMessage;
        if (_deckList == null) return null;

        var loader = _deckList.GetComponent<DeckListLoader>();
        return loader != null ? loader.LoadingMessage : null;
    }

    async void OnButtonClicked()
    {
        var _resolvedLoadingMessage = ResolveLoadingMessage();
        var button = GetComponent<Button>();
        button.interactable = false;
        
        // Show loading message
        if (_resolvedLoadingMessage != null)
        {
            _resolvedLoadingMessage.SetActive(true);
            var textComponent = _resolvedLoadingMessage.GetComponent<TMPro.TMP_Text>();
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
                Debug.Log("Wallet not connected. Attempting to connect...");
                
                // Update loading message to show connection attempt
                var textComponent = _resolvedLoadingMessage?.GetComponent<TMPro.TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = "Connecting wallet...";
                }
                
                // Try to connect wallet first
                await Web3Controller.instance.ConnectWallet();
                
                // Verify connection was successful
                if (!Web3Controller.instance.IsConnected)
                {
                    Debug.LogError("Failed to connect wallet. Cannot mint deck.");
                    if (textComponent != null)
                    {
                        textComponent.text = "Wallet connection failed";
                    }
                    await Cysharp.Threading.Tasks.UniTask.Delay(2000); // Show error for 2 seconds
                    return;
                }
                
                // Update loading message back to minting
                if (textComponent != null)
                {
                    textComponent.text = "Minting new deck...";
                }
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

            // Surface it. The finally below hides the label immediately, so without holding it
            // here a rejected or reverted mint looked exactly like nothing happening.
            var errorText = _resolvedLoadingMessage?.GetComponent<TMPro.TMP_Text>();
            if (errorText != null)
            {
                errorText.text = "Could not mint deck. Please try again.";
                await Cysharp.Threading.Tasks.UniTask.Delay(2500);
            }
        }
        finally
        {
            button.interactable = true;
            
            // Hide loading message
            if (_resolvedLoadingMessage != null)
            {
                _resolvedLoadingMessage.SetActive(false);
            }
        }
    }
}
