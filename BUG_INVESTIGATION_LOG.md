# Mint-Deck Auto-Refresh Bug Investigation

## Bug Description
After minting a deck in the Mint-Deck screen, the deck list does not automatically refresh to show the newly created deck.

## Investigation Findings

### Root Cause: Multiple DeckListLoader Instances
The application has multiple `DeckListLoader` instances across different screens:

1. **Mint Deck Screen**: `MintDeckButtonHandler._deckList` → DeckListLoader
2. **Manage Decks Screen**: `ScreenManageDecks._editDeckListLoader` → DeckListLoader  
3. **Battle Prep Screen**: `BattlePrepController._deckList` → DeckListLoader

### Call Chain Analysis
```
MintDeckButtonHandler.OnButtonClicked()
  ↓ 
PepemonCardDeck.CreateDeck() ✅ WORKS
  ↓
MintDeckButtonHandler._deckList.ReloadAllDecks() ✅ WORKS BUT WRONG INSTANCE
  ↓
DeckListLoader.ReloadAllDecks() ✅ EXECUTES SUCCESSFULLY
  ↓  
PepemonCardDeck.GetPlayerDecks() ✅ RETURNS NEW DECK ID
  ↓
UI Updated on Mint Screen ✅ BUT USER IS ON DIFFERENT SCREEN
```

### Evidence Collected

#### ✅ a) `ReloadAllDecks()` IS executed
- Added logging to `DeckListLoader.ReloadAllDecks()` 
- Added logging to `MintDeckButtonHandler.OnButtonClicked()`
- Method executes successfully and completes

#### ❌ b) The newly created deck ID IS retrieved correctly  
- Added logging to `PepemonCardDeck.CreateDeck()`
- Added logging to `PepemonCardDeck.GetPlayerDecks()`  
- Contract calls succeed and return the new deck ID
- **But it's displayed on the wrong DeckListLoader instance**

### Contributing Factors Investigated

#### ❌ Local cache inside `PepemonCardDeck` not invalidated
- **RULED OUT**: `PepemonCardDeck.GetPlayerDecks()` queries blockchain directly
- No local caching mechanism found
- Each call fetches fresh data from the contract

#### ❌ `ReloadAllDecks()` early-out because `loadingInProgress` is still true
- **RULED OUT**: Added logging shows method executes fully
- `loadingInProgress` flag works as expected
- No early returns detected

#### ✅ The loader instance on Mint screen is not the one being refreshed
- **CONFIRMED**: This is the root cause
- `MintDeckButtonHandler` refreshes its own `_deckList` DeckListLoader
- User sees a different screen with a separate DeckListLoader instance
- The visible DeckListLoader is never notified of the new deck

## Technical Details

### Code Locations
- `Assets/Scripts/UI/MintDeckButtonHandler.cs:29` - Calls ReloadAllDecks on wrong instance
- `Assets/Scripts/UI/Loaders/DeckListLoader.cs:27-111` - ReloadAllDecks implementation  
- `Assets/Scripts/Contracts/PepemonCardDeck.cs:85-97` - GetPlayerDecks implementation
- `Assets/Scripts/Controllers/ScreenManageDecks.cs:25` - Manage Decks ReloadAllDecks call

### Screen Management
- `MainMenuController.OnManageDecksButtonClick()` calls `_screenManageDecks.ReloadAllDecks()`
- This refreshes the Manage Decks screen's DeckListLoader
- But MintDeckButtonHandler refreshes its own separate instance
- No cross-screen communication exists

## Recommended Fix
Implement a centralized event system where:
1. `PepemonCardDeck.CreateDeck()` broadcasts a "deck created" event
2. All `DeckListLoader` instances subscribe to this event
3. All instances refresh automatically when any new deck is created

Alternatively:
- Make `MintDeckButtonHandler` find and refresh ALL `DeckListLoader` instances
- Or use a singleton pattern for deck list management

## Status
✅ Bug reproduced and root cause identified  
✅ All contributing factors investigated and documented  
🔄 Ready for implementation of fix
