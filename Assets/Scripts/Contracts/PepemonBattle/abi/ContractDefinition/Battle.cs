using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Contracts.PepemonBattle.abi.ContractDefinition
{
    public partial class Battle : BattleBase { }

    public class BattleBase 
    {
        [Parameter("uint256", "battleId", 1)]
        public virtual BigInteger BattleId { get; set; }
        [Parameter("tuple", "player1", 2)]
        public virtual Player Player1 { get; set; }
        [Parameter("tuple", "player2", 3)]
        public virtual Player Player2 { get; set; }
        [Parameter("uint256", "currentTurn", 4)]
        public virtual BigInteger CurrentTurn { get; set; }
        [Parameter("uint8", "attacker", 5)]
        public virtual byte Attacker { get; set; }
        [Parameter("uint8", "turnHalves", 6)]
        public virtual byte TurnHalves { get; set; }
    }
}
