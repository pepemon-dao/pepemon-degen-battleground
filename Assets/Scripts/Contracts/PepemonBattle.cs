using Contracts.PepemonBattle.abi.ContractDefinition;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class PepemonBattle
{
    private const int WaitSleepDurationMs = 6500;
    private const string AbiPath = "abi/PepemonBattle.abi";
    private static readonly string abi = Resources.Load<TextAsset>(AbiPath).text;
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
}
