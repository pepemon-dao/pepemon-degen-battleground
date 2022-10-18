using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Numerics;
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
        return await Web3Controller.instance.provider.CallContract<ulong>(
            new Web3CallContractArgs
            {
                abi = abi,
                contract = Address,
                method = "battleIdRNGSeed",
                parameters = new object[] { battleId }
            }
        );
    }

    public static async Task<BattleInfo> WaitForNextBattleCreatedEvent(string address, int startingBlock, CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            int latest = await Web3Controller.instance.provider.GetLatestBlockNumber();
            List<Web3Event> events = await GetBattleCreatedEvents(address, startingBlock, latest);

            if (events.Count > 0)
            {
                return ToBattleInfo(events[0]);
            }

            startingBlock = latest;
            await Task.Delay(WaitSleepDurationMs);
        }

        return default;
    }

    public static async Task<List<Web3Event>> GetBattleCreatedEvents(string playerAddress, int fromBlock, int toBlock)
    {
        Task<List<Web3Event>> player1Events = GetBattleCreatedEvents(fromBlock, toBlock, "player1Addr", playerAddress);
        Task<List<Web3Event>> player2Events = GetBattleCreatedEvents(fromBlock, toBlock, "player2Addr", playerAddress);
        await Task.WhenAll(player1Events, player2Events);

        List<Web3Event> events = new List<Web3Event>();
        events.AddRange(player1Events.Result);
        events.AddRange(player2Events.Result);
        events.Sort((a, b) => a.blockNumber.CompareTo(b.blockNumber));
        return events;
    }

    private static async Task<List<Web3Event>> GetBattleCreatedEvents(int fromBlock, int toBlock, string filterParameter, string filterValue)
    {
        return await Web3Controller.instance.provider.GetPastEvents(
            new Web3GetPastEventsArgs
            {
                abi = abi,
                contract = Address,
                eventName = "BattleCreated",
                fromBlock = fromBlock.ToString(),
                toBlock = toBlock.ToString(),
                filters = new List<Web3EventParameter>(new Web3EventParameter[] {
                    new Web3EventParameter{
                        param = filterParameter,
                        value = filterValue
                    }
                })
            }
        );
    }

    private static BattleInfo ToBattleInfo(Web3Event ev)
    {
        BattleInfo battleInfo = new BattleInfo();
        foreach (var p in ev.parameters)
        {
            if (p.param == "battleId")
            {
                battleInfo.battleId = Convert.ToUInt64(p.value);
            }
            else if (p.param == "player1Addr")
            {
                battleInfo.player1Addr = p.value;
            }
            else if (p.param == "player2Addr")
            {
                battleInfo.player2Addr = p.value;
            }
        }
        return battleInfo;
    }

    [Serializable]
    public struct BattleInfo
    {
        public ulong battleId;
        public string player1Addr;
        public string player2Addr;
    }
}
