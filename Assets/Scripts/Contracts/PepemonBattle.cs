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

class PepemonBattle
{
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonBattleAddress;

    public static async Task<ulong> GetBattleRNGSeed(ulong battleId)
    {
        var request = new QueryUnityRequest<BattleIdRNGSeedFunction, BattleIdRNGSeedOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new BattleIdRNGSeedFunction { BattleId = battleId },
            Address);

        return (ulong) response.Seed;
    }

    public static async Task<uint> WaitForCreatedBattle(string player1Addr, BlockParameter from)
    {
        var eventLogs = await new BattleCreatedEventDTO()
            .GetEventABI()
            .CreateFilterInput(Address, player1Addr, from, null)
            .WaitForEventAsync<BattleCreatedEventDTO>();

        return (uint)eventLogs.Last().Event.BattleId;
    }
}
