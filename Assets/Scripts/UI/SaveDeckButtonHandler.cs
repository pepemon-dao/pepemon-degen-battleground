using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Org.BouncyCastle.Utilities.Collections;

public class SaveDeckButtonHandler : MonoBehaviour
{
    [ReadOnly] private HashSet<ulong> newSupportCards = new HashSet<ulong>();
    [ReadOnly] private HashSet<ulong> oldSupportCards = new HashSet<ulong>();
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
    public void PrepareForTransaction(ulong deckId, ulong previousBattleCard, List<ulong> previousSupportCards)
    {
        currentDeckId = deckId;
        // initially everything is equal, and as the player selects/deselects cards it differs
        newBattleCard = oldBattleCard = previousBattleCard;
        oldSupportCards = new HashSet<ulong>(previousSupportCards);
        newSupportCards = new HashSet<ulong>(previousSupportCards);
    }

    /// <summary>
    /// Sends the transaction to set the deck cards
    /// </summary>
    async void SetDeckCards()
    {
#if UNITY_EDITOR
        await PepemonCardDeck.SetApprovalState(true);
#endif
        // if a new card is not in the old list, add
        var supportCardsToBeAdded = newSupportCards.Where(cardId => !oldSupportCards.Contains(cardId));
        if (supportCardsToBeAdded.Count() > 0)
        {
            Debug.Log($"Adding {supportCardsToBeAdded.Count()} support cards to deck {currentDeckId}");
            await PepemonCardDeck.AddSupportCards(
                currentDeckId, 
                supportCardsToBeAdded.Select(cardId => new SupportCardRequest { SupportCardId = cardId, Amount = 1 }).ToArray());
        }

        // if an old card is not in the new list, remove
        var supportCardsToBeRemoved = oldSupportCards.Where(i => !newSupportCards.Contains(i));
        if (supportCardsToBeRemoved.Count() > 0)
        {
            Debug.Log($"Removing {supportCardsToBeRemoved.Count()} support cards to deck {currentDeckId}");
            await PepemonCardDeck.RemoveSupportCards(
                currentDeckId, 
                supportCardsToBeRemoved.Select(cardId => new SupportCardRequest { SupportCardId = cardId, Amount = 1 }).ToArray());
        }

        // Assuming the transactions worked. This should make oldSupportCards and newSupportCards equal, preventing
        // another transaction if the player presses the button twice
        // creating a new one here is necessary to prevent a "Object changed during iteration" error
        new List<ulong>(supportCardsToBeRemoved).ForEach(i => oldSupportCards.Remove(i));
        new List<ulong>(supportCardsToBeAdded).ForEach(i => oldSupportCards.Add(i));

        if (newBattleCard != oldBattleCard 
            && newBattleCard != 0) // 0 is an invalid card
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
        if (!newSupportCards.Add(cardId))
        {
            Debug.LogWarning($"SupportCard {cardId} already selected");
        }
    }

    public void removeSupportCard(ulong cardId)
    {
        if (!newSupportCards.Remove(cardId))
        {
            Debug.LogWarning($"Unable to deselect SupportCard {cardId}. Already deselected");
        }
    }
}
