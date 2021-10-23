using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/pepemon", order = 1)]
public class Pepemon : ScriptableObject
{
    [ReadOnly, ShowInInspector] public string UID = System.Guid.NewGuid().ToString();

    public string DisplayName;
    public int HealthPoints;
    public PepemonType PType;
    public int Speed;
    public int Intelligence;
    public int Defense;
    public PepemonType NAttackType;
    public int Attack;
    public int SAttack;
    public PepemonType sAttackType;

    public Sprite DisplayIcon;


}
