using Contracts.PepemonMatchmaker.abi.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class PepemonMatchmaker
{
    public enum PepemonLeagues
    {
        Rice = 0,
        PepeKarp,
        Chad
    }

    /// <summary>
    /// PepemonMatchmaker address
    /// </summary>
    private static string[] Addresses => Web3Controller.instance.GetChainConfig().pepemonMatchmakerAddresses;


    public static async Task<string> GetDeckOwner(PepemonLeagues league, ulong deckId)
    {
        var request = new QueryUnityRequest<DeckOwnerFunction, DeckOwnerOutputDTO>(
        Web3Controller.instance.GetUnityRpcRequestClientFactory(),
        Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new DeckOwnerFunction { DeckId = deckId },
            Addresses[(int)league]);

        return response.ReturnValue1;
    }

    public static async Task<uint> GetBattleFinishedEvents(
        PepemonLeagues league,
        string playerAddress,
        bool asWinner,
        BlockParameter from,
        BlockParameter to)
    {
        var eventLogs = await new BattleFinishedEventDTO()
            .GetEventABI()
            .CreateFilterInput(
                Addresses[(int)league],
                asWinner ? playerAddress : null,       // address winner
                asWinner ? null : playerAddress,       // address loser
                from,
                to
             )
            .GetEventsAsync<BattleFinishedEventDTO>();

        return (uint)eventLogs.Last().Event.BattleId;
    }


    public static async Task Enter(PepemonLeagues league, ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransactionAsync(
            new EnterFunction()
            {
                DeckId = deckId,
            },
            Addresses[(int)league]);
    }

    public static async Task Exit(PepemonLeagues league, ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransactionAsync(
            new ExitFunction()
            {
                DeckId = deckId,
            },
            Addresses[(int)league]);
    }
}
