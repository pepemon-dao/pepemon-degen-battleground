using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;

public class PepemonMatchmaker
{
    public enum PepemonLeagues
    {
        PvE = 0,
        PvP,
    }

    [Event("BattleFinished")]
    public class BattleFinishedEventDto : IEventDTO
    {
        [Parameter("address", "winner", 1, true)]
        public virtual string Winner { get; set; }
        [Parameter("address", "loser", 2, true)]
        public virtual string Loser { get; set; }
        [Parameter("uint256", "battleId", 3, false)]
        public virtual BigInteger BattleId { get; set; }
    }

    /// <summary>
    /// PepemonMatchmaker address
    /// </summary>
    private static string[] Addresses => Web3Controller.instance.GetChainConfig().pepemonMatchmakerAddresses;
    private static long BattleGasLimit => Web3Controller.instance.GetChainConfig().pepemonGasLimit;
    private static string Abi => Web3Controller.instance.GetChainConfig().pepemonMatchmakerAbi;
    private static Thirdweb.Contract contracts(PepemonLeagues league) => ThirdwebManager.Instance.SDK.GetContract(Addresses[(int)league], Abi);

    public static async Task<string> GetDeckOwner(PepemonLeagues league, ulong deckId)
    {
        return await contracts(league).Read<string>("deckOwner", deckId);
    }

    public static async Task<uint> GetBattleFinishedEvents(
        PepemonLeagues league,
        string playerAddress,
        bool asWinner,
        BlockParameter from,
        BlockParameter to)
    {
        /*if (Utils.IsWebGLBuild())
        {
            Thirdweb.Contract contract = contracts(league);
            var events = await contract.Events.GetAll(
                new EventQueryOptions
                {
                    fromBlock = (int)from.BlockNumber.Value,
                    toBlock = (int)to.BlockNumber.Value,
                    filters = new Dictionary<string, object>
                    {
                        ["winner"] = asWinner ? playerAddress : null,  // address winner
                        ["loser"] = asWinner ? null : playerAddress,   // address loser
                    }
                });
            //events.Last().data todo: cast to event type
        }*/
        var eventLogs = await new BattleFinishedEventDto()
            .GetEventABI()
            .CreateFilterInput(
                Addresses[(int)league],
                asWinner ? playerAddress : null,       // address winner
                asWinner ? null : playerAddress,       // address loser
                from,
                to
             )
            .GetEventsAsync<BattleFinishedEventDto>(contracts(league));

        return (uint)eventLogs.Last().Event.BattleId;
    }

    public static async Task<ulong> GetLeaderboardPlayersCount(PepemonLeagues league)
    {
        return await contracts(league).Read<ulong>("leaderboardPlayersCount");
    }

    public static async Task<List<(string Address, ulong Ranking)>> GetPlayersRankings(
        PepemonLeagues league, ulong count = 10, ulong offset = 0)
    {
        Thirdweb.Contract contract = contracts(league);
        var result = await contract.Read<RankingReturnType>("getPlayersRankings", count, offset);
        var playersRankings = new List<(string, ulong)>();

        if (result.addresses.Count != result.rankings.Count)
        {
            Debug.LogWarning("GetPlayersRankings: mismatching array sizes");
        }

        for (int i = 0; i < result.rankings.Count; i++)
        {
            playersRankings.Add((result.addresses[i], ((ulong)result.rankings[i])));
        }
        return playersRankings;
    }


    public static async Task<bool> Enter(PepemonLeagues league, ulong deckId)
    {
        return (await contracts(league).Write("enter", new TransactionRequest
        {
            gasLimit = BattleGasLimit > 0 ? BattleGasLimit.ToString() : null
        },
        deckId)).receipt.status.IsOne;
    }

    public static async Task<bool> Exit(PepemonLeagues league, ulong deckId)
    {
        return (await contracts(league).Write("exit", deckId)).receipt.status.IsOne;
    }

    private class RankingReturnType
    {
        public List<string> addresses;
        public List<BigInteger> rankings;
    }
}
