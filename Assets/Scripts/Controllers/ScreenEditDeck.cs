using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// MonoBehavior for Screen_5_EditDeck
/// </summary>
public class ScreenEditDeck : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardsListLoader;
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardsListLoader;
    [TitleGroup("Component References"), SerializeField] GameObject _saveDeckButtonHandler;

    public async void LoadAllCards(ulong deckId)
    {
        var account = FindObjectOfType<MainMenuController>().web3.SelectedAccountAddress;

        // This only returns unused cards
        var ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds);

        // Associates cards with decks. A single deck has one battlecard and a list of support cards
        ConcurrentDictionary<ulong, DeckCards> deckCards = await GetDecksCards(account);

        // These cards will appear grayed out
        var unavailableCardIds = GetUsedCards(deckCards); 

        // This must set before reloading cards
        _saveDeckButtonHandler.GetComponent<SaveDeckButtonHandler>().PrepareForTransaction(
            deckId, 
            deckCards[deckId].selectedBattlecard,
            deckCards[deckId].selectedSupportCards);

        _battleCardsListLoader.GetComponent<BattleCardListLoader>().ReloadAllBattleCards(
            ownedCardIds,
            unavailableCardIds,
            deckCards[deckId].selectedBattlecard);

        _supportCardsListLoader.GetComponent<SupportCardListLoader>().ReloadAllSupportCards(
            ownedCardIds, 
            unavailableCardIds,
            deckCards[deckId].selectedSupportCards);
    }

    private List<ulong> GetUsedCards(ConcurrentDictionary<ulong, DeckCards> deckCards)
    {
        var usedCards = new List<ulong>();
        foreach (var deckId in deckCards.Keys)
        {
            if (deckCards[deckId].selectedSupportCards.Count > 0)
            {
                usedCards.AddRange(deckCards[deckId].selectedSupportCards);
            }

            if (deckCards[deckId].selectedBattlecard > 0)
            {
                usedCards.Add(deckCards[deckId].selectedBattlecard);
            }
        }
        return usedCards;
    }

    private async Task<ConcurrentDictionary<ulong, DeckCards>> GetDecksCards(string account)
    {
        var deckCards = new ConcurrentDictionary<ulong, DeckCards>();
        var decks = await PepemonCardDeck.GetPlayerDecks(account);
        List<Task> loadingTasks = new List<Task>();
        foreach (var deckId in decks)
        {
            loadingTasks.Add(SetDeckCardsAsync(deckId, deckCards));
        }
        await Task.WhenAll(loadingTasks);
        return deckCards;
    }

    private async Task SetDeckCardsAsync(ulong deckId, ConcurrentDictionary<ulong, DeckCards> deckCards)
    {
        deckCards[deckId] = new DeckCards
        {
            selectedBattlecard = await PepemonCardDeck.GetBattleCard(deckId),
            selectedSupportCards = await PepemonCardDeck.GetAllSupportCards(deckId)
        };
    }

    [Serializable]
    public struct DeckCards
    {
        public ulong selectedBattlecard;
        public List<ulong> selectedSupportCards;
    }
}
