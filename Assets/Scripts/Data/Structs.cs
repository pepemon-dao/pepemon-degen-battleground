
using System;
using Sirenix.OdinInspector;

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
    [ShowInInspector] public int basePower { get; set; }

    //triggeredPower = power if req met
    [ShowInInspector] public int triggeredPower { get; set; }
    [ShowInInspector] public EffectTo effectTo { get; set; }
    [ShowInInspector] public EffectFor effectFor { get; set; }
    [ShowInInspector] public int reqCode { get; set; } //requirement code
}

[Serializable]
public struct EffectMany
{
    [ShowInInspector] public int power { get; set; }
    [ShowInInspector] public int numTurns { get; set; }
    [ShowInInspector] public EffectTo effectTo { get; set; }
    [ShowInInspector] public EffectFor effectFor { get; set; }
    [ShowInInspector] public int reqCode { get; set; } //requirement code
}