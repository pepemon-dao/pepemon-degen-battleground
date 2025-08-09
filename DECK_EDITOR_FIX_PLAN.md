# Deck Editor Fix Plan: Card Selection and Blockchain Interaction Issues

## Problem Analysis

### Root Cause
The deck editor's card selection system doesn't properly distinguish between:
1. Cards already in the deck (should not be sent to blockchain again)
2. Newly added cards (should be sent to blockchain)
3. Cards removed from the deck (should be removed from blockchain)

### Current Flow Issues
1. `DeckDisplay.GetSelectedSupportCards()` returns ALL cards in the deck area
2. `ScreenEditDeck.GetSupportCardsDiff()` compares this against the original deck state
3. When cards are already in deck, the diff calculation fails because it treats all selected cards as "new"
4. This causes blockchain transactions to attempt re-adding existing cards, leading to failures

## Comprehensive Solution

### Phase 1: Fix State Management (Critical Priority)

#### 1.1 Enhanced DeckDisplay State Tracking
- **File**: `Assets/Scripts/UI/DeckDisplay.cs`
- **Changes**: Add proper state tracking for cards

```csharp
public class DeckDisplay : MonoBehaviour
{
    // New state tracking
    private Dictionary<ulong, int> originalSupportCards = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> currentSupportCards = new Dictionary<ulong, int>();
    
    // Method to set the original state when loading a deck
    public void SetOriginalSupportCards(IDictionary<ulong, int> cards)
    {
        originalSupportCards.Clear();
        foreach(var kvp in cards)
        {
            originalSupportCards[kvp.Key] = kvp.Value;
        }
        currentSupportCards.Clear();
        foreach(var kvp in cards)
        {
            currentSupportCards[kvp.Key] = kvp.Value;
        }
    }
    
    // Modified method to return only the current state
    public Dictionary<ulong, int> GetCurrentSupportCards()
    {
        return new Dictionary<ulong, int>(currentSupportCards);
    }
    
    // New method to get the difference for blockchain operations
    public void GetSupportCardChanges(out Dictionary<ulong, int> toAdd, out Dictionary<ulong, int> toRemove)
    {
        toAdd = new Dictionary<ulong, int>();
        toRemove = new Dictionary<ulong, int>();
        
        // Check all cards in current state
        foreach(var kvp in currentSupportCards)
        {
            ulong cardId = kvp.Key;
            int currentCount = kvp.Value;
            int originalCount = originalSupportCards.GetValueOrDefault(cardId, 0);
            
            if (currentCount > originalCount)
            {
                toAdd[cardId] = currentCount - originalCount;
            }
            else if (currentCount < originalCount)
            {
                toRemove[cardId] = originalCount - currentCount;
            }
        }
        
        // Check for completely removed cards
        foreach(var kvp in originalSupportCards)
        {
            ulong cardId = kvp.Key;
            if (!currentSupportCards.ContainsKey(cardId))
            {
                toRemove[cardId] = kvp.Value;
            }
        }
    }
}
```

#### 1.2 Update ScreenEditDeck Integration
- **File**: `Assets/Scripts/Controllers/ScreenEditDeck.cs`
- **Changes**: Modify to use new DeckDisplay methods

```csharp
public async void HandleSaveButtonClick()
{
    setButtonsInteractibleState(false);

    var pepemonCardDeckAddress = Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

    // Approval logic (unchanged)
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

        // NEW: Use the enhanced diff calculation
        var deckDisplay = _deckDisplay.GetComponent<DeckDisplay>();
        
        deckDisplay.GetSupportCardChanges(out var cardsToAdd, out var cardsToRemove);
        
        // Convert to SupportCardRequest arrays
        var supportCardsToBeAdded = cardsToAdd.Select(kvp => 
            new SupportCardRequest { SupportCardId = kvp.Key, Amount = kvp.Value }).ToArray();
        var supportCardsToBeRemoved = cardsToRemove.Select(kvp => 
            new SupportCardRequest { SupportCardId = kvp.Key, Amount = kvp.Value }).ToArray();

        // Process changes
        if (supportCardsToBeAdded.Length > 0 || supportCardsToBeRemoved.Length > 0)
        {
            await UpdateSupportCards(supportCardsToBeAdded, supportCardsToBeRemoved);
        }
        
        await UpdateBattlecard(deckDisplay.GetSelectedBattleCard());
    }

    setButtonsInteractibleState(true);
}
```

### Phase 2: Enhanced UI Feedback and Error Handling

#### 2.1 Improved Logging and Debugging
- **File**: `Assets/Scripts/Contracts/PepemonCardDeck.cs`
- **Enhancement**: Add more detailed logging

```csharp
public static async Task AddSupportCards(ulong deckId, params SupportCardRequest[] requests)
{
    // Enhanced logging
    Debug.Log($"[BLOCKCHAIN] AddSupportCards: Deck {deckId}, adding {requests.Length} card types:");
    
    int totalCards = 0;
    foreach (var req in requests)
    {
        totalCards += (int)req.Amount;
        Debug.Log($"  - Card ID {req.SupportCardId}: {req.Amount} copies");
    }
    Debug.Log($"[BLOCKCHAIN] Total cards being added: {totalCards}");
    
    try
    {
        if (Utils.IsWebGLBuild())
        {
            await contract.Write("addSupportCardsToDeck", deckId, requests.Select((a) => new List<object>()
            {
                a.SupportCardId,
                a.Amount
            }));
        }
        else
        {
            await contract.Write("addSupportCardsToDeck", deckId, requests);
        }
        Debug.Log($"[BLOCKCHAIN] AddSupportCards transaction successful for deck {deckId}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"[BLOCKCHAIN] AddSupportCards failed for deck {deckId}: {ex.Message}");
        Debug.LogError($"[BLOCKCHAIN] Exception details: {ex}");
        throw;
    }
}
```

#### 2.2 User Feedback System
- **File**: `Assets/Scripts/Controllers/ScreenEditDeck.cs`
- **Enhancement**: Add user-friendly error messages

```csharp
private async Task UpdateSupportCards(SupportCardRequest[] supportCardsToBeAdded, 
                                     SupportCardRequest[] supportCardsToBeRemoved)
{
    try
    {
        if (supportCardsToBeRemoved.Count() > 0)
        {
            Debug.Log($"[DECK SAVE] Removing {supportCardsToBeRemoved.Count()} types of support cards from deck {currentDeckId}");
            await PepemonCardDeck.RemoveSupportCards(currentDeckId, supportCardsToBeRemoved);
            
            // Update local state
            foreach (var card in supportCardsToBeRemoved)
            {
                if (supportCards.ContainsKey((ulong)card.SupportCardId))
                {
                    if (supportCards[(ulong)card.SupportCardId] - card.Amount <= 0)
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

        if (supportCardsToBeAdded.Count() > 0)
        {
            // Validate cards before adding
            var validCardsToAdd = ValidateSupportCards(supportCardsToBeAdded);
            if (validCardsToAdd.Length == 0)
            {
                Debug.LogWarning("[DECK SAVE] No valid support cards to add after validation.");
                ShowUserMessage("No valid cards to add to deck.", MessageType.Warning);
                return;
            }
            
            if (validCardsToAdd.Length < supportCardsToBeAdded.Length)
            {
                int filteredCount = supportCardsToBeAdded.Length - validCardsToAdd.Length;
                Debug.LogWarning($"[DECK SAVE] Filtered out {filteredCount} invalid card(s) from deck.");
                ShowUserMessage($"Some cards couldn't be added (invalid or insufficient quantity).", MessageType.Warning);
            }
            
            Debug.Log($"[DECK SAVE] Adding {validCardsToAdd.Length} types of support cards to deck {currentDeckId}");
            await PepemonCardDeck.AddSupportCards(currentDeckId, validCardsToAdd);
            
            // Update local state
            foreach (var card in validCardsToAdd)
            {
                if (!supportCards.TryAdd((ulong)card.SupportCardId, (int)card.Amount))
                {
                    supportCards[(ulong)card.SupportCardId] += (int)card.Amount;
                }
            }
            
            ShowUserMessage("Cards successfully added to deck!", MessageType.Success);
        }
    }
    catch (Nethereum.JsonRpc.Client.RpcResponseException ex)
    {
        Debug.LogError($"[DECK SAVE] RPC Error updating support cards: {ex.Message}");
        ShowUserMessage("Failed to save deck. Please try again.", MessageType.Error);
    }
    catch (Exception ex)
    {
        Debug.LogError($"[DECK SAVE] General error updating support cards: {ex.Message}");
        ShowUserMessage("An unexpected error occurred. Please try again.", MessageType.Error);
    }
}

private void ShowUserMessage(string message, MessageType type)
{
    // TODO: Implement user-friendly toast/popup system
    Debug.Log($"[USER MESSAGE] {type}: {message}");
}

private enum MessageType
{
    Success,
    Warning,
    Error
}
```

### Phase 3: Comprehensive Testing Strategy

#### 3.1 Test Cases to Implement

1. **Basic Deck Operations**:
   - Load empty deck, add cards, save
   - Load existing deck, add more cards, save
   - Load existing deck, remove cards, save
   - Load existing deck, replace some cards, save

2. **Edge Cases**:
   - Add same card type multiple times
   - Remove all cards of a type
   - Exceed deck limits
   - Invalid card IDs
   - Network failures during save

3. **State Consistency**:
   - UI state matches blockchain state after save
   - Card counts are accurate
   - No duplicate transactions

#### 3.2 Debug Utilities

```csharp
// Add to ScreenEditDeck.cs
#if UNITY_EDITOR
[Header("Debug Tools")]
[SerializeField] private bool enableDebugMode = false;

private void LogDeckState(string operation)
{
    if (!enableDebugMode) return;
    
    Debug.Log($"[DECK DEBUG] {operation}:");
    Debug.Log($"  Current Deck ID: {currentDeckId}");
    Debug.Log($"  Battle Card: {battleCard}");
    Debug.Log($"  Support Cards in Memory: {supportCards.Count} types");
    
    foreach(var kvp in supportCards)
    {
        Debug.Log($"    - Card {kvp.Key}: {kvp.Value} copies");
    }
    
    var deckDisplay = _deckDisplay.GetComponent<DeckDisplay>();
    var currentSelection = deckDisplay.GetCurrentSupportCards();
    Debug.Log($"  Support Cards in UI: {currentSelection.Count} types");
    
    foreach(var kvp in currentSelection)
    {
        Debug.Log($"    - Card {kvp.Key}: {kvp.Value} copies");
    }
}
#endif
```

### Phase 4: Implementation Order

1. **Week 1**: Implement Phase 1 changes (state management)
2. **Week 2**: Implement Phase 2 changes (error handling, logging)
3. **Week 3**: Add Phase 3 testing and validation
4. **Week 4**: Final testing and deployment

### Phase 5: Monitoring and Rollback Plan

#### 5.1 Success Metrics
- Zero failed transactions due to duplicate card additions
- Successful save rate > 95%
- User error reports < 1% of operations
- Load time improvements (due to better caching)

#### 5.2 Rollback Triggers
- Failed save rate > 5%
- Critical blockchain interaction failures
- Major UI/UX regressions
- Performance degradation > 20%

## Files to Modify

### Critical Changes
1. **Assets/Scripts/UI/DeckDisplay.cs** - State management overhaul
2. **Assets/Scripts/Controllers/ScreenEditDeck.cs** - Integration with new state system
3. **Assets/Scripts/Contracts/PepemonCardDeck.cs** - Enhanced logging

### Supporting Changes
4. **Assets/Scripts/Controllers/CardPreview.cs** - Updated UI interaction
5. Add new utility classes for state management
6. Add comprehensive logging throughout the chain

## Risk Assessment

### High Risk
- Blockchain integration changes could cause transaction failures
- State management changes could cause data loss

### Medium Risk  
- UI changes could confuse existing users
- Performance impact from additional state tracking

### Low Risk
- Logging changes
- Debug utilities

## Mitigation Strategies

1. **Backup System**: Implement local caching of deck states
2. **Progressive Rollout**: Deploy to 10% of users first
3. **Rollback Plan**: Keep old code paths available with feature flags
4. **Extensive Testing**: Full test suite before deployment

This plan addresses the core issues while maintaining system stability and providing a clear path for implementation and testing.
