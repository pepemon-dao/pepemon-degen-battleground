using NUnit.Framework;
using Pepemon.BattleRules;

/// <summary>
/// The first battle is advertised as unloseable. These tests are the only thing that actually
/// holds that promise - before this, the "guaranteed" win was a stat bias plus a random seed
/// and a new player could and did lose their first fight.
/// </summary>
public class TutorialWinGuaranteeTests
{
    [Test]
    public void ResolveDamage_AlwaysDealsAtLeastOne()
    {
        Assert.AreEqual(1, TutorialWinGuarantee.ResolveDamage(0, 100));
        Assert.AreEqual(1, TutorialWinGuarantee.ResolveDamage(5, 5));
        Assert.AreEqual(1, TutorialWinGuarantee.ResolveDamage(3, 18));
    }

    [Test]
    public void ResolveDamage_SubtractsDefenceWhenAttackWins()
    {
        Assert.AreEqual(9, TutorialWinGuarantee.ResolveDamage(19, 10));
        Assert.AreEqual(13, TutorialWinGuarantee.ResolveDamage(19, 6));
    }

    [Test]
    public void ClampLethalDamage_LeavesNonLethalHitsAlone()
    {
        Assert.AreEqual(6, TutorialWinGuarantee.ClampLethalDamage(30, 6, guaranteeSurvival: true));
    }

    [Test]
    public void ClampLethalDamage_LeavesPlayerOnOneHp()
    {
        var dmg = TutorialWinGuarantee.ClampLethalDamage(4, 99, guaranteeSurvival: true);

        Assert.AreEqual(3, dmg, "damage should be reduced to leave exactly 1 HP");
        Assert.AreEqual(1, 4 - dmg);
    }

    [Test]
    public void ClampLethalDamage_AtOneHpBlocksEverything()
    {
        Assert.AreEqual(0, TutorialWinGuarantee.ClampLethalDamage(1, 50, guaranteeSurvival: true));
    }

    [Test]
    public void ClampLethalDamage_DoesNothingWhenProtectionIsOff()
    {
        // Regular bot battles and PvP must stay losable.
        Assert.AreEqual(99, TutorialWinGuarantee.ClampLethalDamage(4, 99, guaranteeSurvival: false));
    }

    [Test]
    public void ProtectedPlayerSurvivesAnUnlimitedBeating()
    {
        var hp = 20;

        for (var i = 0; i < 500; i++)
        {
            var dmg = TutorialWinGuarantee.ClampLethalDamage(hp, 999, guaranteeSurvival: true);
            hp -= dmg;

            Assert.Greater(hp, 0, $"protected player died on exchange {i}");
        }
    }

    /// <summary>
    /// The guarantee is only meaningful if the battle also ends. Minimum damage of 1 means the
    /// bot's HP strictly decreases every exchange, so the player's win is reached in bounded time.
    /// </summary>
    [Test]
    public void BotAlwaysDiesInBoundedRounds()
    {
        var botHp = 20;
        var exchanges = 0;

        while (botHp > 0)
        {
            // Worst case for the player: the bot out-defends every single attack.
            botHp -= TutorialWinGuarantee.ResolveDamage(1, 999);
            exchanges++;

            Assert.Less(exchanges, 1000, "battle failed to terminate");
        }

        Assert.AreEqual(20, exchanges, "worst case is exactly one damage per exchange");
    }

    [Test]
    public void IsLethal_DetectsTheKillingBlow()
    {
        Assert.IsTrue(TutorialWinGuarantee.IsLethal(5, 5));
        Assert.IsTrue(TutorialWinGuarantee.IsLethal(5, 9));
        Assert.IsFalse(TutorialWinGuarantee.IsLethal(5, 4));
    }
}
