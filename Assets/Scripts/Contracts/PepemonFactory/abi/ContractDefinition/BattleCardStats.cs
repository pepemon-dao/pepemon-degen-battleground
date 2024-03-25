using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonFactory.abi.ContractDefinition
{
    public partial class BattleCardStats : BattleCardStatsBase { }

    public class BattleCardStatsBase 
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
}
