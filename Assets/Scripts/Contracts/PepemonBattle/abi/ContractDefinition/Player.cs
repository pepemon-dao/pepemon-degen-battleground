using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class Player : PlayerBase { }

    public class PlayerBase 
    {
        [Parameter("address", "playerAddr", 1)]
        public virtual string PlayerAddr { get; set; }
        [Parameter("uint256", "deckId", 2)]
        public virtual BigInteger DeckId { get; set; }
        [Parameter("tuple", "hand", 3)]
        public virtual Hand Hand { get; set; }
        [Parameter("uint256[60]", "totalSupportCardIds", 4)]
        public virtual List<BigInteger> TotalSupportCardIds { get; set; }
        [Parameter("uint256", "playedCardCount", 5)]
        public virtual BigInteger PlayedCardCount { get; set; }
    }
}
