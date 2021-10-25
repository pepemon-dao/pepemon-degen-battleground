using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/card", order = 1)]
public class Card : ScriptableObject
{
    [ReadOnly, ShowInInspector] public string UID = System.Guid.NewGuid().ToString();

    public string DisplayName;

    public Sprite Icon;

    public CardType CType;

    [ShowIf("@this.CType == CardType.Offense || this.CType == CardType.SpecialOffense")] public int AttackPower;

    [ShowIf("@this.CType == CardType.Defense || this.CType == CardType.SpecialDefense")] public int DefensePower;
}
