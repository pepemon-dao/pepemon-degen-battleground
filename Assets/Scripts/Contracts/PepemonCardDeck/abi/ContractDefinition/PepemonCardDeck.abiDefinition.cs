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

namespace Contracts.PepemonCardDeck.abi.ContractDefinition
{


    public partial class PepemonCardDeckAbiDeployment : PepemonCardDeckAbiDeploymentBase
    {
        public PepemonCardDeckAbiDeployment() : base(BYTECODE) { }
        public PepemonCardDeckAbiDeployment(string byteCode) : base(byteCode) { }
    }

    public class PepemonCardDeckAbiDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public PepemonCardDeckAbiDeploymentBase() : base(BYTECODE) { }
        public PepemonCardDeckAbiDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class MaxSupportCardsFunction : MaxSupportCardsFunctionBase { }

    [Function("MAX_SUPPORT_CARDS", "uint256")]
    public class MaxSupportCardsFunctionBase : FunctionMessage
    {

    }

    public partial class MinSupportCardsFunction : MinSupportCardsFunctionBase { }

    [Function("MIN_SUPPORT_CARDS", "uint256")]
    public class MinSupportCardsFunctionBase : FunctionMessage
    {

    }

    public partial class AddBattleCardToDeckFunction : AddBattleCardToDeckFunctionBase { }

    [Function("addBattleCardToDeck")]
    public class AddBattleCardToDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("uint256", "battleCardId", 2)]
        public virtual BigInteger BattleCardId { get; set; }
    }

    public partial class AddSupportCardsToDeckFunction : AddSupportCardsToDeckFunctionBase { }

    [Function("addSupportCardsToDeck")]
    public class AddSupportCardsToDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("tuple[]", "supportCards", 2)]
        public virtual List<SupportCardRequest> SupportCards { get; set; }
    }

    public partial class ApproveFunction : ApproveFunctionBase { }

    [Function("approve")]
    public class ApproveFunctionBase : FunctionMessage
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }
        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class BalanceOfFunction : BalanceOfFunctionBase { }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
    }

    public partial class BattleCardAddressFunction : BattleCardAddressFunctionBase { }

    [Function("battleCardAddress", "address")]
    public class BattleCardAddressFunctionBase : FunctionMessage
    {

    }

    public partial class CreateDeckFunction : CreateDeckFunctionBase { }

    [Function("createDeck")]
    public class CreateDeckFunctionBase : FunctionMessage
    {

    }

    public partial class DecksFunction : DecksFunctionBase { }

    [Function("decks", typeof(DecksOutputDTO))]
    public class DecksFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetAllSupportCardsInDeckFunction : GetAllSupportCardsInDeckFunctionBase { }

    [Function("getAllSupportCardsInDeck", "uint256[]")]
    public class GetAllSupportCardsInDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class GetApprovedFunction : GetApprovedFunctionBase { }

    [Function("getApproved", "address")]
    public class GetApprovedFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class GetBattleCardInDeckFunction : GetBattleCardInDeckFunctionBase { }

    [Function("getBattleCardInDeck", "uint256")]
    public class GetBattleCardInDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class GetCardTypesInDeckFunction : GetCardTypesInDeckFunctionBase { }

    [Function("getCardTypesInDeck", "uint256[]")]
    public class GetCardTypesInDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class GetCountOfCardTypeInDeckFunction : GetCountOfCardTypeInDeckFunctionBase { }

    [Function("getCountOfCardTypeInDeck", "uint256")]
    public class GetCountOfCardTypeInDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("uint256", "_cardTypeId", 2)]
        public virtual BigInteger CardTypeId { get; set; }
    }

    public partial class GetDeckCountFunction : GetDeckCountFunctionBase { }

    [Function("getDeckCount", "uint256")]
    public class GetDeckCountFunctionBase : FunctionMessage
    {
        [Parameter("address", "player", 1)]
        public virtual string Player { get; set; }
    }

    public partial class GetSupportCardCountInDeckFunction : GetSupportCardCountInDeckFunctionBase { }

    [Function("getSupportCardCountInDeck", "uint256")]
    public class GetSupportCardCountInDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class IsApprovedForAllFunction : IsApprovedForAllFunctionBase { }

    [Function("isApprovedForAll", "bool")]
    public class IsApprovedForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
        [Parameter("address", "operator", 2)]
        public virtual string Operator { get; set; }
    }

    public partial class NameFunction : NameFunctionBase { }

    [Function("name", "string")]
    public class NameFunctionBase : FunctionMessage
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

    public partial class OwnerOfFunction : OwnerOfFunctionBase { }

    [Function("ownerOf", "address")]
    public class OwnerOfFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class PlayerToDecksFunction : PlayerToDecksFunctionBase { }

    [Function("playerToDecks", "uint256")]
    public class PlayerToDecksFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string Player { get; set; }
        [Parameter("uint256", "", 2)]
        public virtual BigInteger DeckIdx { get; set; }
    }

    public partial class RemoveBattleCardFromDeckFunction : RemoveBattleCardFromDeckFunctionBase { }

    [Function("removeBattleCardFromDeck")]
    public class RemoveBattleCardFromDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
    }

    public partial class RemoveSupportCardsFromDeckFunction : RemoveSupportCardsFromDeckFunctionBase { }

    [Function("removeSupportCardsFromDeck")]
    public class RemoveSupportCardsFromDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("tuple[]", "_supportCards", 2)]
        public virtual List<SupportCardRequest> SupportCards { get; set; }
    }

    public partial class RenounceOwnershipFunction : RenounceOwnershipFunctionBase { }

    [Function("renounceOwnership")]
    public class RenounceOwnershipFunctionBase : FunctionMessage
    {

    }

    public partial class SafeTransferFromFunction : SafeTransferFromFunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }
        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class SafeTransferFrom1Function : SafeTransferFrom1FunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFrom1FunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }
        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
        [Parameter("bytes", "_data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetApprovalForAllFunction : SetApprovalForAllFunctionBase { }

    [Function("setApprovalForAll")]
    public class SetApprovalForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "operator", 1)]
        public virtual string Operator { get; set; }
        [Parameter("bool", "approved", 2)]
        public virtual bool Approved { get; set; }
    }

    public partial class SetBattleCardAddressFunction : SetBattleCardAddressFunctionBase { }

    [Function("setBattleCardAddress")]
    public class SetBattleCardAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "_battleCardAddress", 1)]
        public virtual string BattleCardAddress { get; set; }
    }

    public partial class SetMaxSupportCardsFunction : SetMaxSupportCardsFunctionBase { }

    [Function("setMaxSupportCards")]
    public class SetMaxSupportCardsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_maxSupportCards", 1)]
        public virtual BigInteger MaxSupportCards { get; set; }
    }

    public partial class SetMinSupportCardsFunction : SetMinSupportCardsFunctionBase { }

    [Function("setMinSupportCards")]
    public class SetMinSupportCardsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_minSupportCards", 1)]
        public virtual BigInteger MinSupportCards { get; set; }
    }

    public partial class SetSupportCardAddressFunction : SetSupportCardAddressFunctionBase { }

    [Function("setSupportCardAddress")]
    public class SetSupportCardAddressFunctionBase : FunctionMessage
    {
        [Parameter("address", "_supportCardAddress", 1)]
        public virtual string SupportCardAddress { get; set; }
    }

    public partial class ShuffleDeckFunction : ShuffleDeckFunctionBase { }

    [Function("shuffleDeck", "uint256[]")]
    public class ShuffleDeckFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_deckId", 1)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("uint256", "_seed", 2)]
        public virtual BigInteger Seed { get; set; }
    }

    public partial class SupportCardAddressFunction : SupportCardAddressFunctionBase { }

    [Function("supportCardAddress", "address")]
    public class SupportCardAddressFunctionBase : FunctionMessage
    {

    }

    public partial class SupportsInterfaceFunction : SupportsInterfaceFunctionBase { }

    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "interfaceId", 1)]
        public virtual byte[] InterfaceId { get; set; }
    }

    public partial class SymbolFunction : SymbolFunctionBase { }

    [Function("symbol", "string")]
    public class SymbolFunctionBase : FunctionMessage
    {

    }

    public partial class TokenURIFunction : TokenURIFunctionBase { }

    [Function("tokenURI", "string")]
    public class TokenURIFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class TransferFromFunction : TransferFromFunctionBase { }

    [Function("transferFrom")]
    public class TransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }
        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner", 1)]
        public virtual string NewOwner { get; set; }
    }

    public partial class ApprovalEventDTO : ApprovalEventDTOBase { }

    [Event("Approval")]
    public class ApprovalEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true )]
        public virtual string Owner { get; set; }
        [Parameter("address", "approved", 2, true )]
        public virtual string Approved { get; set; }
        [Parameter("uint256", "tokenId", 3, true )]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class ApprovalForAllEventDTO : ApprovalForAllEventDTOBase { }

    [Event("ApprovalForAll")]
    public class ApprovalForAllEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true )]
        public virtual string Owner { get; set; }
        [Parameter("address", "operator", 2, true )]
        public virtual string Operator { get; set; }
        [Parameter("bool", "approved", 3, false )]
        public virtual bool Approved { get; set; }
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

    public partial class TransferEventDTO : TransferEventDTOBase { }

    [Event("Transfer")]
    public class TransferEventDTOBase : IEventDTO
    {
        [Parameter("address", "from", 1, true )]
        public virtual string From { get; set; }
        [Parameter("address", "to", 2, true )]
        public virtual string To { get; set; }
        [Parameter("uint256", "tokenId", 3, true )]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class MaxSupportCardsOutputDTO : MaxSupportCardsOutputDTOBase { }

    [FunctionOutput]
    public class MaxSupportCardsOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class MinSupportCardsOutputDTO : MinSupportCardsOutputDTOBase { }

    [FunctionOutput]
    public class MinSupportCardsOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }







    public partial class BalanceOfOutputDTO : BalanceOfOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class BattleCardAddressOutputDTO : BattleCardAddressOutputDTOBase { }

    [FunctionOutput]
    public class BattleCardAddressOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }



    public partial class DecksOutputDTO : DecksOutputDTOBase { }

    [FunctionOutput]
    public class DecksOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "battleCardId", 1)]
        public virtual BigInteger BattleCardId { get; set; }
        [Parameter("uint256", "supportCardCount", 2)]
        public virtual BigInteger SupportCardCount { get; set; }
    }

    public partial class GetAllSupportCardsInDeckOutputDTO : GetAllSupportCardsInDeckOutputDTOBase { }

    [FunctionOutput]
    public class GetAllSupportCardsInDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<ulong> SupportCards { get; set; }  // note: manually renamed and changed from BigInteger to ulong
    }

    public partial class GetApprovedOutputDTO : GetApprovedOutputDTOBase { }

    [FunctionOutput]
    public class GetApprovedOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class GetBattleCardInDeckOutputDTO : GetBattleCardInDeckOutputDTOBase { }

    [FunctionOutput]
    public class GetBattleCardInDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetCardTypesInDeckOutputDTO : GetCardTypesInDeckOutputDTOBase { }

    [FunctionOutput]
    public class GetCardTypesInDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<BigInteger> ReturnValue1 { get; set; }
    }

    public partial class GetCountOfCardTypeInDeckOutputDTO : GetCountOfCardTypeInDeckOutputDTOBase { }

    [FunctionOutput]
    public class GetCountOfCardTypeInDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetDeckCountOutputDTO : GetDeckCountOutputDTOBase { }

    [FunctionOutput]
    public class GetDeckCountOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger DeckCount { get; set; }
    }

    public partial class GetSupportCardCountInDeckOutputDTO : GetSupportCardCountInDeckOutputDTOBase { }

    [FunctionOutput]
    public class GetSupportCardCountInDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class IsApprovedForAllOutputDTO : IsApprovedForAllOutputDTOBase { }

    [FunctionOutput]
    public class IsApprovedForAllOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
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

    public partial class OwnerOfOutputDTO : OwnerOfOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOfOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class PlayerToDecksOutputDTO : PlayerToDecksOutputDTOBase { }

    [FunctionOutput]
    public class PlayerToDecksOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger DeckId { get; set; }
    }





















    public partial class ShuffleDeckOutputDTO : ShuffleDeckOutputDTOBase { }

    [FunctionOutput]
    public class ShuffleDeckOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<BigInteger> ReturnValue1 { get; set; }
    }

    public partial class SupportCardAddressOutputDTO : SupportCardAddressOutputDTOBase { }

    [FunctionOutput]
    public class SupportCardAddressOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
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

    public partial class TokenURIOutputDTO : TokenURIOutputDTOBase { }

    [FunctionOutput]
    public class TokenURIOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }




}
