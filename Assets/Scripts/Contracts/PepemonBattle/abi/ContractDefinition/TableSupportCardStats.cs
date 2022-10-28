using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class TableSupportCardStats : TableSupportCardStatsBase { }

    public class TableSupportCardStatsBase 
    {
        [Parameter("uint256", "supportCardId", 1)]
        public virtual BigInteger SupportCardId { get; set; }
        [Parameter("tuple", "effectMany", 2)]
        public virtual EffectMany EffectMany { get; set; }
    }
}
