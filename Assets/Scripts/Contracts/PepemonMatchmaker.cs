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
    private static string Abi => "[{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"defaultRanking\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"battleAddress\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"deckAddress\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"rewardPoolAddress\",\"type\":\"address\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"AdminAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"AdminRemoved\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"winner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"loser\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"battleId\",\"type\":\"uint256\"}],\"name\":\"BattleFinished\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"addAdmin\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"addPveDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"deckOwner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"enter\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"exit\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"forceExit\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"winnerRating\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"loserRating\",\"type\":\"uint256\"}],\"name\":\"getEloRatingChange\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"count\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"offset\",\"type\":\"uint256\"}],\"name\":\"getPlayersRankings\",\"outputs\":[{\"internalType\":\"address[]\",\"name\":\"addresses\",\"type\":\"address[]\"},{\"internalType\":\"uint256[]\",\"name\":\"rankings\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getWaitingCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"isAdmin\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"isPveMode\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"leaderboardPlayers\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"leaderboardPlayersCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"},{\"internalType\":\"bytes\",\"name\":\"\",\"type\":\"bytes\"}],\"name\":\"onERC1155BatchReceived\",\"outputs\":[{\"internalType\":\"bytes4\",\"name\":\"\",\"type\":\"bytes4\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"\",\"type\":\"bytes\"}],\"name\":\"onERC1155Received\",\"outputs\":[{\"internalType\":\"bytes4\",\"name\":\"\",\"type\":\"bytes4\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"\",\"type\":\"bytes\"}],\"name\":\"onERC721Received\",\"outputs\":[{\"internalType\":\"bytes4\",\"name\":\"\",\"type\":\"bytes4\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"playerRanking\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"removePveDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceAdmin\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bool\",\"name\":\"allow\",\"type\":\"bool\"}],\"name\":\"setAllowBattleAgainstOneself\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"battleContractAddress\",\"type\":\"address\"}],\"name\":\"setBattleContractAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"deckContractAddress\",\"type\":\"address\"}],\"name\":\"setDeckContractAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"kFactor\",\"type\":\"uint256\"}],\"name\":\"setKFactor\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"matchRange\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"matchRangePerMinute\",\"type\":\"uint256\"}],\"name\":\"setMatchRange\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bool\",\"name\":\"enable\",\"type\":\"bool\"}],\"name\":\"setPveMode\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"rewardPoolAddress\",\"type\":\"address\"}],\"name\":\"setRewardPoolAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"waitingDecks\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"enterTimestamp\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"}]";
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
        var (addresses, rankings) = await contract.Read<(List<string>, List<ulong>)>("getPlayersRankings", count, offset);
        var playersRankings = new List<(string, ulong)>();

        if (addresses.Count != rankings.Count)
        {
            Debug.LogWarning("GetPlayersRankings: mismatching array sizes");
        }

        for (int i = 0; i < rankings.Count; i++)
        {
            playersRankings.Add((addresses[i], rankings[i]));
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
}
