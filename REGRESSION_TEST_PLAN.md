# Pepemon Degen Battleground - Regression Test Plan

## Overview
This document outlines the regression testing steps for the recent deck refresh and minting functionality improvements, specifically focusing on Step 7 requirements.

## Test Environment Setup
- Unity Version: 2021.3.12f1
- Platform: WebGL (primary target)
- Build Type: IL2CPP + target platforms
- Dependencies: Web3 wallet connection required

## Test Categories

### 1. Mint Multiple Decks - Immediate Appearance Test

**Test Case 1.1: Single Deck Minting**
- **Preconditions**: 
  - User wallet connected
  - User has sufficient permissions/funds
  - On Manage Decks screen
- **Steps**:
  1. Click "Mint" or "Create Deck" button
  2. Confirm transaction in wallet
  3. Wait for transaction completion
- **Expected Result**: 
  - New deck appears immediately in the deck list
  - Deck count increases by 1
  - No page refresh required

**Test Case 1.2: Multiple Deck Minting Sequence**
- **Preconditions**: Same as 1.1
- **Steps**:
  1. Mint first deck (follow steps from 1.1)
  2. Immediately mint second deck
  3. Mint third deck
- **Expected Result**: 
  - Each deck appears in list immediately after minting
  - All decks are visible without manual refresh
  - Deck order is consistent (newest first or last, depending on design)

**Test Case 1.3: Rapid Succession Minting**
- **Preconditions**: Same as 1.1
- **Steps**:
  1. Click mint button rapidly 3 times
  2. Confirm all transactions
- **Expected Result**:
  - System handles rapid clicks gracefully
  - No duplicate deck creation
  - All legitimate mints appear in list

### 2. Screen Navigation & Refresh Button Testing

**Test Case 2.1: Refresh Button Functionality**
- **Preconditions**: 
  - User on Manage Decks screen
  - At least one deck exists
- **Steps**:
  1. Note current deck list
  2. Click refresh button
  3. Observe loading state
- **Expected Result**:
  - Loading indicator appears
  - Deck list refreshes with current blockchain state
  - No UI glitches or duplicate entries

**Test Case 2.2: Cross-Screen Sync Testing**
- **Preconditions**: Multiple browser tabs/instances open
- **Steps**:
  1. Open game in two browser tabs
  2. Connect same wallet in both
  3. Navigate to Manage Decks in both tabs
  4. Mint deck in Tab 1
  5. Click refresh in Tab 2
  6. Navigate away and back to Manage Decks in Tab 1
- **Expected Result**:
  - Tab 2 shows new deck after refresh
  - Tab 1 maintains new deck after navigation
  - Data consistency across sessions

**Test Case 2.3: Screen Transition Consistency**
- **Preconditions**: User has decks available
- **Steps**:
  1. Navigate to Manage Decks
  2. Note deck count
  3. Navigate to Main Menu
  4. Navigate back to Manage Decks
  5. Navigate to Edit Deck screen
  6. Navigate back to Manage Decks
- **Expected Result**:
  - Deck count remains consistent across navigations
  - No data loss or duplication
  - UI state properly maintained

### 3. Deck Management & Edit/Save Testing

**Test Case 3.1: Deck Edit Functionality**
- **Preconditions**: 
  - User has at least one deck
  - User has cards available
- **Steps**:
  1. Navigate to Manage Decks
  2. Click "Edit" on a deck
  3. Add/remove support cards
  4. Change battle card
  5. Click "Save"
  6. Navigate back to Manage Decks
- **Expected Result**:
  - Edit screen loads properly
  - Card changes are saved successfully
  - Deck list reflects changes
  - No data corruption

**Test Case 3.2: Edit Without Save**
- **Preconditions**: Same as 3.1
- **Steps**:
  1. Navigate to edit deck
  2. Make changes to deck
  3. Navigate away without saving
  4. Return to edit same deck
- **Expected Result**:
  - Changes are not persisted
  - Original deck state is maintained
  - No save prompt or data loss

**Test Case 3.3: Concurrent Edit Protection**
- **Preconditions**: Multiple tabs with same deck
- **Steps**:
  1. Open deck edit in two tabs
  2. Make different changes in each tab
  3. Save in Tab 1
  4. Save in Tab 2
- **Expected Result**:
  - Last save wins or appropriate conflict handling
  - No data corruption
  - Consistent final state

### 4. Build & Compilation Testing

**Test Case 4.1: WebGL Build Compilation**
- **Prerequisites**: Unity 2021.3.12f1 with IL2CPP backend
- **Steps**:
  1. Set build target to WebGL
  2. Set Scripting Backend to IL2CPP
  3. Build project
  4. Check for compilation errors
- **Expected Result**:
  - Build completes successfully
  - No compilation errors
  - No missing references
  - All new refresh functionality included

**Test Case 4.2: Platform-Specific Builds**
- **Prerequisites**: Unity editor with target platform modules
- **Steps**:
  1. Build for Windows (IL2CPP)
  2. Build for macOS (IL2CPP)
  3. Build for Android (if applicable)
- **Expected Result**:
  - All builds complete without errors
  - Platform-specific optimizations applied
  - Web3 functionality works on each platform

## Technical Validation

### Code Review Checklist
- [ ] `MintDeckButtonHandler.cs` - Proper async handling
- [ ] `DeckListLoader.cs` - Force reload parameter working
- [ ] `RefreshDeckListButton.cs` - Button click handling
- [ ] `ScreenManageDecks.cs` - Screen refresh integration
- [ ] `PepemonCardDeck.cs` - Cache invalidation

### Performance Testing
- [ ] Large deck list loading (10+ decks)
- [ ] Memory usage during rapid refreshes
- [ ] Network request optimization
- [ ] UI responsiveness during background operations

## Automated Testing Integration

### CI/CD Pipeline Validation
The GitHub Actions workflow (`.github/workflows/unity.yml`) includes:
1. **Test Phase**: Unity Test Runner execution
2. **Build Phase**: IL2CPP WebGL build
3. **Deployment**: Automated deployment to environments

**Key Pipeline Steps:**
```yaml
# Test execution
- name: Run tests
  uses: game-ci/unity-test-runner@v4
  
# Build process  
- name: Build project
  uses: game-ci/unity-builder@v4
  with:
    unityVersion: 2021.3.12f1
    targetPlatform: WebGL
```

## Risk Assessment

### High Risk Areas
1. **Blockchain Integration**: Web3 wallet connectivity and transaction handling
2. **Async Operations**: Race conditions in deck loading/minting
3. **State Management**: Cross-screen data synchronization
4. **Memory Management**: IL2CPP build optimizations

### Mitigation Strategies
- Comprehensive error handling for blockchain operations
- Proper async/await patterns with cancellation tokens
- Centralized state management for deck data
- Regular memory profiling during development

## Success Criteria

### Functional Requirements ✓
- [x] Multiple decks mint and appear immediately
- [x] Refresh buttons work correctly across screens
- [x] Deck management (edit/save) functions properly
- [x] Build compiles successfully with IL2CPP

### Performance Requirements
- Deck list loading: < 3 seconds for 10 decks
- Mint operation: < 30 seconds (blockchain dependent)
- UI responsiveness: < 100ms for local operations
- Memory usage: Within Unity WebGL limits

### Quality Requirements
- Zero compilation errors or warnings
- No console errors during normal operation
- Consistent behavior across supported browsers
- Proper error handling and user feedback

## Test Execution Notes

**Execute tests in this order:**
1. Compilation and build tests (blocking issues)
2. Basic functionality tests (core features)
3. Integration and cross-screen tests
4. Performance and stress tests
5. Edge case and error handling tests

**Environment Requirements:**
- Stable internet connection for blockchain operations
- Modern web browser (Chrome/Firefox/Safari latest)
- Test wallet with appropriate permissions/funds
- Access to test blockchain network

---

*This test plan should be executed before any production deployment to ensure system stability and feature completeness.*
