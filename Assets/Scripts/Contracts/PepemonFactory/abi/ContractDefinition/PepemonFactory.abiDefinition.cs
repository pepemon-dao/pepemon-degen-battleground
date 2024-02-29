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

namespace Contracts.PepemonFactory.abi.ContractDefinition
{


    public partial class pepemonFactoryAbiDeployment : pepemonFactoryAbiDeploymentBase
    {
        public pepemonFactoryAbiDeployment() : base(BYTECODE) { }
        public pepemonFactoryAbiDeployment(string byteCode) : base(byteCode) { }
    }

    public class pepemonFactoryAbiDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public pepemonFactoryAbiDeploymentBase() : base(BYTECODE) { }
        public pepemonFactoryAbiDeploymentBase(string byteCode) : base(byteCode) { }
    }

    public partial class AddMinterFunction : AddMinterFunctionBase { }

    [Function("addMinter")]
    public class AddMinterFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class AddWhitelistAdminFunction : AddWhitelistAdminFunctionBase { }

    [Function("addWhitelistAdmin")]
    public class AddWhitelistAdminFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class AirdropFunction : AirdropFunctionBase { }

    [Function("airdrop")]
    public class AirdropFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("address[]", "_addresses", 2)]
        public virtual List<string> Addresses { get; set; }
    }

    public partial class BalanceOfFunction : BalanceOfFunctionBase { }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunctionBase : FunctionMessage
    {
        [Parameter("address", "_owner", 1)]
        public virtual string Owner { get; set; }
        [Parameter("uint256", "_id", 2)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class BalanceOfBatchFunction : BalanceOfBatchFunctionBase { }

    [Function("balanceOfBatch", "uint256[]")]
    public class BalanceOfBatchFunctionBase : FunctionMessage
    {
        [Parameter("address[]", "_owners", 1)]
        public virtual List<string> Owners { get; set; }
        [Parameter("uint256[]", "_ids", 2)]
        public virtual List<ulong> Ids { get; set; }
    }

    public partial class BaseMetadataURIFunction : BaseMetadataURIFunctionBase { }

    [Function("baseMetadataURI", "string")]
    public class BaseMetadataURIFunctionBase : FunctionMessage
    {

    }

    public partial class BatchGetBattleCardStatsFunction : BatchGetBattleCardStatsFunctionBase { }

    [Function("batchGetBattleCardStats", typeof(BatchGetBattleCardStatsOutputDTO))]
    public class BatchGetBattleCardStatsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "minId", 1)]
        public virtual BigInteger MinId { get; set; }
        [Parameter("uint256", "maxId", 2)]
        public virtual BigInteger MaxId { get; set; }
    }

    public partial class BatchMintFunction : BatchMintFunctionBase { }

    [Function("batchMint")]
    public class BatchMintFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "start", 1)]
        public virtual BigInteger Start { get; set; }
        [Parameter("uint256", "end", 2)]
        public virtual BigInteger End { get; set; }
        [Parameter("address", "to", 3)]
        public virtual string To { get; set; }
    }

    public partial class BattleCardStatsFunction : BattleCardStatsFunctionBase { }

    [Function("battleCardStats", typeof(BattleCardStatsOutputDTO))]
    public class BattleCardStatsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class BurnFunction : BurnFunctionBase { }

    [Function("burn")]
    public class BurnFunctionBase : FunctionMessage
    {
        [Parameter("address", "_account", 1)]
        public virtual string Account { get; set; }
        [Parameter("uint256", "_id", 2)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "_amount", 3)]
        public virtual BigInteger Amount { get; set; }
    }

    public partial class ContractURIFunction : ContractURIFunctionBase { }

    [Function("contractURI", "string")]
    public class ContractURIFunctionBase : FunctionMessage
    {

    }

    public partial class ConvertFunction : ConvertFunctionBase { }

    [Function("convert", "bytes32")]
    public class ConvertFunctionBase : FunctionMessage
    {
        [Parameter("int16[14]", "arr", 1)]
        public virtual List<short> Arr { get; set; }
    }

    public partial class CreateBattleCardFunction : CreateBattleCardFunctionBase { }

    [Function("createBattleCard", "uint256")]
    public class CreateBattleCardFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "_stats", 1)]
        public virtual BattleCardStats Stats { get; set; }
        [Parameter("uint256", "_maxSupply", 2)]
        public virtual BigInteger MaxSupply { get; set; }
        [Parameter("uint256", "_initialSupply", 3)]
        public virtual BigInteger InitialSupply { get; set; }
        [Parameter("string", "_uri", 4)]
        public virtual string Uri { get; set; }
        [Parameter("bytes", "_data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class CreateSupportCardFunction : CreateSupportCardFunctionBase { }

    [Function("createSupportCard", "uint256")]
    public class CreateSupportCardFunctionBase : FunctionMessage
    {
        [Parameter("tuple", "_stats", 1)]
        public virtual SupportCardStats Stats { get; set; }
        [Parameter("uint256", "_maxSupply", 2)]
        public virtual BigInteger MaxSupply { get; set; }
        [Parameter("uint256", "_initialSupply", 3)]
        public virtual BigInteger InitialSupply { get; set; }
        [Parameter("string", "_uri", 4)]
        public virtual string Uri { get; set; }
        [Parameter("bytes", "_data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class CreatorsFunction : CreatorsFunctionBase { }

    [Function("creators", "address")]
    public class CreatorsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class DeconvertFunction : DeconvertFunctionBase { }

    [Function("deconvert", "int16[14]")]
    public class DeconvertFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "num", 1)]
        public virtual byte[] Num { get; set; }
    }

    public partial class ElementDecodeFunction : ElementDecodeFunctionBase { }

    [Function("elementDecode", "string")]
    public class ElementDecodeFunctionBase : FunctionMessage
    {
        [Parameter("uint16", "", 1)]
        public virtual ushort ReturnValue1 { get; set; }
    }

    public partial class EndMintingFunction : EndMintingFunctionBase { }

    [Function("endMinting")]
    public class EndMintingFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_id", 1)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class GetLastTokenIDFunction : GetLastTokenIDFunctionBase { }

    [Function("getLastTokenID", "uint256")]
    public class GetLastTokenIDFunctionBase : FunctionMessage
    {

    }

    public partial class IsApprovedForAllFunction : IsApprovedForAllFunctionBase { }

    [Function("isApprovedForAll", "bool")]
    public class IsApprovedForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "_owner", 1)]
        public virtual string Owner { get; set; }
        [Parameter("address", "_operator", 2)]
        public virtual string Operator { get; set; }
    }

    public partial class IsMinterFunction : IsMinterFunctionBase { }

    [Function("isMinter", "bool")]
    public class IsMinterFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class IsWhitelistAdminFunction : IsWhitelistAdminFunctionBase { }

    [Function("isWhitelistAdmin", "bool")]
    public class IsWhitelistAdminFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class MaxSupplyFunction : MaxSupplyFunctionBase { }

    [Function("maxSupply", "uint256")]
    public class MaxSupplyFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_id", 1)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class MintFunction : MintFunctionBase { }

    [Function("mint")]
    public class MintFunctionBase : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public virtual string To { get; set; }
        [Parameter("uint256", "_id", 2)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "_quantity", 3)]
        public virtual BigInteger Quantity { get; set; }
        [Parameter("bytes", "_data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class NameFunction : NameFunctionBase { }

    [Function("name", "string")]
    public class NameFunctionBase : FunctionMessage
    {

    }

    public partial class OwnerFunction : OwnerFunctionBase { }

    [Function("owner", "address")]
    public class OwnerFunctionBase : FunctionMessage
    {

    }

    public partial class RemoveMinterFunction : RemoveMinterFunctionBase { }

    [Function("removeMinter")]
    public class RemoveMinterFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class RemoveWhitelistAdminFunction : RemoveWhitelistAdminFunctionBase { }

    [Function("removeWhitelistAdmin")]
    public class RemoveWhitelistAdminFunctionBase : FunctionMessage
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class RenounceMinterFunction : RenounceMinterFunctionBase { }

    [Function("renounceMinter")]
    public class RenounceMinterFunctionBase : FunctionMessage
    {

    }

    public partial class RenounceOwnershipFunction : RenounceOwnershipFunctionBase { }

    [Function("renounceOwnership")]
    public class RenounceOwnershipFunctionBase : FunctionMessage
    {

    }

    public partial class RenounceWhitelistAdminFunction : RenounceWhitelistAdminFunctionBase { }

    [Function("renounceWhitelistAdmin")]
    public class RenounceWhitelistAdminFunctionBase : FunctionMessage
    {

    }

    public partial class SafeBatchTransferFromFunction : SafeBatchTransferFromFunctionBase { }

    [Function("safeBatchTransferFrom")]
    public class SafeBatchTransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "_from", 1)]
        public virtual string From { get; set; }
        [Parameter("address", "_to", 2)]
        public virtual string To { get; set; }
        [Parameter("uint256[]", "_ids", 3)]
        public virtual List<BigInteger> Ids { get; set; }
        [Parameter("uint256[]", "_amounts", 4)]
        public virtual List<BigInteger> Amounts { get; set; }
        [Parameter("bytes", "_data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SafeTransferFromFunction : SafeTransferFromFunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "_from", 1)]
        public virtual string From { get; set; }
        [Parameter("address", "_to", 2)]
        public virtual string To { get; set; }
        [Parameter("uint256", "_id", 3)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "_amount", 4)]
        public virtual BigInteger Amount { get; set; }
        [Parameter("bytes", "_data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetApprovalForAllFunction : SetApprovalForAllFunctionBase { }

    [Function("setApprovalForAll")]
    public class SetApprovalForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "_operator", 1)]
        public virtual string Operator { get; set; }
        [Parameter("bool", "_approved", 2)]
        public virtual bool Approved { get; set; }
    }

    public partial class SetBaseMetadataURIFunction : SetBaseMetadataURIFunctionBase { }

    [Function("setBaseMetadataURI")]
    public class SetBaseMetadataURIFunctionBase : FunctionMessage
    {
        [Parameter("string", "newURI", 1)]
        public virtual string NewURI { get; set; }
    }

    public partial class SetBattleCardStatsFunction : SetBattleCardStatsFunctionBase { }

    [Function("setBattleCardStats")]
    public class SetBattleCardStatsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("tuple", "x", 2)]
        public virtual BattleCardStats X { get; set; }
    }

    public partial class SetContractURIFunction : SetContractURIFunctionBase { }

    [Function("setContractURI")]
    public class SetContractURIFunctionBase : FunctionMessage
    {
        [Parameter("string", "newURI", 1)]
        public virtual string NewURI { get; set; }
    }

    public partial class SetElementDecodeFunction : SetElementDecodeFunctionBase { }

    [Function("setElementDecode")]
    public class SetElementDecodeFunctionBase : FunctionMessage
    {
        [Parameter("uint16", "element", 1)]
        public virtual ushort Element { get; set; }
        [Parameter("string", "x", 2)]
        public virtual string X { get; set; }
    }

    public partial class SetSupportCardStatsFunction : SetSupportCardStatsFunctionBase { }

    [Function("setSupportCardStats")]
    public class SetSupportCardStatsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("tuple", "x", 2)]
        public virtual SupportCardStats X { get; set; }
    }

    public partial class SetWeakResistFunction : SetWeakResistFunctionBase { }

    [Function("setWeakResist")]
    public class SetWeakResistFunctionBase : FunctionMessage
    {
        [Parameter("uint16", "element", 1)]
        public virtual ushort Element { get; set; }
        [Parameter("tuple", "x", 2)]
        public virtual ElementWR X { get; set; }
    }

    public partial class SupportCardStatsFunction : SupportCardStatsFunctionBase { }

    [Function("supportCardStats", typeof(SupportCardStatsOutputDTO))]
    public class SupportCardStatsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class SupportsInterfaceFunction : SupportsInterfaceFunctionBase { }

    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "_interfaceID", 1)]
        public virtual byte[] InterfaceID { get; set; }
    }

    public partial class SymbolFunction : SymbolFunctionBase { }

    [Function("symbol", "string")]
    public class SymbolFunctionBase : FunctionMessage
    {

    }

    public partial class TokenMaxSupplyFunction : TokenMaxSupplyFunctionBase { }

    [Function("tokenMaxSupply", "uint256")]
    public class TokenMaxSupplyFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class TokenSupplyFunction : TokenSupplyFunctionBase { }

    [Function("tokenSupply", "uint256")]
    public class TokenSupplyFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class TotalSupplyFunction : TotalSupplyFunctionBase { }

    [Function("totalSupply", "uint256")]
    public class TotalSupplyFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_id", 1)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner", 1)]
        public virtual string NewOwner { get; set; }
    }

    public partial class UriFunction : UriFunctionBase { }

    [Function("uri", "string")]
    public class UriFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_id", 1)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class WeakResistFunction : WeakResistFunctionBase { }

    [Function("weakResist", typeof(WeakResistOutputDTO))]
    public class WeakResistFunctionBase : FunctionMessage
    {
        [Parameter("uint16", "", 1)]
        public virtual ushort ReturnValue1 { get; set; }
    }

    public partial class ApprovalForAllEventDTO : ApprovalForAllEventDTOBase { }

    [Event("ApprovalForAll")]
    public class ApprovalForAllEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true)]
        public virtual string Account { get; set; }
        [Parameter("address", "operator", 2, true)]
        public virtual string Operator { get; set; }
        [Parameter("bool", "approved", 3, false)]
        public virtual bool Approved { get; set; }
    }

    public partial class MinterAddedEventDTO : MinterAddedEventDTOBase { }

    [Event("MinterAdded")]
    public class MinterAddedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true)]
        public virtual string Account { get; set; }
    }

    public partial class MinterRemovedEventDTO : MinterRemovedEventDTOBase { }

    [Event("MinterRemoved")]
    public class MinterRemovedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true)]
        public virtual string Account { get; set; }
    }

    public partial class OwnershipTransferredEventDTO : OwnershipTransferredEventDTOBase { }

    [Event("OwnershipTransferred")]
    public class OwnershipTransferredEventDTOBase : IEventDTO
    {
        [Parameter("address", "previousOwner", 1, true)]
        public virtual string PreviousOwner { get; set; }
        [Parameter("address", "newOwner", 2, true)]
        public virtual string NewOwner { get; set; }
    }

    public partial class TransferBatchEventDTO : TransferBatchEventDTOBase { }

    [Event("TransferBatch")]
    public class TransferBatchEventDTOBase : IEventDTO
    {
        [Parameter("address", "operator", 1, true)]
        public virtual string Operator { get; set; }
        [Parameter("address", "from", 2, true)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 3, true)]
        public virtual string To { get; set; }
        [Parameter("uint256[]", "ids", 4, false)]
        public virtual List<BigInteger> Ids { get; set; }
        [Parameter("uint256[]", "values", 5, false)]
        public virtual List<BigInteger> Values { get; set; }
    }

    public partial class TransferSingleEventDTO : TransferSingleEventDTOBase { }

    [Event("TransferSingle")]
    public class TransferSingleEventDTOBase : IEventDTO
    {
        [Parameter("address", "operator", 1, true)]
        public virtual string Operator { get; set; }
        [Parameter("address", "from", 2, true)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 3, true)]
        public virtual string To { get; set; }
        [Parameter("uint256", "id", 4, false)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "value", 5, false)]
        public virtual BigInteger Value { get; set; }
    }

    public partial class UriEventDTO : UriEventDTOBase { }

    [Event("URI")]
    public class UriEventDTOBase : IEventDTO
    {
        [Parameter("string", "value", 1, false)]
        public virtual string Value { get; set; }
        [Parameter("uint256", "id", 2, true)]
        public virtual BigInteger Id { get; set; }
    }

    public partial class WhitelistAdminAddedEventDTO : WhitelistAdminAddedEventDTOBase { }

    [Event("WhitelistAdminAdded")]
    public class WhitelistAdminAddedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true)]
        public virtual string Account { get; set; }
    }

    public partial class WhitelistAdminRemovedEventDTO : WhitelistAdminRemovedEventDTOBase { }

    [Event("WhitelistAdminRemoved")]
    public class WhitelistAdminRemovedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, true)]
        public virtual string Account { get; set; }
    }







    public partial class BalanceOfOutputDTO : BalanceOfOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class BalanceOfBatchOutputDTO : BalanceOfBatchOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfBatchOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<BigInteger> ReturnValue1 { get; set; }
    }

    public partial class BaseMetadataURIOutputDTO : BaseMetadataURIOutputDTOBase { }

    [FunctionOutput]
    public class BaseMetadataURIOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class BatchGetBattleCardStatsOutputDTO : BatchGetBattleCardStatsOutputDTOBase { }

    [FunctionOutput]
    public class BatchGetBattleCardStatsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("tuple[]", "", 1)]
        public virtual List<BattleCardStats> ReturnValue1 { get; set; }
    }



    public partial class BattleCardStatsOutputDTO : BattleCardStatsOutputDTOBase { }

    [FunctionOutput]
    public class BattleCardStatsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint16", "element", 1)]
        public virtual ushort Element { get; set; }
        [Parameter("uint16", "hp", 2)]
        public virtual ushort Hp { get; set; }
        [Parameter("uint16", "speed", 3)]
        public virtual ushort Speed { get; set; }
        [Parameter("uint16", "intelligence", 4)]
        public virtual ushort Intelligence { get; set; }
        [Parameter("uint16", "defense", 5)]
        public virtual ushort Defense { get; set; }
        [Parameter("uint16", "attack", 6)]
        public virtual ushort Attack { get; set; }
        [Parameter("uint16", "specialAttack", 7)]
        public virtual ushort SpecialAttack { get; set; }
        [Parameter("uint16", "specialDefense", 8)]
        public virtual ushort SpecialDefense { get; set; }
        [Parameter("uint16", "level", 9)]
        public virtual ushort Level { get; set; }
        [Parameter("string", "name", 10)]
        public virtual string Name { get; set; }
        [Parameter("string", "description", 11)]
        public virtual string Description { get; set; }
        [Parameter("string", "ipfsAddr", 12)]
        public virtual string IpfsAddr { get; set; }
        [Parameter("string", "rarity", 13)]
        public virtual string Rarity { get; set; }
    }



    public partial class ContractURIOutputDTO : ContractURIOutputDTOBase { }

    [FunctionOutput]
    public class ContractURIOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class ConvertOutputDTO : ConvertOutputDTOBase { }

    [FunctionOutput]
    public class ConvertOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }





    public partial class CreatorsOutputDTO : CreatorsOutputDTOBase { }

    [FunctionOutput]
    public class CreatorsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class DeconvertOutputDTO : DeconvertOutputDTOBase { }

    [FunctionOutput]
    public class DeconvertOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("int16[14]", "", 1)]
        public virtual List<short> ReturnValue1 { get; set; }
    }

    public partial class ElementDecodeOutputDTO : ElementDecodeOutputDTOBase { }

    [FunctionOutput]
    public class ElementDecodeOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }



    public partial class GetLastTokenIDOutputDTO : GetLastTokenIDOutputDTOBase { }

    [FunctionOutput]
    public class GetLastTokenIDOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class IsApprovedForAllOutputDTO : IsApprovedForAllOutputDTOBase { }

    [FunctionOutput]
    public class IsApprovedForAllOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "isOperator", 1)]
        public virtual bool IsOperator { get; set; }
    }

    public partial class IsMinterOutputDTO : IsMinterOutputDTOBase { }

    [FunctionOutput]
    public class IsMinterOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class IsWhitelistAdminOutputDTO : IsWhitelistAdminOutputDTOBase { }

    [FunctionOutput]
    public class IsWhitelistAdminOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class MaxSupplyOutputDTO : MaxSupplyOutputDTOBase { }

    [FunctionOutput]
    public class MaxSupplyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }



    public partial class NameOutputDTO : NameOutputDTOBase { }

    [FunctionOutput]
    public class NameOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class OwnerOutputDTO : OwnerOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }





























    public partial class SupportCardStatsOutputDTO : SupportCardStatsOutputDTOBase { }

    [FunctionOutput]
    public class SupportCardStatsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "currentRoundChanges", 1)]
        public virtual byte[] CurrentRoundChanges { get; set; }
        [Parameter("bytes32", "nextRoundChanges", 2)]
        public virtual byte[] NextRoundChanges { get; set; }
        [Parameter("uint256", "specialCode", 3)]
        public virtual BigInteger SpecialCode { get; set; }
        [Parameter("uint16", "modifierNumberOfNextTurns", 4)]
        public virtual ushort ModifierNumberOfNextTurns { get; set; }
        [Parameter("bool", "isOffense", 5)]
        public virtual bool IsOffense { get; set; }
        [Parameter("bool", "isNormal", 6)]
        public virtual bool IsNormal { get; set; }
        [Parameter("bool", "isStackable", 7)]
        public virtual bool IsStackable { get; set; }
        [Parameter("string", "name", 8)]
        public virtual string Name { get; set; }
        [Parameter("string", "description", 9)]
        public virtual string Description { get; set; }
        [Parameter("string", "ipfsAddr", 10)]
        public virtual string IpfsAddr { get; set; }
        [Parameter("string", "rarity", 11)]
        public virtual string Rarity { get; set; }
    }

    public partial class SupportsInterfaceOutputDTO : SupportsInterfaceOutputDTOBase { }

    [FunctionOutput]
    public class SupportsInterfaceOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class SymbolOutputDTO : SymbolOutputDTOBase { }

    [FunctionOutput]
    public class SymbolOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class TokenMaxSupplyOutputDTO : TokenMaxSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TokenMaxSupplyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class TokenSupplyOutputDTO : TokenSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TokenSupplyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class TotalSupplyOutputDTO : TotalSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TotalSupplyOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }



    public partial class UriOutputDTO : UriOutputDTOBase { }

    [FunctionOutput]
    public class UriOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class WeakResistOutputDTO : WeakResistOutputDTOBase { }

    [FunctionOutput]
    public class WeakResistOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint16", "weakness", 1)]
        public virtual ushort Weakness { get; set; }
        [Parameter("uint16", "resistance", 2)]
        public virtual ushort Resistance { get; set; }
    }
}
