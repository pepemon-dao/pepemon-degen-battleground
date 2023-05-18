
using System;

namespace Pepemon.Battle
{
    //spd, inte, def, atk, sAtk, sDef - Current stats of battle card (with powerups included)
    //Each param can go into the negatives
    [Serializable]
    public struct CurrentBattleCardStats
    {
        public int spd { get; set; }
        public int inte { get; set; }
        public int def { get; set; }
        public int atk { get; set; }
        public int sAtk { get; set; }
        public int sDef { get; set; }
    }

    [Serializable]
    public struct EffectOne
    {
        // If power is 0, it is equal to the total of all normal offense/defense cards in the current turn.

        //basePower = power if req not met
        public int basePower;

        //triggeredPower = power if req met
        public int triggeredPower;
        public EffectTo effectTo;
        public EffectFor effectFor;
        public int reqCode; //requirement code
    }

    [Serializable]
    public struct EffectMany
    {
        public int power;
        public int numTurns;
        public EffectTo effectTo;
        public EffectFor effectFor;
        public int reqCode; //requirement code
    }
}
