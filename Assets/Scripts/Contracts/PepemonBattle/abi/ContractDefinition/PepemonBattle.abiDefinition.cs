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

namespace Contracts.PepemonBattle.abi.ContractDefinition
{


    public partial class PepemonBattleAbiDeployment : PepemonBattleAbiDeploymentBase
    {
        public PepemonBattleAbiDeployment() : base(BYTECODE) { }
        public PepemonBattleAbiDeployment(string byteCode) : base(byteCode) { }
    }

    public class PepemonBattleAbiDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public PepemonBattleAbiDeploymentBase() : base(BYTECODE) { }
        public PepemonBattleAbiDeploymentBase(string byteCode) : base(byteCode) { }
        [Parameter("address", "cardOracleAddress", 1)]
        public virtual string CardOracleAddress { get; set; }
        [Parameter("address", "deckOracleAddress", 2)]
        public virtual string DeckOracleAddress { get; set; }
        [Parameter("address", "randOracleAddress", 3)]
        public virtual string RandOracleAddress { get; set; }
    }

    public partial class AddAdminFunction : AddAdminFunctionBase { }

    [Function("addAdmin")]
    public class AddAdminFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class BattleIdRNGSeedFunction : BattleIdRNGSeedFunctionBase { }

    [Function("battleIdRNGSeed", "uint256")]
    public class BattleIdRNGSeedFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger BattleId { get; set; }
    }

    public partial class CalSupportCardsInHandFunction : CalSupportCardsInHandFunctionBase { }

    [Function("calSupportCardsInHand", typeof(CalSupportCardsInHandOutputDTO))]
    public class CalSupportCardsInHandFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "atkHand", 1)]
        public virtual Hand AtkHand { get; set; }
        [Parameter("tuple", "defHand", 2)]
        public virtual Hand DefHand { get; set; }
    }

    public partial class CheckIfBattleEndedFunction : CheckIfBattleEndedFunctionBase { }

    [Function("checkIfBattleEnded", typeof(CheckIfBattleEndedOutputDTO))]
    public class CheckIfBattleEndedFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "battle", 1)]
        public virtual Battle Battle { get; set; }
    }

    public partial class CreateBattleFunction : CreateBattleFunctionBase { }

    [Function("createBattle", typeof(CreateBattleOutputDTO))]
    public class CreateBattleFunctionBase : FunctionMessage
    {
        [Parameter("address", "p1Addr", 1)]
        public virtual string P1Addr { get; set; }
        [Parameter("uint256", "p1DeckId", 2)]
        public virtual BigInteger P1DeckId { get; set; }
        [Parameter("address", "p2Addr", 3)]
        public virtual string P2Addr { get; set; }
        [Parameter("uint256", "p2DeckId", 4)]
        public virtual BigInteger P2DeckId { get; set; }
    }

    public partial class FightFunction : FightFunctionBase { }

    [Function("fight", typeof(FightOutputDTO))]
    public class FightFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "battle", 1)]
        public virtual Battle Battle { get; set; }
    }

    public partial class GoForBattleFunction : GoForBattleFunctionBase { }

    [Function("goForBattle", typeof(GoForBattleOutputDTO))]
    public class GoForBattleFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "battle", 1)]
        public virtual Battle Battle { get; set; }
    }

    public partial class IsAdminFunction : IsAdminFunctionBase { }

    [Function("isAdmin", "bool")]
    public class IsAdminFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class RenounceAdminFunction : RenounceAdminFunctionBase { }

    [Function("renounceAdmin")]
    public class RenounceAdminFunctionBase : FunctionMessage
    {

    }

    public partial class AdminAddedEventDTO : AdminAddedEventDTOBase { }

    [Event("AdminAdded")]
    public class AdminAddedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true )]
        public virtual string Account { get; set; }
    }

    public partial class AdminRemovedEventDTO : AdminRemovedEventDTOBase { }

    [Event("AdminRemoved")]
    public class AdminRemovedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true )]
        public virtual string Account { get; set; }
    }

    public partial class BattleCreatedEventDTO : BattleCreatedEventDTOBase { }

    [Event("BattleCreated")]
    public class BattleCreatedEventDTOBase : IEventDTO
    {
        [Parameter("address", "player1Addr", 1, true )]
        public virtual string Player1Addr { get; set; }
        [Parameter("address", "player2Addr", 2, true )]
        public virtual string Player2Addr { get; set; }
        [Parameter("uint256", "battleId", 3, false)]
        public virtual BigInteger BattleId { get; set; }
        [Parameter("uint256", "p1DeckId", 4, false)]
        public virtual BigInteger Player1Deck { get; set; }
        [Parameter("uint256", "p2DeckId", 5, false)]
        public virtual BigInteger Player2Deck { get; set; }
    }



    public partial class BattleIdRNGSeedOutputDTO : BattleIdRNGSeedOutputDTOBase { }

    [FunctionOutput]
    public class BattleIdRNGSeedOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger Seed { get; set; }
    }

    public partial class CalSupportCardsInHandOutputDTO : CalSupportCardsInHandOutputDTOBase { }

    [FunctionOutput]
    public class CalSupportCardsInHandOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("tuple", "", 1)]
        public virtual Hand ReturnValue1 { get; set; }
        [Parameter("tuple", "", 2)]
        public virtual Hand ReturnValue2 { get; set; }
    }

    public partial class CheckIfBattleEndedOutputDTO : CheckIfBattleEndedOutputDTOBase { }

    [FunctionOutput]
    public class CheckIfBattleEndedOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
        [Parameter("address", "", 2)]
        public virtual string ReturnValue2 { get; set; }
    }

    public partial class CreateBattleOutputDTO : CreateBattleOutputDTOBase { }

    [FunctionOutput]
    public class CreateBattleOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("tuple", "", 1)]
        public virtual Battle ReturnValue1 { get; set; }
        [Parameter("uint256", "battleId", 2)]
        public virtual BigInteger BattleId { get; set; }
    }

    public partial class FightOutputDTO : FightOutputDTOBase { }

    [FunctionOutput]
    public class FightOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("tuple", "", 1)]
        public virtual Battle ReturnValue1 { get; set; }
    }

    public partial class GoForBattleOutputDTO : GoForBattleOutputDTOBase { }

    [FunctionOutput]
    public class GoForBattleOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("tuple", "", 1)]
        public virtual Battle ReturnValue1 { get; set; }
        [Parameter("address", "winner", 2)]
        public virtual string Winner { get; set; }
    }

    public partial class IsAdminOutputDTO : IsAdminOutputDTOBase { }

    [FunctionOutput]
    public class IsAdminOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }


}
