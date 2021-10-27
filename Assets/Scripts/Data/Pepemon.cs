using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/pepemon", order = 1)]
public class Pepemon : ScriptableObject
{
    [ReadOnly, ShowInInspector] public string UID = System.Guid.NewGuid().ToString();

    [HideLabel, PreviewField(55), AssetsOnly]
    [HorizontalGroup("General/Left", 55)]
    public Sprite DisplayIcon;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public string DisplayName;

    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public PepemonType Type;


    [BoxGroup("General")]
    [VerticalGroup("General/Left/Right")]
    [LabelWidth(80)]
    public CardRarity Rarity;

    [TitleGroup("Properties")] public int HealthPoints;
    [TitleGroup("Properties")] public int Speed;
    [TitleGroup("Properties")] public int Intelligence;
    [TitleGroup("Properties")] public int Attack;
    [TitleGroup("Properties")] public int Defense;

    public int SAttack;
}
