using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonFactory.abi.ContractDefinition
{
    public partial class SupportCardStats : SupportCardStatsBase { }

    public class SupportCardStatsBase 
    {
        [Parameter("bytes32", "currentRoundChanges", 1)]
        public virtual byte[] CurrentRoundChanges { get; set; }
        [Parameter("bytes32", "nextRoundChanges", 2)]
        public virtual byte[] NextRoundChanges { get; set; }
        [Parameter("uint256", "specialCode", 3)]
        public virtual BigInteger SpecialCode { get; set; }
        [Parameter("uint16", "modifierNumberOfNextTurns", 4)]
        public virtual ushort ModifierNumberOfNextTurns { get; set; }
        [Parameter("bool", "isOffense", 5)]
        public virtual bool IsOffense { get; set; }
        [Parameter("bool", "isNormal", 6)]
        public virtual bool IsNormal { get; set; }
        [Parameter("bool", "isStackable", 7)]
        public virtual bool IsStackable { get; set; }
        [Parameter("string", "name", 8)]
        public virtual string Name { get; set; }
        [Parameter("string", "description", 9)]
        public virtual string Description { get; set; }
        [Parameter("string", "ipfsAddr", 10)]
        public virtual string IpfsAddr { get; set; }
        [Parameter("string", "rarity", 11)]
        public virtual string Rarity { get; set; }
    }
}
