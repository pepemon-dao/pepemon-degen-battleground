using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pepemon.Battle;
using Sirenix.OdinInspector;
using Thirdweb;
using UnityEngine;
using UnityEngine.UI;
using static PepemonCardDeck;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// MonoBehavior for Screen_5_EditDeck
/// </summary>
public class ScreenEditDeck : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckDisplay;
    [TitleGroup("Component References"), SerializeField] GameObject _saveDeckButton;
    [TitleGroup("Component References"), SerializeField] GameObject _mintCardsButton;
    [TitleGroup("Component References"), SerializeField] GameObject _previousScreenButton;
    [TitleGroup("Component References"), SerializeField] GameObject _textLoading;
    //[TitleGroup("Helpers"), SerializeField] List<Card> ownedDeck; 
    //[TitleGroup("Helpers"), SerializeField] List<BattleCard> ownedBattleDeck; 
    //[TitleGroup("Helpers"), SerializeField] List<Card> starterDeck; 

    private Dictionary<ulong, CardMetadata?> metadataLookup = new Dictionary<ulong, CardMetadata?>();

    private ulong currentDeckId;
    private ulong battleCard;
    private IDictionary<ulong, int> supportCards = new Dictionary<ulong, int>();
    private IDictionary<ulong, int> starterSupportCards = new Dictionary<ulong, int>();

    private Dictionary<ulong, int> ownedCardIds = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> ownedBattleCardIds = new Dictionary<ulong, int>();

    private bool isLoading = false;

    private bool shouldUpdateTheStarterSupportCardsAfterSave = false;

    private string account = "";

    public void Start()
    {
        _saveDeckButton.GetComponent<Button>().onClick.AddListener(HandleSaveButtonClick);
        _mintCardsButton.GetComponent<Button>().onClick.AddListener(HandleMintCardsButtonClick);
    }

    public void LoadAllCards(ulong deckId, int filter, bool forceFullRefresh = false)
    {
        if (isLoading)
        {
            return;
        }
        StartCoroutine(LoadAllCardsCoroutine(deckId, filter, forceFullRefresh));
    }

    private IEnumerator LoadAllCardsCoroutine(ulong deckId, int filter, bool forceFullRefresh = false)
    {
        isLoading = true;
        _textLoading.SetActive(true);

        var deckDisplayComponent = _deckDisplay.GetComponent<DeckDisplay>();

        bool loadingNewDeck = false;

        if (currentDeckId != deckId)
        {
            currentDeckId = deckId;
            loadingNewDeck = true;
        }
        
        // Force full refresh when explicitly requested (e.g., after minting cards)
        if (forceFullRefresh)
        {
            loadingNewDeck = true;
            // Clear metadata cache to ensure fresh metadata lookup
            metadataLookup.Clear();
            Debug.Log("Force refresh: Cleared metadata cache");
        }

        account = "";

        // Getting the address from the wallet
        
        if (account == "")
        {
            var getAddressRequest = ThirdwebManager.Instance.SDK.Wallet.GetAddress();

            while (!getAddressRequest.IsCompleted)
            {
                yield return null;
            }
            if (getAddressRequest.IsFaulted)
            {
                Debug.LogError(getAddressRequest.Exception);
            }
            else
            {
                account = getAddressRequest.Result;
            }
        }

        supportCards = new OrderedDictionary<ulong, int>();
        starterSupportCards = new OrderedDictionary<ulong, int>();
        bool isStarterDeck = false;//deckId == 1234;

        // Handle starter deck case
        if (isStarterDeck)
        {
            // we are not using this system currently and would be on chain - TODO: remove all the connected code to this
            //SetupStarterDeck(deckDisplayComponent, ownedCardIds, ownedBattleCardIds, loadingNewDeck);
        }
        else
        {
            /*
            // This only returns unused cards
            ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds.ToList());

            battleCard = await PepemonCardDeck.GetBattleCard(deckId);
            supportCards = new Dictionary<ulong, int>(await PepemonCardDeck.GetAllSupportCards(deckId));

            */
            

            
            if (loadingNewDeck)
            {
                DeckDisplay.battleCardId = 0; //safe guards
                battleCard = 0; //safe guards

                ownedCardIds = new();
                ownedBattleCardIds = new();

                // Fetch battle card
                yield return StartCoroutine(PepemonCardDeck.GetBattleCard(deckId, result => battleCard = result));

                // Fetch all support cards
                yield return StartCoroutine(PepemonCardDeck.GetAllSupportCards(deckId, result => supportCards = result));

                // Fetch owned cards
                yield return StartCoroutine(PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds.ToList(), result => ownedCardIds = result));

                starterSupportCards = supportCards;

                var keysToRemove = new List<ulong>();

                foreach (var entry in ownedCardIds)
                {
                    if (!metadataLookup.TryGetValue(entry.Key, out var metadata))
                    {
                        // Metadata not found in lookup, retrieve it
                        Debug.Log($"Fetching metadata for card ID: {entry.Key}");
                        metadata = PepemonFactoryCardCache.GetMetadata(entry.Key);
                        if (metadata != null)
                        {
                            metadataLookup[entry.Key] = metadata;
                            Debug.Log($"Successfully cached metadata for card ID: {entry.Key}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to fetch metadata for card ID: {entry.Key}. Skipping card.");
                            continue; // Skip this card if metadata is null
                        }
                    }

                    // Additional null check before accessing metadata
                    if (metadata.HasValue && !string.IsNullOrEmpty(metadata.Value.description))
                    {
                        bool isBattleCard = metadata.Value.description.Contains("Battle ver");
                        if (isBattleCard)
                        {
                            // Add to ownedBattleCardIds
                            ownedBattleCardIds.Add(entry.Key, entry.Value);

                            // Mark the key for removal
                            keysToRemove.Add(entry.Key);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Metadata is null or empty for card ID: {entry.Key}. Treating as support card.");
                        // If metadata is missing, assume it's a support card and leave in ownedCardIds
                    }
                }

                // Remove the marked keys from ownedCardIds
                foreach (var key in keysToRemove)
                {
                    ownedCardIds.Remove(key);
                }

            }
            else
            {
                battleCard = deckDisplayComponent.GetSelectedBattleCard();
                supportCards = deckDisplayComponent.GetSelectedSupportCards();
            }

            if (!loadingNewDeck)
            {
                if (shouldUpdateTheStarterSupportCardsAfterSave)
                {
                    starterSupportCards = supportCards;
                }
            }

            shouldUpdateTheStarterSupportCardsAfterSave = false;

            
            if (battleCard == 0)
            {
                battleCard = DeckDisplay.battleCardId;
            }
        }

        // Set battle card if not set
        if (DeckDisplay.battleCardId == 0)
        {
            DeckDisplay.battleCardId = battleCard;
        }

        if (loadingNewDeck)
        {
            if (FilterController.Instance != null)
            {
                FilterController.Instance.ResetFilters();
            }
        }

        if (loadingNewDeck)
        {
            DeckDisplay.Instance.UnloadPreviousDeck();
        }

        // Load deck data into the display
        deckDisplayComponent.ClearMyCardsList();
        deckDisplayComponent.LoadSelectedCards(battleCard, supportCards);
        // Load battle cards (Pepemon) first so they appear at the top
        deckDisplayComponent.LoadAllBattleCards(ownedBattleCardIds, battleCard);
        deckDisplayComponent.LoadAllSupportCards(ownedCardIds, supportCards);

        DeckDisplay.Instance.UpdateCardInDeckDisplay(loadingNewDeck);

        _textLoading.SetActive(false);
        isLoading = false;
    }

    // Helper method for setting up the starter deck
    /*
    private void SetupStarterDeck(DeckDisplay deckDisplayComponent, Dictionary<ulong, int> ownedCardIds, Dictionary<ulong, int> ownedBattleCardIds, bool IsLoadingNewDeck)
    {
        if (IsLoadingNewDeck)
        {
            DeckDisplay.battleCardId = 7;
        }
        else
        {
            battleCard = deckDisplayComponent.GetSelectedBattleCard();
            supportCards = deckDisplayComponent.GetSelectedSupportCards();
        }

        foreach (var card in ownedDeck)
        {
            ulong id = (ulong)card.ID;
            ownedCardIds[id] = ownedCardIds.ContainsKey(id) ? ownedCardIds[id] + 1 : 1;
        }

        if (!IsLoadingNewDeck)
        {
            ownedCardIds = DeductSelectedCardsFromOwned(supportCards, ownedCardIds);
        }

        foreach (var card in ownedBattleDeck)
        {
            int cardId = int.Parse(card.ID);
            ulong id = (ulong)cardId;
            if (id != battleCard)
            {
                ownedBattleCardIds[id] = ownedBattleCardIds.ContainsKey(id) ? ownedBattleCardIds[id] + 1 : 1;
            }
        }
    }*/

    // Helper method for deducting selected cards from owned cards
    private Dictionary<ulong, int> DeductSelectedCardsFromOwned(IDictionary<ulong, int> selectedCards, Dictionary<ulong, int> ownedCardIds)
    {
        foreach (var card in selectedCards)
        {
            if (ownedCardIds.ContainsKey(card.Key))
            {
                ownedCardIds[card.Key] -= card.Value;
                if (ownedCardIds[card.Key] <= 0)
                {
                    ownedCardIds.Remove(card.Key);
                }
            }
        }

        return ownedCardIds;
    }

    private void setButtonsInteractibleState(bool interactible)
    {
        _saveDeckButton.GetComponent<Button>().interactable = interactible;
        //_previousScreenButton.GetComponent<Button>().interactable = interactible;
        _mintCardsButton.GetComponent<Button>().interactable = interactible;
    }

    public async void HandleMintCardsButtonClick()
    {
        setButtonsInteractibleState(false);
        
        // Show minting feedback
        _textLoading.SetActive(true);
        var loadingText = _textLoading.GetComponent<TMPro.TMP_Text>();
        if (loadingText != null)
        {
            loadingText.text = "Minting cards...";
        }
        
        try
        {
            // Fire mint transaction
            await PepemonCardDeck.MintCards();
            Debug.Log("Mint transaction completed, waiting for blockchain propagation...");
            
            // Update loading text
            if (loadingText != null)
            {
                loadingText.text = "Waiting for cards to appear...";
            }

            // Minimal propagation wait (simplified approach)
            await System.Threading.Tasks.Task.Delay(3000);

            // Force full refresh to fetch newly minted cards from wallet
            var filter = FilterController.Instance != null ? FilterController.Instance.currentFilter : 0;
            LoadAllCards(currentDeckId, filter, forceFullRefresh: true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during mint process: {ex.Message}");
            _textLoading.SetActive(false);
        }
        finally
        {
            setButtonsInteractibleState(true);
        }
    }
    
    public void FilterCards(int filter)
    {
        setButtonsInteractibleState(false);
        try
        {
            LoadAllCards(currentDeckId, filter);
        } 
        finally
        {
            setButtonsInteractibleState(true);
        }
    }

   public async void HandleSaveButtonClick()
    {
        setButtonsInteractibleState(false);

        var pepemonCardDeckAddress = Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

        // necessary to avoid "ERC1155#safeTransferFrom: INVALID_OPERATOR"
        // TODO: place this in an "approve" button
        var approvalOk = await PepemonFactory.GetApprovalState(pepemonCardDeckAddress);
        if (!approvalOk)
        {
            try
            {
                await PepemonFactory.SetApprovalState(true, pepemonCardDeckAddress);
                approvalOk = await PepemonFactory.GetApprovalState(pepemonCardDeckAddress);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("SetApprovedForAll failed: " + ex.Message);
            }
        }

        if (approvalOk)
        {
            shouldUpdateTheStarterSupportCardsAfterSave = true;

            GetSupportCardsDiff(
                starterSupportCards,
                _deckDisplay.GetComponent<DeckDisplay>().GetSelectedSupportCards(),
                out var supportCardsToBeAdded,
                out var supportCardsToBeRemoved);

            // TODO: wait for transaction receipt
            if (supportCardsToBeAdded.Length > 0 ||supportCardsToBeRemoved.Length > 0)
            {
                await UpdateSupportCards(supportCardsToBeAdded, supportCardsToBeRemoved);
            }
            
            await UpdateBattlecard(_deckDisplay.GetComponent<DeckDisplay>().GetSelectedBattleCard());
        }

        setButtonsInteractibleState(true);
    }

    private async Task UpdateBattlecard(ulong newBattleCard)
    {
        if (newBattleCard != battleCard && newBattleCard != 0) // 0 is an invalid card
        {
            try
            {
                Debug.Log($"Setting battlecard {newBattleCard} on deck {currentDeckId}");
                await PepemonCardDeck.SetBattleCard(currentDeckId, newBattleCard);

                // update currently selected battlecard in case of success
                battleCard = newBattleCard;
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                Debug.LogError($"Unable to process transaction: {ex.Message}");
                // TODO: display error toast
            }
        }
        else if (newBattleCard != battleCard && newBattleCard == 0)
        {
            try
            {
                Debug.Log($"Removing battlecard {newBattleCard} on deck {currentDeckId}");
                await PepemonCardDeck.RemoveBattleCard(currentDeckId);

                // update currently selected battlecard in case of success
                battleCard = newBattleCard;
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                Debug.LogError($"Unable to process transaction: {ex.Message}");
                // TODO: display error toast
            }
        }
    }

    private async Task UpdateSupportCards(SupportCardRequest[] supportCardsToBeAdded, 
                                         SupportCardRequest[] supportCardsToBeRemoved)
    {
        if (supportCardsToBeAdded.Count() > 0)
        {
            try
            {
                Debug.Log($"Adding {supportCardsToBeAdded.Count()} types of support cards to deck {currentDeckId}");
                await PepemonCardDeck.AddSupportCards(currentDeckId, supportCardsToBeAdded);

                // update supportCards with added cards in case of success
                foreach (var card in supportCardsToBeAdded)
                {
                    if (!supportCards.TryAdd((ulong)card.SupportCardId, (int)card.Amount))
                    {
                        supportCards[(ulong)card.SupportCardId] += (int)card.Amount;
                    }
                }
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                Debug.LogError($"Unable to process transaction AddSupportCards: {ex.Message}");
                // TODO: display error toast
            }
        }
        if (supportCardsToBeRemoved.Count() > 0)
        {
            try
            {
                Debug.Log($"Removing {supportCardsToBeRemoved.Count()} types of  support cards from deck {currentDeckId}");
                await PepemonCardDeck.RemoveSupportCards(currentDeckId, supportCardsToBeRemoved);
                // update supportCards with removed cards in case of success
                foreach (var card in supportCardsToBeRemoved)
                {
                    if (supportCards.ContainsKey((ulong)card.SupportCardId))
                    {
                        if (supportCards[(ulong)card.SupportCardId] - card.Amount == 0)
                        {
                            supportCards.Remove((ulong)card.SupportCardId);
                        }
                        else
                        {
                            supportCards[(ulong)card.SupportCardId] -= (int)card.Amount;
                        }
                    }
                }
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
            {
                Debug.LogError($"Unable to process transaction RemoveSupportCards: {ex.Message}");
                // TODO: display error toast
            }
        }
    }

    public void GetSupportCardsDiff(IDictionary<ulong, int> oldSupportCards, 
                                    Dictionary<ulong, int> newSupportCards,
                                    out SupportCardRequest[] toBeAddedSupportCardRequest, 
                                    out SupportCardRequest[] toBeRemovedSupportCardRequest)
    {
        var cardsToBeAdded = new Dictionary<ulong, int>();
        var cardsToBeRemoved = new Dictionary<ulong, int>();

        foreach (ulong k in oldSupportCards.Keys.Concat(newSupportCards.Keys))
        {
            newSupportCards.TryGetValue(k, out var newCard);
            oldSupportCards.TryGetValue(k, out var oldCard);
            var cardCountDelta = newCard - oldCard;
            if (cardCountDelta > 0)
            {
                cardsToBeAdded[k] = cardCountDelta;
            }
            else if (cardCountDelta < 0)
            {
                // invert sign because RemoveSupportCards only accepts positive numbers
                cardsToBeRemoved[k] = cardCountDelta * -1;
            }
        }

        toBeAddedSupportCardRequest = cardsToBeAdded.Select(
                keyPair => new SupportCardRequest { SupportCardId = keyPair.Key, Amount = keyPair.Value })
                .ToArray();

        toBeRemovedSupportCardRequest = cardsToBeRemoved.Select(
                keyPair => new SupportCardRequest { SupportCardId = keyPair.Key, Amount = keyPair.Value })
                .ToArray();
    }
}
