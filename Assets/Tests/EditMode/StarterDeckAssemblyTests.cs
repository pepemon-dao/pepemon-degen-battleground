using System.Collections.Generic;
using NUnit.Framework;
using Pepemon.BattleRules;

/// <summary>
/// The claim assembles a deck from cards a faucet grants, and the granted token ids are not
/// knowable ahead of time. These cover the cases where the starter list and reality disagree -
/// getting any of them wrong either reverts the assembly transaction or hands the player a
/// deck they cannot play.
/// </summary>
public class StarterDeckAssemblyTests
{
    private static Dictionary<ulong, int> Owned(params (ulong id, int amount)[] entries)
    {
        var d = new Dictionary<ulong, int>();
        foreach (var (id, amount) in entries) d[id] = amount;
        return d;
    }

    [Test]
    public void SelectsOnlyCardsThePlayerActuallyOwns()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 10, 20, 30 },
            Owned((10, 1), (30, 1)),
            maxSupportCards: 60);

        Assert.AreEqual(2, selection.Count);
        Assert.IsTrue(selection.ContainsKey(10));
        Assert.IsFalse(selection.ContainsKey(20), "card 20 is not owned and must be skipped");
        Assert.IsTrue(selection.ContainsKey(30));
    }

    [Test]
    public void CountsDuplicatesInTheStarterList()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 10, 10, 10 },
            Owned((10, 5)),
            maxSupportCards: 60);

        Assert.AreEqual(3, selection[10]);
    }

    [Test]
    public void NeverAsksForMoreCopiesThanAreOwned()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 10, 10, 10 },
            Owned((10, 2)),
            maxSupportCards: 60);

        Assert.AreEqual(2, selection[10], "asking for 3 while owning 2 would revert the transaction");
    }

    [Test]
    public void RespectsTheContractCap()
    {
        var desired = new List<ulong>();
        for (ulong i = 0; i < 20; i++) desired.Add(i);

        var owned = new Dictionary<ulong, int>();
        for (ulong i = 0; i < 20; i++) owned[i] = 1;

        var selection = StarterDeckAssembly.SelectSupportCards(desired, owned, maxSupportCards: 5);

        Assert.AreEqual(5, StarterDeckAssembly.TotalCards(selection));
    }

    [Test]
    public void CapTruncatesFromTheEndSoTheDeckKeepsItsCore()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 1, 2, 3, 4 },
            Owned((1, 1), (2, 1), (3, 1), (4, 1)),
            maxSupportCards: 2);

        Assert.IsTrue(selection.ContainsKey(1));
        Assert.IsTrue(selection.ContainsKey(2));
        Assert.IsFalse(selection.ContainsKey(3));
        Assert.IsFalse(selection.ContainsKey(4));
    }

    [Test]
    public void PartialCopiesFitIntoARemainingBudget()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 1, 2, 2, 2 },
            Owned((1, 1), (2, 3)),
            maxSupportCards: 3);

        Assert.AreEqual(1, selection[1]);
        Assert.AreEqual(2, selection[2], "only 2 of the budget remained after card 1");
        Assert.AreEqual(3, StarterDeckAssembly.TotalCards(selection));
    }

    [Test]
    public void OwningNothingProducesNoTransaction()
    {
        var selection = StarterDeckAssembly.SelectSupportCards(
            new List<ulong> { 10, 20 },
            Owned(),
            maxSupportCards: 60);

        Assert.IsEmpty(selection, "an empty selection must not be sent as an addSupportCards call");
    }

    [Test]
    public void HandlesNullAndZeroCapWithoutThrowing()
    {
        Assert.IsEmpty(StarterDeckAssembly.SelectSupportCards(null, Owned((1, 1)), 60));
        Assert.IsEmpty(StarterDeckAssembly.SelectSupportCards(new List<ulong> { 1 }, null, 60));
        Assert.IsEmpty(StarterDeckAssembly.SelectSupportCards(new List<ulong> { 1 }, Owned((1, 1)), 0));
    }

    [Test]
    public void TotalCards_HandlesNull()
    {
        Assert.AreEqual(0, StarterDeckAssembly.TotalCards(null));
    }
}
