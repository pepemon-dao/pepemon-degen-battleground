using UnityEngine;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/card", order = 1)]
public class Card : ScriptableObject
{
    [ReadOnly, ShowInInspector] public string UID = System.Guid.NewGuid().ToString();

    [HideLabel, PreviewField(55), AssetsOnly]
    [HorizontalGroup("General/Left", 55)]
    public Sprite Icon;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public int ID;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public string DisplayName;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public string CardDescription;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public CardRarity Rarity;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public PlayCardType Type;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public Sprite CardEffectSprite;

    [Obsolete("shouldnt be used anymore")]
    [TitleGroup("Properties")]
    [ShowIf("@this.Type == PlayCardType.Offense || this.Type == PlayCardType.SpecialOffense")] public int AttackPower;

    [Obsolete("shouldnt be used anymore")]
    [TitleGroup("Properties")]
    [ShowIf("@this.Type == PlayCardType.Defense || this.Type == PlayCardType.SpecialDefense")] public int DefensePower;

    [TitleGroup("Properties")]
    public EffectOne[] effectOnes;

    [TitleGroup("Properties")]
    public EffectMany effectMany;

    [TitleGroup("Properties")]
    public bool Unstackable;

    [TitleGroup("Properties")]
    public bool Unresettable;

    [TitleGroup("Additionals"), ShowIf("Unstackable")] public int UnstackableAmount;
    [TitleGroup("Additionals"), ShowIf("Unresettable")] public int UnresettableAmount;


    public bool IsAttackingCard()
    {
        if (Type == PlayCardType.SpecialOffense || Type == PlayCardType.Offense) return true;
        else return false;
    }
}