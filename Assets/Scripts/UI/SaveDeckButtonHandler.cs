using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class SaveDeckButtonHandler : MonoBehaviour
{
    [ReadOnly] private Dictionary<ulong, int> newSupportCards = new Dictionary<ulong, int>();
    [ReadOnly] private Dictionary<ulong, int> oldSupportCards = new Dictionary<ulong, int>();
    [ReadOnly] private ulong currentDeckId;
    [ReadOnly] private ulong newBattleCard;
    [ReadOnly] public ulong oldBattleCard { get; private set; }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetDeckCards);
    }

    /// <summary>
    /// Sets the initial state used to add or remove cards on a deck
    /// </summary>
    /// <param name="deckId">deck being edited</param>
    /// <param name="previousBattleCard">original battlecard of the deck</param>
    /// <param name="previousSupportCards">original support cards of the deck</param>
    public void PrepareForTransaction(ulong deckId, ulong previousBattleCard, Dictionary<ulong, int> previousSupportCards)
    {
        currentDeckId = deckId;
        // initially everything is equal, and as the player selects/deselects cards it differs
        newBattleCard = oldBattleCard = previousBattleCard;
        oldSupportCards = new Dictionary<ulong, int>(previousSupportCards);
        newSupportCards = new Dictionary<ulong, int>(previousSupportCards);
    }

    /// <summary>
    /// Sends the transaction to set the deck cards
    /// </summary>
    async void SetDeckCards()
    {
#if UNITY_EDITOR
        await PepemonCardDeck.SetApprovalState(true);
#endif
        Dictionary<ulong, int> supportCardsToBeRemoved = new Dictionary<ulong, int>();
        Dictionary<ulong, int> supportCardsToBeAdded = new Dictionary<ulong, int>();

        foreach(ulong k in oldSupportCards.Keys.Concat(newSupportCards.Keys))
        {
            newSupportCards.TryGetValue(k, out var newCard);
            oldSupportCards.TryGetValue(k, out var oldCard);
            var cardCountDelta = newCard - oldCard;
            if (cardCountDelta > 0)
            {
                supportCardsToBeAdded[k] = cardCountDelta;
            }
            else if (cardCountDelta < 0)
            {
                // invert sign because RemoveSupportCards only accepts positive numbers
                supportCardsToBeRemoved[k] = cardCountDelta * -1;
            }
        }

        Debug.Log($"Adding {supportCardsToBeAdded.Values.Sum()} support cards to deck {currentDeckId}");
        await PepemonCardDeck.AddSupportCards(
            currentDeckId,
            supportCardsToBeAdded.Select(
                keyPair => new SupportCardRequest { SupportCardId = keyPair.Key, Amount = keyPair.Value }).ToArray());

        Debug.Log($"Removing {supportCardsToBeRemoved.Values.Sum()} support cards to deck {currentDeckId}");
        await PepemonCardDeck.RemoveSupportCards(
            currentDeckId,
            supportCardsToBeRemoved.Select(
                keyPair => new SupportCardRequest { SupportCardId = keyPair.Key, Amount = keyPair.Value }).ToArray());

        // TODO: make oldSupportCards contain the same values as newSupportCards
        //new List<ulong>(supportCardsToBeRemoved).ForEach(i => oldSupportCards.Remove(i));
        //new List<ulong>(supportCardsToBeAdded).ForEach(i => oldSupportCards.Add(i));

        if (newBattleCard != oldBattleCard && newBattleCard != 0) // 0 is an invalid card
        {
            Debug.Log($"Setting battlecard {newBattleCard} on deck {currentDeckId}");
            await PepemonCardDeck.SetBattleCard(currentDeckId, newBattleCard);
        }
#if UNITY_EDITOR
        await PepemonCardDeck.SetApprovalState(false);
#endif
    }

    public void setBattleCard(ulong cardId)
    {
        newBattleCard = Math.Max(cardId, 0);
    }

    public void addSupportCard(ulong cardId)
    {
        newSupportCards[cardId] = newSupportCards.ContainsKey(cardId) ? newSupportCards[cardId] + 1 : 1;
    }

    public void removeSupportCard(ulong cardId)
    {
        newSupportCards[cardId] = newSupportCards.ContainsKey(cardId) ? newSupportCards[cardId] - 1 : -1;
    }
}
