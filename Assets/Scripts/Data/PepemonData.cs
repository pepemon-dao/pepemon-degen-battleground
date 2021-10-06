using UnityEngine;
using Sirenix.OdinInspector;

namespace Pepemon.Data
{
    public enum PepemonType { Fire, Plant, Water };

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
    public class PepemonData : ScriptableObject
    {
        [BoxGroup("Pepemon")] public int hp;
        [BoxGroup("Pepemon")] public PepemonType nature;
        [BoxGroup("Pepemon")] public int intelligence;
        [BoxGroup("Pepemon")] public int speed;

        [BoxGroup("Pepemon/Battle")] public int normalAttack;
        [BoxGroup("Pepemon/Battle")] public int stronglAttack;
        [BoxGroup("Pepemon/Battle")] public int defense;
    }
}