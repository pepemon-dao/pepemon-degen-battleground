using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace Contracts.PepemonMatchmaker.abi.ContractDefinition
{


    public partial class PepemonMatchmakerAbiDeployment : PepemonMatchmakerAbiDeploymentBase
    {
        public PepemonMatchmakerAbiDeployment() : base(BYTECODE) { }
        public PepemonMatchmakerAbiDeployment(string byteCode) : base(byteCode) { }
    }

    public class PepemonMatchmakerAbiDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public PepemonMatchmakerAbiDeploymentBase() : base(BYTECODE) { }
        public PepemonMatchmakerAbiDeploymentBase(string byteCode) : base(byteCode) { }
        [Parameter("uint256", "defaultRanking", 1)]
        public virtual BigInteger DefaultRanking { get; set; }
        [Parameter("address", "battleAddress", 2)]
        public virtual string BattleAddress { get; set; }
        [Parameter("address", "deckAddress", 3)]
        public virtual string DeckAddress { get; set; }
        [Parameter("address", "rewardPoolAddress", 4)]
        public virtual string RewardPoolAddress { get; set; }
    }

    public partial class DeckOwnerFunction : DeckOwnerFunctionBase { }

    [Function("deckOwner", "address")]
    public class DeckOwnerFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class EnterFunction : EnterFunctionBase { }

    [Function("enter")]
    public class EnterFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class ExitFunction : ExitFunctionBase { }

    [Function("exit")]
    public class ExitFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class GetEloRatingChangeFunction : GetEloRatingChangeFunctionBase { }

    [Function("getEloRatingChange", "uint256")]
    public class GetEloRatingChangeFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "winnerRating", 1)]
        public virtual BigInteger WinnerRating { get; set; }
        [Parameter("uint256", "loserRating", 2)]
        public virtual BigInteger LoserRating { get; set; }
    }

    public partial class GetWaitingCountFunction : GetWaitingCountFunctionBase { }

    [Function("getWaitingCount", "uint256")]
    public class GetWaitingCountFunctionBase : FunctionMessage
    {

    }

    public partial class OnERC1155BatchReceivedFunction : OnERC1155BatchReceivedFunctionBase { }

    [Function("onERC1155BatchReceived", "bytes4")]
    public class OnERC1155BatchReceivedFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
        [Parameter("address", "", 2)]
        public virtual string ReturnValue2 { get; set; }
        [Parameter("uint256[]", "", 3)]
        public virtual List<BigInteger> ReturnValue3 { get; set; }
        [Parameter("uint256[]", "", 4)]
        public virtual List<BigInteger> ReturnValue4 { get; set; }
        [Parameter("bytes", "", 5)]
        public virtual byte[] ReturnValue5 { get; set; }
    }

    public partial class OnERC1155ReceivedFunction : OnERC1155ReceivedFunctionBase { }

    [Function("onERC1155Received", "bytes4")]
    public class OnERC1155ReceivedFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
        [Parameter("address", "", 2)]
        public virtual string ReturnValue2 { get; set; }
        [Parameter("uint256", "", 3)]
        public virtual BigInteger ReturnValue3 { get; set; }
        [Parameter("uint256", "", 4)]
        public virtual BigInteger ReturnValue4 { get; set; }
        [Parameter("bytes", "", 5)]
        public virtual byte[] ReturnValue5 { get; set; }
    }

    public partial class OwnerFunction : OwnerFunctionBase { }

    [Function("owner", "address")]
    public class OwnerFunctionBase : FunctionMessage
    {

    }

    public partial class PlayerRankingFunction : PlayerRankingFunctionBase { }

    [Function("playerRanking", "uint256")]
    public class PlayerRankingFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class RenounceOwnershipFunction : RenounceOwnershipFunctionBase { }

    [Function("renounceOwnership")]
    public class RenounceOwnershipFunctionBase : FunctionMessage
    {

    }

    public partial class SetBattleContractAddressFunction : SetBattleContractAddressFunctionBase { }

    [Function("setBattleContractAddress")]
    public class SetBattleContractAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "battleContractAddress", 1)]
        public virtual string BattleContractAddress { get; set; }
    }

    public partial class SetDeckContractAddressFunction : SetDeckContractAddressFunctionBase { }

    [Function("setDeckContractAddress")]
    public class SetDeckContractAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "deckContractAddress", 1)]
        public virtual string DeckContractAddress { get; set; }
    }

    public partial class SetKFactorFunction : SetKFactorFunctionBase { }

    [Function("setKFactor")]
    public class SetKFactorFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "kFactor", 1)]
        public virtual BigInteger KFactor { get; set; }
    }

    public partial class SetMatchRangeFunction : SetMatchRangeFunctionBase { }

    [Function("setMatchRange")]
    public class SetMatchRangeFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "matchRange", 1)]
        public virtual BigInteger MatchRange { get; set; }
        [Parameter("uint256", "matchRangePerMinute", 2)]
        public virtual BigInteger MatchRangePerMinute { get; set; }
    }

    public partial class SetRewardPoolAddressFunction : SetRewardPoolAddressFunctionBase { }

    [Function("setRewardPoolAddress")]
    public class SetRewardPoolAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "rewardPoolAddress", 1)]
        public virtual string RewardPoolAddress { get; set; }
    }

    public partial class SupportsInterfaceFunction : SupportsInterfaceFunctionBase { }

    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "interfaceId", 1)]
        public virtual byte[] InterfaceId { get; set; }
    }

    public partial class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner", 1)]
        public virtual string NewOwner { get; set; }
    }

    public partial class WaitingDecksFunction : WaitingDecksFunctionBase { }

    [Function("waitingDecks", typeof(WaitingDecksOutputDTO))]
    public class WaitingDecksFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class BattleFinishedEventDTO : BattleFinishedEventDTOBase { }

    [Event("BattleFinished")]
    public class BattleFinishedEventDTOBase : IEventDTO
    {
        [Parameter("address", "winner", 1, true )]
        public virtual string Winner { get; set; }
        [Parameter("address", "loser", 2, true )]
        public virtual string Loser { get; set; }
        [Parameter("uint256", "battleId", 3, false )]
        public virtual BigInteger BattleId { get; set; }
    }

    public partial class OwnershipTransferredEventDTO : OwnershipTransferredEventDTOBase { }

    [Event("OwnershipTransferred")]
    public class OwnershipTransferredEventDTOBase : IEventDTO
    {
        [Parameter("address", "previousOwner", 1, true )]
        public virtual string PreviousOwner { get; set; }
        [Parameter("address", "newOwner", 2, true )]
        public virtual string NewOwner { get; set; }
    }

    public partial class DeckOwnerOutputDTO : DeckOwnerOutputDTOBase { }

    [FunctionOutput]
    public class DeckOwnerOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }





    public partial class GetEloRatingChangeOutputDTO : GetEloRatingChangeOutputDTOBase { }

    [FunctionOutput]
    public class GetEloRatingChangeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetWaitingCountOutputDTO : GetWaitingCountOutputDTOBase { }

    [FunctionOutput]
    public class GetWaitingCountOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }





    public partial class OwnerOutputDTO : OwnerOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class PlayerRankingOutputDTO : PlayerRankingOutputDTOBase { }

    [FunctionOutput]
    public class PlayerRankingOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }













    public partial class SupportsInterfaceOutputDTO : SupportsInterfaceOutputDTOBase { }

    [FunctionOutput]
    public class SupportsInterfaceOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }



    public partial class WaitingDecksOutputDTO : WaitingDecksOutputDTOBase { }

    [FunctionOutput]
    public class WaitingDecksOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("uint256", "enterTimestamp", 2)]
        public virtual BigInteger EnterTimestamp { get; set; }
    }
}
