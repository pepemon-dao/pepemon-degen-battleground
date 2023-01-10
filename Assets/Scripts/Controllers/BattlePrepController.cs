using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BattlePrepController : MonoBehaviour
{

    [TitleGroup("Component References"), SerializeField] GameObject _searchForOpponentButton;
    [TitleGroup("Component References"), SerializeField] GameObject _exitButton;
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;

    [ReadOnly] private PepemonMatchmaker.PepemonLeagues selectedLeague;
    [ReadOnly] private ulong selectedDeck;
    [ReadOnly] private CancellationTokenSource _cancellationTokenSource;

    // loaded on GameController
    public static BattleData battleData;

    void Start()
    {
        battleData ??= new BattleData();
        _searchForOpponentButton.GetComponent<Button>().onClick.AddListener(OnSearchForOpponentButtonClick);
        _exitButton.GetComponent<Button>().onClick.AddListener(onExitButtonClick);
        _deckList.GetComponent<DeckListLoader>().onSelectDeck.AddListener(OnDeckSelected);
    }

    // set by LeagueSelection buttons
    public void SetLeague(int league)
    {
        selectedLeague = (PepemonMatchmaker.PepemonLeagues)league;
    }

    public void OnDeckSelected(ulong deckId)
    {
        selectedDeck = deckId;
    }

    private async void OnSearchForOpponentButtonClick()
    {
        // SetApprovalForAll for CardDeck
        await EnsureDeckTransferApproved();

        var blockNumber = await new BlockParameter().RequestLatestBlockNumber();

        // filter events starting from the next block, to avoid possibly getting the last battle of the player
        var nextBlock = new BlockParameter((blockNumber.BlockNumber.ToUlong() + 1).ToHexBigInteger());

        FindObjectOfType<MainMenuController>().ShowScreen(7);


        Debug.Log("Looking for battle");
        await PepemonMatchmaker.Enter(selectedLeague, selectedDeck);

        var deckOwner = await PepemonMatchmaker.GetDeckOwner(selectedLeague, selectedDeck);

        string player1addr = Web3Controller.instance.SelectedAccountAddress, 
               player2addr = null;

        // If the player is in the waitlist, then its because a battle has Not started yet, the current player's address will
        // be in the second parameter of the BattleCreated event in this case.
        // However if the battle has started, the player's address will be in the first parameter of the BattleCreated event
        if (deckOwner == player1addr)
        {
            player2addr = player1addr;
            player1addr = null;
        }

        _exitButton.GetComponent<Button>().interactable = true;

        Debug.Log("Waiting for CreatedBattle events");

        _cancellationTokenSource = new CancellationTokenSource();
        var battleEvent = await PepemonBattle.WaitForCreatedBattle(
                player1addr,
                player2addr,
                nextBlock, 
                _cancellationTokenSource.Token);

        if (battleEvent != null)
        {
            Debug.Log("Received battle event. BattleId: " + battleEvent?.BattleId);

            await PrepareBattleScene((PepemonBattle.BattleCreatedEventData) battleEvent);
        }
    }

    private async Task PrepareBattleScene(PepemonBattle.BattleCreatedEventData battleEventData)
    {
        Debug.Log("Loading battle data..");

        var reqBattleRngSeed = PepemonBattle.GetBattleRNGSeed(battleEventData.BattleId);
        var reqPlayer1BattleCard = PepemonCardDeck.GetBattleCard(battleEventData.Player1Deck);
        var reqPlayer2BattleCard = PepemonCardDeck.GetBattleCard(battleEventData.Player2Deck);
        var reqPlayer1SupportCards = PepemonCardDeck.GetAllSupportCards(battleEventData.Player1Deck);
        var reqPlayer2SupportCards = PepemonCardDeck.GetAllSupportCards(battleEventData.Player2Deck);
        await Task.WhenAll(reqBattleRngSeed, reqPlayer1BattleCard, reqPlayer2BattleCard, reqPlayer1SupportCards, reqPlayer2SupportCards);

        Debug.Log("Battle data loaded");
        
        battleData.battleRngSeed = reqBattleRngSeed.Result;
        battleData.player1BattleCard = reqPlayer1BattleCard.Result;
        battleData.player2BattleCard = reqPlayer2BattleCard.Result;
        battleData.player1SupportCards = reqPlayer1SupportCards.Result;
        battleData.player2SupportCards = reqPlayer2SupportCards.Result;
    }

    private async void onExitButtonClick()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _exitButton.GetComponent<Button>().interactable = false;
            Debug.Log("Trying to exit matchmaking");
            await PepemonMatchmaker.Exit(selectedLeague, selectedDeck);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Matchmaking Exit failed: " + ex.Message);
        }

        FindObjectOfType<MainMenuController>().ShowScreen(4);
    }

    private async Task EnsureDeckTransferApproved()
    {
        var matchmakerAddress = Web3Controller.instance.GetChainConfig().pepemonMatchmakerAddresses[(int)selectedLeague];
        // necessary to avoid "ERC1155#safeTransferFrom: INVALID_OPERATOR"
        // TODO: place this in an "approve" button
        var approvalOk = await PepemonCardDeck.GetApprovalState(matchmakerAddress);
        if (!approvalOk)
        {
            try
            {
                await PepemonCardDeck.SetApprovalState(true, matchmakerAddress);
                approvalOk = await PepemonCardDeck.GetApprovalState(matchmakerAddress);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("SetApprovedForAll failed: " + ex.Message);
            }
        }
    }

    [Serializable]
    public struct BattleData
    {
        public BigInteger battleRngSeed;
        public ulong player1BattleCard;
        public ulong player2BattleCard;
        public Dictionary<ulong, int> player1SupportCards;
        public Dictionary<ulong, int> player2SupportCards;
    }

}
