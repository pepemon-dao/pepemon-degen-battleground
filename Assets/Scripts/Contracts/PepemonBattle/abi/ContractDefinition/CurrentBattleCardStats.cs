using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class CurrentBattleCardStats : CurrentBattleCardStatsBase { }

    public class CurrentBattleCardStatsBase 
    {
        [Parameter("int256", "spd", 1)]
        public virtual BigInteger Spd { get; set; }
        [Parameter("uint256", "inte", 2)]
        public virtual BigInteger Inte { get; set; }
        [Parameter("int256", "def", 3)]
        public virtual BigInteger Def { get; set; }
        [Parameter("int256", "atk", 4)]
        public virtual BigInteger Atk { get; set; }
        [Parameter("int256", "sAtk", 5)]
        public virtual BigInteger SAtk { get; set; }
        [Parameter("int256", "sDef", 6)]
        public virtual BigInteger SDef { get; set; }
    }
}
