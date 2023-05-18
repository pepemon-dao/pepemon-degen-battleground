
namespace Pepemon.Battle
{
    public enum PlayCardType
    {
        Offense,
        SpecialOffense,
        Defense,
        SpecialDefense
    }

    public enum PepemonType
    {
        Fire,
        Grass,
        Water,
        Lightning,
        Wind,
        Poison,
        Ghost,
        Fairy,
        Earth,
        Unknown,
        None,
    }

    public enum CardRarity
    {
        Common,
        Rare,
        Epic
    }

    public enum EffectTo
    {
        Attack,
        SpecialAttack,
        Defense,
        SpecialDefense,
        Speed,
        Intelligence
    }

    public enum EffectFor
    {
        Me,
        Enemy
    }
}
