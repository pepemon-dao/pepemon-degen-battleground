using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using UnityEngine;

namespace Contracts.PepemonCardDeck.abi.ContractDefinition
{
    [Serializable]
    public partial class SupportCardRequest : SupportCardRequestBase { }
    [Serializable]
    public class SupportCardRequestBase 
    {
        [Parameter("uint256", "supportCardId", 1)]
        [SerializeField] public virtual BigInteger SupportCardId { get; set; }
        [Parameter("uint256", "amount", 2)]
        [SerializeField]  public virtual BigInteger Amount { get; set; }
    }
}
