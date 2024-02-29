using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonFactory.abi.ContractDefinition
{
    public partial class ElementWR : ElementWRBase { }

    public class ElementWRBase 
    {
        [Parameter("uint16", "weakness", 1)]
        public virtual ushort Weakness { get; set; }
        [Parameter("uint16", "resistance", 2)]
        public virtual ushort Resistance { get; set; }
    }
}
