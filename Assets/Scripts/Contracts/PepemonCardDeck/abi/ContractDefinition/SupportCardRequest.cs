using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonCardDeck.abi.ContractDefinition
{
    public partial class SupportCardRequest : SupportCardRequestBase { }

    public class SupportCardRequestBase 
    {
        [Parameter("uint256", "supportCardId", 1)]
        public virtual BigInteger SupportCardId { get; set; }
        [Parameter("uint256", "amount", 2)]
        public virtual BigInteger Amount { get; set; }
    }
}
