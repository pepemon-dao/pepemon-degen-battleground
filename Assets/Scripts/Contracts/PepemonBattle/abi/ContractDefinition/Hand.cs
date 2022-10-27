using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class Hand : HandBase { }

    public class HandBase 
    {
        [Parameter("int256", "health", 1)]
        public virtual BigInteger Health { get; set; }
        [Parameter("uint256", "battleCardId", 2)]
        public virtual BigInteger BattleCardId { get; set; }
        [Parameter("tuple", "currentBCstats", 3)]
        public virtual CurrentBattleCardStats CurrentBCstats { get; set; }
        [Parameter("uint256[8]", "supportCardInHandIds", 4)]
        public virtual List<BigInteger> SupportCardInHandIds { get; set; }
        [Parameter("uint256", "tableSupportCardStats", 5)]
        public virtual BigInteger TableSupportCardStats { get; set; }
        [Parameter("tuple[5]", "tableSupportCards", 6)]
        public virtual List<TableSupportCardStats> TableSupportCards { get; set; }
    }
}
