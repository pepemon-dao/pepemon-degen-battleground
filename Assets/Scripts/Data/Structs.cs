
using System;
using UnityEngine;

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
    [SerializeField] public int basePower { get; set; }

    //triggeredPower = power if req met
    [SerializeField] public int triggeredPower { get; set; }
    [SerializeField] public EffectTo effectTo { get; set; }
    [SerializeField] public EffectFor effectFor { get; set; }
    [SerializeField] public int reqCode { get; set; } //requirement code
}

[Serializable]
public struct EffectMany
{
    [SerializeField] public int power { get; set; }
    [SerializeField] public int numTurns { get; set; }
    [SerializeField] public EffectTo effectTo { get; set; }
    [SerializeField] public EffectFor effectFor { get; set; }
    [SerializeField] public int reqCode { get; set; } //requirement code
}