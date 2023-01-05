using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BattlePrepController : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _searchForOpponentButton;
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;

    [ReadOnly] private PepemonMatchmaker.PepemonLeagues selectedLeague;
    [ReadOnly] private ulong selectedDeck;

    void Start()
    {
        _searchForOpponentButton.GetComponent<Button>().onClick.AddListener(OnSearchForOpponentButtonClick);
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
        
        // filter events starting from this block
        var nextBlock = new BlockParameter((blockNumber.BlockNumber.ToUlong() + 1).ToHexBigInteger());

        FindObjectOfType<MainMenuController>().ShowScreen(7);

        Debug.Log("battle start");
        await PepemonMatchmaker.EnterBattle(selectedLeague, selectedDeck);

        Debug.Log("waiting events");
        var battleId = await PepemonBattle.WaitForCreatedBattle(
            Web3Controller.instance.SelectedAccountAddress,
            nextBlock);
        Debug.Log("battlid:" + battleId);

        Debug.Log("battle ended");
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
}
