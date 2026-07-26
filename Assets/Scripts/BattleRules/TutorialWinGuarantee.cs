namespace Pepemon.BattleRules
{
    /// <summary>
    /// Pure damage rules, isolated from MonoBehaviour so they can actually be tested.
    ///
    /// These mirror PepemonBattle.sol. The first-battle guarantee lives here rather than
    /// inline in the battle coroutine because it is the single promise the onboarding makes:
    /// the player must not lose their first fight.
    /// </summary>
    public static class TutorialWinGuarantee
    {
        /// <summary>
        /// Damage for one attack. At least 1 always lands, which is what makes any battle
        /// terminate: the defender loses HP on every exchange regardless of how well it
        /// defends. Matches the contract rule.
        /// </summary>
        public static int ResolveDamage(int totalAttackPower, int totalDefensePower)
        {
            return totalAttackPower > totalDefensePower
                ? totalAttackPower - totalDefensePower
                : 1;
        }

        /// <summary>
        /// Caps incoming damage so a protected player is left on 1 HP instead of dying.
        ///
        /// Returns the damage that should actually be applied. When protection is off, or the
        /// blow is not lethal, the damage passes through untouched.
        /// </summary>
        public static int ClampLethalDamage(int currentHp, int incomingDamage, bool guaranteeSurvival)
        {
            if (!guaranteeSurvival) return incomingDamage;
            if (currentHp <= 0) return incomingDamage;
            if (currentHp - incomingDamage > 0) return incomingDamage;

            var survivable = currentHp - 1;
            return survivable > 0 ? survivable : 0;
        }

        /// <summary>
        /// True when this blow would end the battle for the defender.
        /// </summary>
        public static bool IsLethal(int currentHp, int incomingDamage)
        {
            return currentHp - incomingDamage <= 0;
        }
    }
}
