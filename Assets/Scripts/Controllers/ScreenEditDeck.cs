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

    /// <summary>
    /// Loads a deck into the editor.
    /// </summary>
    /// <param name="forceRefresh">
    /// Re-read ownership from chain even when the deck id has not changed. Required after
    /// minting: the owned-card fetch is otherwise gated on the deck id changing, so minting
    /// and reloading the deck you are already viewing showed stale data and the new cards
    /// never appeared.
    /// </param>
    public void LoadAllCards(ulong deckId, int filter, bool forceRefresh = false)
    {
        if (isLoading)
        {
            return;
        }
        StartCoroutine(LoadAllCardsCoroutine(deckId, filter, forceRefresh));
    }

    private IEnumerator LoadAllCardsCoroutine(ulong deckId, int filter, bool forceRefresh)
    {
        isLoading = true;
        _textLoading.SetActive(true);

        // Everything below runs inside try/finally so a throw can never strand isLoading at
        // true - which previously left the screen showing "Loading" forever, unrecoverable
        // without a scene reload.
        try
        {

        var deckDisplayComponent = _deckDisplay.GetComponent<DeckDisplay>();

        bool loadingNewDeck = forceRefresh;

        if (currentDeckId != deckId)
        {
            currentDeckId = deckId;
            loadingNewDeck = true;
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

                // Snapshot, not an alias. These were the same object, so every mutation of
                // supportCards after a successful save also rewrote the baseline the next
                // diff is computed against - producing deltas that ask the contract to
                // transfer cards the wallet no longer holds.
                starterSupportCards = new Dictionary<ulong, int>(supportCards);

                var keysToRemove = new List<ulong>();

                foreach (var entry in ownedCardIds)
                {
                    if (!metadataLookup.TryGetValue(entry.Key, out var metadata))
                    {
                        // Metadata not found in lookup, retrieve it
                        metadata = PepemonFactoryCardCache.GetMetadata(entry.Key);
                        if (metadata != null)
                        {
                            metadataLookup[entry.Key] = metadata;
                        }
                    }

                    // Metadata can legitimately be missing - a freshly minted id may not be in
                    // the cache yet. Dereferencing the nullable here used to throw, kill this
                    // coroutine, and leave the screen stuck on "Loading" permanently.
                    if (metadata == null)
                    {
                        Debug.LogWarning($"[deck] No metadata for card {entry.Key}; treating it as a support card.");
                        continue;
                    }

                    var description = metadata.Value.description ?? string.Empty;
                    bool isBattleCard = description.Contains("Battle ver");
                    if (isBattleCard)
                    {
                        // Add to ownedBattleCardIds
                        ownedBattleCardIds.Add(entry.Key, entry.Value);

                        // Mark the key for removal
                        keysToRemove.Add(entry.Key);
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
                    starterSupportCards = new Dictionary<ulong, int>(supportCards);
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
        deckDisplayComponent.LoadAllSupportCards(ownedCardIds, supportCards);
        deckDisplayComponent.LoadAllBattleCards(ownedBattleCardIds, battleCard);

        DeckDisplay.Instance.UpdateCardInDeckDisplay(loadingNewDeck);

        }
        finally
        {
            _textLoading.SetActive(false);
            isLoading = false;
        }
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
        try
        {
            SetStatus("Minting cards...");
            await PepemonCardDeck.MintCards();

            // forceRefresh: the deck id has not changed, so without this the owned-card fetch
            // is skipped and the freshly minted cards never show up.
            SetStatus("Loading your new cards...");
            LoadAllCards(currentDeckId, FilterController.Instance.currentFilter, forceRefresh: true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unable to mint cards: {ex.Message}");
            SetStatus("Minting failed. Please try again.", autoHide: true);
        }
        finally
        {
            setButtonsInteractibleState(true);
        }
    }

    /// <summary>
    /// Shows a transient message using the existing loading label.
    ///
    /// Every failure path here previously did nothing but Debug.LogError, so a rejected or
    /// reverted transaction looked exactly like success.
    /// </summary>
    private Coroutine _statusHideRoutine;

    /// <param name="autoHide">
    /// True for terminal messages ("Deck saved.", failures). Those have no follow-up step to
    /// clear them, so without this they sit over the deck forever - the label is centred on
    /// screen, so it covers the Pepemon card and reads as the screen being stuck.
    /// </param>
    private void SetStatus(string message, bool autoHide = false)
    {
        Debug.Log($"[deck] {message}");

        if (_textLoading == null) return;

        if (_statusHideRoutine != null)
        {
            StopCoroutine(_statusHideRoutine);
            _statusHideRoutine = null;
        }

        _textLoading.SetActive(!string.IsNullOrEmpty(message));

        var label = _textLoading.GetComponent<TMPro.TMP_Text>();
        if (label == null) label = _textLoading.GetComponentInChildren<TMPro.TMP_Text>();
        if (label != null) label.text = message;

        if (autoHide && isActiveAndEnabled && !string.IsNullOrEmpty(message))
        {
            _statusHideRoutine = StartCoroutine(HideStatusAfter(2.5f));
        }
    }

    private IEnumerator HideStatusAfter(float seconds)
    {
        // Realtime: this must clear even if something else has frozen the game.
        yield return new WaitForSecondsRealtime(seconds);

        if (_textLoading != null) _textLoading.SetActive(false);
        _statusHideRoutine = null;
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

        // try/finally: the re-enable used to sit after the awaits with no protection, so any
        // exception that was not an RpcResponseException - a wallet rejection, for instance -
        // left the Save button disabled for good.
        try
        {
            var pepemonCardDeckAddress = Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

            // necessary to avoid "ERC1155#safeTransferFrom: INVALID_OPERATOR"
            var approvalOk = await PepemonFactory.GetApprovalState(pepemonCardDeckAddress);
            if (!approvalOk)
            {
                try
                {
                    SetStatus("Approving card transfers...");
                    await PepemonFactory.SetApprovalState(true, pepemonCardDeckAddress);
                    approvalOk = await PepemonFactory.GetApprovalState(pepemonCardDeckAddress);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("SetApprovedForAll failed: " + ex.Message);
                }
            }

            if (!approvalOk)
            {
                SetStatus("Approval declined - deck not saved.", autoHide: true);
                return;
            }

            shouldUpdateTheStarterSupportCardsAfterSave = true;

            GetSupportCardsDiff(
                starterSupportCards,
                _deckDisplay.GetComponent<DeckDisplay>().GetSelectedSupportCards(),
                out var supportCardsToBeAdded,
                out var supportCardsToBeRemoved);

            var failures = 0;

            if (supportCardsToBeAdded.Length > 0 || supportCardsToBeRemoved.Length > 0)
            {
                SetStatus("Saving your cards...");
                failures += await UpdateSupportCards(supportCardsToBeAdded, supportCardsToBeRemoved);
            }

            SetStatus("Saving your Pepemon...");
            failures += await UpdateBattlecard(_deckDisplay.GetComponent<DeckDisplay>().GetSelectedBattleCard());

            SetStatus(failures == 0
                ? "Deck saved."
                : "Deck partly saved - some changes failed. Check your deck and retry.",
                autoHide: true);

            // Re-read ownership from chain. Saving moves cards out of the wallet and into the
            // deck, but ownedCardIds was only refetched when the deck id changed - so after a
            // save the editor still offered cards the wallet no longer held. Adding one of
            // those produced "want 2, own 0" and a failed save, which is what made every
            // second edit fail.
            LoadAllCards(currentDeckId, FilterController.Instance.currentFilter, forceRefresh: true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unable to save deck: {ex.Message}");
            SetStatus("Save failed. Please try again.", autoHide: true);
        }
        finally
        {
            setButtonsInteractibleState(true);
        }
    }

    /// <summary>Returns the number of transactions that failed, so the caller can tell the player.</summary>
    private async Task<int> UpdateBattlecard(ulong newBattleCard)
    {
        if (newBattleCard == battleCard) return 0;

        // Catch Exception rather than only RpcResponseException: a wallet rejection and
        // thirdweb's WebGL error wrapping are not RpcResponseExceptions, and used to escape
        // all the way out of the async void click handler.
        if (newBattleCard != 0) // 0 is an invalid card
        {
            try
            {
                Debug.Log($"Setting battlecard {newBattleCard} on deck {currentDeckId}");
                await PepemonCardDeck.SetBattleCard(currentDeckId, newBattleCard);

                // update currently selected battlecard in case of success
                battleCard = newBattleCard;
                return 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to set battle card: {ex.Message}");
                return 1;
            }
        }

        try
        {
            Debug.Log($"Removing battlecard on deck {currentDeckId}");
            await PepemonCardDeck.RemoveBattleCard(currentDeckId);

            // update currently selected battlecard in case of success
            battleCard = newBattleCard;
            return 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unable to remove battle card: {ex.Message}");
            return 1;
        }
    }

    /// <summary>Returns the number of transactions that failed, so the caller can tell the player.</summary>
    private async Task<int> UpdateSupportCards(SupportCardRequest[] supportCardsToBeAdded,
                                         SupportCardRequest[] supportCardsToBeRemoved)
    {
        var failures = 0;

        // Pre-flight against actual wallet balances. addSupportCardsToDeck transfers the
        // cards out of the wallet, so requesting more copies than are held makes ERC1155
        // revert with no reason string - which is unattributable in the UI and looks like
        // "saving is broken". Cards already inside the deck are no longer in the wallet.
        var unaffordable = supportCardsToBeAdded
            .Where(r => !ownedCardIds.TryGetValue((ulong)r.SupportCardId, out var held)
                        || held < (int)r.Amount)
            .ToArray();

        if (unaffordable.Length > 0)
        {
            var detail = string.Join(", ", unaffordable.Select(r =>
            {
                ownedCardIds.TryGetValue((ulong)r.SupportCardId, out var held);
                return $"card {r.SupportCardId} (want {r.Amount}, own {held})";
            }));

            Debug.LogError($"[deck] Refusing addSupportCards - not enough copies owned: {detail}");
            SetStatus("You don't own enough copies of some cards. Reload the deck and try again.", autoHide: true);
            return supportCardsToBeAdded.Length;
        }

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
            catch (Exception ex)
            {
                Debug.LogError($"Unable to process transaction AddSupportCards: {ex.Message}");
                failures++;
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
            catch (Exception ex)
            {
                Debug.LogError($"Unable to process transaction RemoveSupportCards: {ex.Message}");
                failures++;
            }
        }

        return failures;
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
