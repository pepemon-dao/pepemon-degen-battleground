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

        _deckDisplay.GetComponent<DeckDisplay>().ReloadAllBattleCards(ownedCardIds.Keys.ToList(), battleCard);
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

        Debug.Log($"Adding {supportCardsToBeAdded.Count()} support cards to deck {currentDeckId}");
        await PepemonCardDeck.AddSupportCards(currentDeckId, supportCardsToBeAdded);

        Debug.Log($"Removing {supportCardsToBeRemoved.Count()} support cards from deck {currentDeckId}");
        await PepemonCardDeck.RemoveSupportCards(currentDeckId, supportCardsToBeRemoved);

        // TODO: make oldSupportCards contain the same values as newSupportCards
        //new List<ulong>(supportCardsToBeRemoved).ForEach(i => oldSupportCards.Remove(i));
        //new List<ulong>(supportCardsToBeAdded).ForEach(i => oldSupportCards.Add(i));

        var newBattleCard = _deckDisplay.GetComponent<DeckDisplay>().GetSelectedBattleCard();

        if (newBattleCard != battleCard && newBattleCard != 0) // 0 is an invalid card
        {
            Debug.Log($"Setting battlecard {newBattleCard} on deck {currentDeckId}");
            await PepemonCardDeck.SetBattleCard(currentDeckId, newBattleCard);
        }
        if (newBattleCard == 0)
        {
            await PepemonCardDeck.RemoveBattleCard(currentDeckId);
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
