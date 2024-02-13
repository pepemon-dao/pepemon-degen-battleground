using Contracts.PepemonMatchmaker.abi.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using System.Collections.Generic;
using System.Linq;
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
    private static long BattleGasLimit => Web3Controller.instance.GetChainConfig().pepemonGasLimit;

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

    public static async Task<ulong> GetLeaderboardPlayersCount(PepemonLeagues league)
    {
        var request = new QueryUnityRequest<LeaderboardPlayersCountFunction, LeaderboardPlayersCountOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new LeaderboardPlayersCountFunction(), Addresses[(int)league]);

        return response.Count;
    }

    public static async Task<List<(string Address, ulong Ranking)>> GetPlayersRankings(
        PepemonLeagues league, ulong count = 10, ulong offset = 0)
    {
        var request = new QueryUnityRequest<GetPlayersRankingsFunction, GetPlayersRankingsOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new GetPlayersRankingsFunction { Count = count, Offset = offset},
            Addresses[(int)league]);

        var playersRankings = new List<(string, ulong)>();
        
        if (response.Addresses.Count != response.Rankings.Count)
        {
            Debug.LogWarning("GetPlayersRankings: mismatching array sizes");
        }

        for (int i=0; i < response.Rankings.Count; i++)
        {
            playersRankings.Add((response.Addresses[i], response.Rankings[i]));
        }
        return playersRankings;
    }


    public static async Task Enter(PepemonLeagues league, ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SendTransactionAndWaitForReceiptAsync(
            new EnterFunction()
            {
                DeckId = deckId,
                Gas = BattleGasLimit > 0 ? BattleGasLimit : null
            },
            Addresses[(int)league]);
    }

    public static async Task Exit(PepemonLeagues league, ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SendTransactionAndWaitForReceiptAsync(
            new ExitFunction()
            {
                DeckId = deckId,
            },
            Addresses[(int)league]);
    }
}
