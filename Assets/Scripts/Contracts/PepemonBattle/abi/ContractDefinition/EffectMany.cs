using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class EffectMany : EffectManyBase { }

    public class EffectManyBase 
    {
        [Parameter("int256", "power", 1)]
        public virtual BigInteger Power { get; set; }
        [Parameter("uint256", "numTurns", 2)]
        public virtual BigInteger NumTurns { get; set; }
        [Parameter("uint8", "effectTo", 3)]
        public virtual byte EffectTo { get; set; }
        [Parameter("uint8", "effectFor", 4)]
        public virtual byte EffectFor { get; set; }
        [Parameter("uint256", "reqCode", 5)]
        public virtual BigInteger ReqCode { get; set; }
    }
}
