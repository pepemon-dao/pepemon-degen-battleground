using Contracts.PepemonBattle.abi.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.RPC.Eth.DTOs;
using System.Linq;
using System.Numerics;

class PepemonBattle
{
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonBattleAddress;

    /// <summary>
    /// Retrieves the random seed generated during a new Battle.
    /// The "ulong" type cannot be used as a return type because it cannot handle the generated seed's size
    /// </summary>
    /// <param name="battleId">ID of the battle</param>
    /// <returns>The uint256 seed if the battle exists, 0 if the battle does not exists</returns>
    public static async Task<BigInteger> GetBattleRNGSeed(ulong battleId)
    {
        var request = new QueryUnityRequest<BattleIdRNGSeedFunction, BattleIdRNGSeedOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new BattleIdRNGSeedFunction { BattleId = battleId },
            Address);

        return response.Seed;
    }

    /// <summary>
    /// Tries to get events based off two player's addresses, if the event is not found, keeps polling for events
    /// and checking them until an event is found or cancellationToken is triggered
    /// </summary>
    /// <param name="playerAddr1">Address of the 1st player, ignored when Null</param>
    /// <param name="playerAddr2">Address of the 2nd player, ignored when Null</param>
    /// <param name="from">Filter from a specific block</param>
    /// <param name="cancellationToken">Stops waiting for the event</param>
    /// <returns>Data of the BattleCreated event if an event was found, null if cancellationToken was triggered</returns>
    public static async Task<BattleCreatedEventData?> WaitForCreatedBattle(
        string playerAddr1,
        string playerAddr2,
        BlockParameter from,
        CancellationToken cancellationToken)
    {
        var eventLogs = await new BattleCreatedEventDTO()
            .GetEventABI()
            .CreateFilterInput(Address, playerAddr1, playerAddr2, from, null)
            .WaitForEventAsync<BattleCreatedEventDTO>(token: cancellationToken);

        var lastEvent = eventLogs.LastOrDefault()?.Event;
        if (lastEvent == null)
        {
            return null;
        }

        return new BattleCreatedEventData()
        {
            BattleId = (ulong) lastEvent.BattleId,
            Player1Addr = lastEvent.Player1Addr,
            Player2Addr = lastEvent.Player2Addr,
            Player1Deck = (ulong)lastEvent.Player1Deck,
            Player2Deck = (ulong)lastEvent.Player2Deck,
        };
    }

    [Serializable]
    public struct BattleCreatedEventData
    {
        public ulong BattleId;
        public string Player1Addr;
        public string Player2Addr;
        public ulong Player1Deck;
        public ulong Player2Deck;
    }
}
