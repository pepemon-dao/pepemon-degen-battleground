using System.Collections.Generic;
using System.Linq;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonoBehavior for Screen_5_EditDeck
/// </summary>
public class ScreenEditDeck : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckDisplay;
    [TitleGroup("Component References"), SerializeField] GameObject _saveDeckButtonHandler;
    private ulong currentDeckId;
    private ulong battleCard;
    private Dictionary<ulong, int> supportCards;

    public void Start()
    {
        _saveDeckButtonHandler.GetComponent<Button>().onClick.AddListener(HandleSaveButtonClick);
    }

    public async void LoadAllCards(ulong deckId)
    {
        currentDeckId = deckId;
        var account = FindObjectOfType<MainMenuController>().web3.SelectedAccountAddress;

        // This only returns unused cards
        var ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds);

        battleCard = await PepemonCardDeck.GetBattleCard(deckId);
        supportCards = await PepemonCardDeck.GetAllSupportCards(deckId);

        _deckDisplay.GetComponent<DeckDisplay>().ReloadAllBattleCards(ownedCardIds, battleCard);
        _deckDisplay.GetComponent<DeckDisplay>().ReloadAllSupportCards(ownedCardIds, supportCards);
    }

    public async void HandleSaveButtonClick()
    {
        // necessary to avoid "ERC1155#safeTransferFrom: INVALID_OPERATOR"
        await PepemonCardDeck.SetApprovalState(true);

        GetSupportCardsDiff(
            supportCards,
            _deckDisplay.GetComponent<DeckDisplay>().GetSelectedSupportCards(),
            out var supportCardsToBeAdded, 
            out var supportCardsToBeRemoved);

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
            Debug.LogError($"Unable to process transaction: {ex.Message}");
            // TODO: display error toast
        }

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
            Debug.LogError($"Unable to process transaction: {ex.Message}");
            // TODO: display error toast
        }

        var newBattleCard = _deckDisplay.GetComponent<DeckDisplay>().GetSelectedBattleCard();
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
        if (newBattleCard == 0)
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

        await PepemonCardDeck.SetApprovalState(false);
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
