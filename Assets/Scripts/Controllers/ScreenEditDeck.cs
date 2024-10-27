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
    [TitleGroup("Helpers"), SerializeField] List<Card> ownedDeck; 
    [TitleGroup("Helpers"), SerializeField] List<BattleCard> ownedBattleDeck; 
    [TitleGroup("Helpers"), SerializeField] List<Card> starterDeck; 
    [TitleGroup("Helpers"), SerializeField] FilterController filterController; 
    private ulong currentDeckId;
    private ulong battleCard;
    private IDictionary<ulong, int> supportCards = new Dictionary<ulong, int>();

    private bool isLoading = false;

    public void Start()
    {
        _saveDeckButton.GetComponent<Button>().onClick.AddListener(HandleSaveButtonClick);
        _mintCardsButton.GetComponent<Button>().onClick.AddListener(HandleMintCardsButtonClick);
    }

    public void LoadAllCards(ulong deckId, int filter)
    {
        if (isLoading)
        {
            return;
        }
        StartCoroutine(LoadAllCardsCoroutine(deckId, filter));
    }

    private IEnumerator LoadAllCardsCoroutine(ulong deckId, int filter)
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
        string account = "";

        // Getting the address from the wallet
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

        supportCards = new OrderedDictionary<ulong, int>();
        bool isStarterDeck = deckId == 1234;
        Dictionary<ulong, int> ownedCardIds = new Dictionary<ulong, int>();
        Dictionary<ulong, int> ownedBattleCardIds = new Dictionary<ulong, int>();

        // Handle starter deck case
        if (isStarterDeck)
        {
            SetupStarterDeck(deckDisplayComponent, ownedCardIds, ownedBattleCardIds, loadingNewDeck);
        }
        else
        {
            /*
            // This only returns unused cards
            ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds.ToList());

            battleCard = await PepemonCardDeck.GetBattleCard(deckId);
            supportCards = new Dictionary<ulong, int>(await PepemonCardDeck.GetAllSupportCards(deckId));

            */

            // Fetch owned cards
            yield return StartCoroutine(PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds.ToList(), result => ownedCardIds = result));

            if (loadingNewDeck)
            {
                DeckDisplay.battleCardId = 0; //safe guards
                battleCard = 0; //safe guards

                // Fetch battle card
                yield return StartCoroutine(PepemonCardDeck.GetBattleCard(deckId, result => battleCard = result));

                // Fetch all support cards
                yield return StartCoroutine(PepemonCardDeck.GetAllSupportCards(deckId, result => supportCards = result));
            }
            else
            {
                battleCard = deckDisplayComponent.GetSelectedBattleCard();
                supportCards = deckDisplayComponent.GetSelectedSupportCards();
            }

            if (battleCard == 0)
            {
                if (DeckDisplay.battleCardId == 0)
                {
                    DeckDisplay.battleCardId = 7;
                }
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
            if (filterController == null && FilterController.Instance != null)
            {
                FilterController.Instance.SetFilter(0);
            }
        }

        // Load deck data into the display
        deckDisplayComponent.ClearMyCardsList();
        deckDisplayComponent.LoadSelectedCards(battleCard, supportCards, filter);
        deckDisplayComponent.LoadAllSupportCards(ownedCardIds, supportCards, filter);
        deckDisplayComponent.LoadAllBattleCards(ownedBattleCardIds, battleCard, filter);

        _textLoading.SetActive(false);
        isLoading = false;
    }

    // Helper method for setting up the starter deck
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
            DeductSelectedCardsFromOwned(supportCards, ownedCardIds);
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
    }

    // Helper method for deducting selected cards from owned cards
    private void DeductSelectedCardsFromOwned(IDictionary<ulong, int> selectedCards, Dictionary<ulong, int> ownedCardIds)
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
    }

    private void setButtonsInteractibleState(bool interactible)
    {
        _saveDeckButton.GetComponent<Button>().interactable = interactible;
        _previousScreenButton.GetComponent<Button>().interactable = interactible;
        _mintCardsButton.GetComponent<Button>().interactable = interactible;
    }

    public async void HandleMintCardsButtonClick()
    {
        setButtonsInteractibleState(false);
        try
        {
            await PepemonCardDeck.MintCards();
            LoadAllCards(currentDeckId, FilterController.Instance.currentFilter);
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
            GetSupportCardsDiff(
                supportCards,
                _deckDisplay.GetComponent<DeckDisplay>().GetSelectedSupportCards(),
                out var supportCardsToBeAdded,
                out var supportCardsToBeRemoved);

            // TODO: wait for transaction receipt
            await UpdateSupportCards(supportCardsToBeAdded, supportCardsToBeRemoved);
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
