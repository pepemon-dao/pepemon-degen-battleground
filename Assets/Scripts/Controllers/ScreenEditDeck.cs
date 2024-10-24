using System;
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
    private ulong currentDeckId;
    private ulong battleCard;
    private Dictionary<ulong, int> supportCards = new Dictionary<ulong, int>();

    private bool isLoading = false;

    public void Start()
    {
        _saveDeckButton.GetComponent<Button>().onClick.AddListener(HandleSaveButtonClick);
        _mintCardsButton.GetComponent<Button>().onClick.AddListener(HandleMintCardsButtonClick);
    }

    public async void LoadAllCards(ulong deckId, int filter)
    {
        if (isLoading)
        {
            return;
        }
        isLoading = true;

        var deckDisplayComponent = _deckDisplay.GetComponent<DeckDisplay>();

        _textLoading.SetActive(true);

        currentDeckId = deckId;
        string account = "";
        try
        {
            account = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        } catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        supportCards = new Dictionary<ulong, int>();

        bool isStarterDeck = account == "";
        Dictionary<ulong, int> ownedCardIds = new Dictionary<ulong, int>();
        Dictionary<ulong, int> ownedBattleCardIds = new Dictionary<ulong, int>();

        if (isStarterDeck)
        {
            bool firstLoad = false;
            if (DeckDisplay.battleCardId == 0)
            {
                DeckDisplay.battleCardId = 7;
                firstLoad = true;
            }

            if (!firstLoad) 
            {
                // this ensure of the saving while in the process of equip and unequipping
                battleCard = deckDisplayComponent.GetSelectedBattleCard();
                supportCards = deckDisplayComponent.GetSelectedSupportCards();

                foreach (var card in ownedDeck)
                {
                    ulong id = (ulong)card.ID;
                    ownedCardIds[id] = ownedCardIds.ContainsKey(id) ? ownedCardIds[id] + 1 : 1;
                }

                foreach (var card in ownedDeck)
                {
                    ulong id = (ulong)card.ID;
                    if (supportCards.ContainsKey(id))
                    {
                        ownedCardIds[id] -= supportCards[id];

                        if (ownedCardIds[id] <= 0)
                        {
                            ownedCardIds.Remove(id);
                        }
                    }
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
            else
            {
                battleCard = DeckDisplay.battleCardId;

                foreach (var card in ownedDeck)
                {
                    ulong id = (ulong)card.ID;
                    ownedCardIds[id] = ownedCardIds.ContainsKey(id) ? ownedCardIds[id] + 1 : 1;
                }


                foreach (var card in ownedBattleDeck)
                {
                    int cardId = int.Parse(card.ID);
                    ulong id = (ulong)cardId;
                    ownedBattleCardIds[id] = ownedBattleCardIds.ContainsKey(id) ? ownedBattleCardIds[id] + 1 : 1;
                }

                foreach (var card in starterDeck)
                {
                    ulong id = (ulong)card.ID;
                    supportCards[id] = supportCards.ContainsKey(id) ? supportCards[id] + 1 : 1;
                }
            }
        }
        else
        {
            // This only returns unused cards
            ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds.ToList());

            battleCard = await PepemonCardDeck.GetBattleCard(deckId);
            supportCards = new Dictionary<ulong, int>(await PepemonCardDeck.GetAllSupportCards(deckId));
        }


        if (DeckDisplay.battleCardId == 0)
        {
            DeckDisplay.battleCardId = battleCard;
        }

        deckDisplayComponent.ClearMyCardsList();
        deckDisplayComponent.LoadSelectedCards(battleCard, supportCards, filter);
        deckDisplayComponent.LoadAllSupportCards(ownedCardIds, supportCards, filter);
        deckDisplayComponent.LoadAllBattleCards(ownedBattleCardIds, battleCard, filter);
        _textLoading.SetActive(false);

        isLoading = false;
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

    public void GetSupportCardsDiff(Dictionary<ulong, int> oldSupportCards, 
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
