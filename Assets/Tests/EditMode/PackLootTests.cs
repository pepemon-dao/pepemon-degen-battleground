using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Pepemon.Store;

/// <summary>
/// Covers the booster store logic that can be checked without a chain or a scene.
///
/// The reveal is the payoff moment of the whole store, and the price is what decides whether a
/// purchase succeeds at all - the contract rejects underpayment and overpayment alike. Neither
/// is worth discovering is wrong from a 35-minute cloud build.
/// </summary>
public class PackLootTests
{
    #region rarity

    [Test]
    public void BattleCardIdsAreRecognisedAsPepemon()
    {
        for (ulong id = 1; id <= 10; id++)
        {
            Assert.AreEqual(CardRarity.Pepemon, PackLoot.RarityOf(id), $"card {id}");
        }
    }

    [Test]
    public void RarityMatchesTheContractPools()
    {
        Assert.AreEqual(CardRarity.Common, PackLoot.RarityOf(11));
        Assert.AreEqual(CardRarity.Common, PackLoot.RarityOf(29));
        Assert.AreEqual(CardRarity.Rare, PackLoot.RarityOf(18));
        Assert.AreEqual(CardRarity.Rare, PackLoot.RarityOf(48));
        Assert.AreEqual(CardRarity.Epic, PackLoot.RarityOf(23));
        Assert.AreEqual(CardRarity.Epic, PackLoot.RarityOf(49));
    }

    [Test]
    public void UnknownCardsFallBackToCommonRatherThanThrowing()
    {
        // A card added to the factory later must never stop a pack being revealed.
        Assert.AreEqual(CardRarity.Common, PackLoot.RarityOf(500));
    }

    #endregion

    #region gained

    [Test]
    public void GainedReturnsCardsWhoseBalanceWentUp()
    {
        var before = new Dictionary<ulong, int> { { 5, 1 } };
        var after = new Dictionary<ulong, int> { { 5, 1 }, { 12, 1 }, { 23, 1 } };

        CollectionAssert.AreEqual(new List<ulong> { 12, 23 }, PackLoot.Gained(before, after));
    }

    [Test]
    public void GainedReportsOneEntryPerExtraCopy()
    {
        var before = new Dictionary<ulong, int> { { 12, 1 } };
        var after = new Dictionary<ulong, int> { { 12, 4 } };

        CollectionAssert.AreEqual(new List<ulong> { 12, 12, 12 }, PackLoot.Gained(before, after));
    }

    [Test]
    public void GainedIgnoresCardsTheWalletAlreadyHad()
    {
        var before = new Dictionary<ulong, int> { { 3, 2 }, { 12, 1 } };
        var after = new Dictionary<ulong, int> { { 3, 2 }, { 12, 1 } };

        CollectionAssert.IsEmpty(PackLoot.Gained(before, after));
    }

    [Test]
    public void GainedTreatsAnEmptyStartingWalletAsAllNew()
    {
        // GetOwnedCards omits zero balances, so a fresh wallet's snapshot is empty rather than
        // full of zeroes. Every card in the pack must still be reported.
        var after = new Dictionary<ulong, int> { { 1, 1 }, { 12, 1 } };

        CollectionAssert.AreEqual(new List<ulong> { 1, 12 }, PackLoot.Gained(new Dictionary<ulong, int>(), after));
    }

    [Test]
    public void GainedSurvivesNullSnapshots()
    {
        CollectionAssert.IsEmpty(PackLoot.Gained(null, null));
        CollectionAssert.AreEqual(new List<ulong> { 7 },
            PackLoot.Gained(null, new Dictionary<ulong, int> { { 7, 1 } }));
    }

    [Test]
    public void GainedIgnoresBalancesThatSomehowWentDown()
    {
        // Should not happen, but a negative difference must not produce phantom entries.
        var before = new Dictionary<ulong, int> { { 12, 3 } };
        var after = new Dictionary<ulong, int> { { 12, 1 } };

        CollectionAssert.IsEmpty(PackLoot.Gained(before, after));
    }

    #endregion

    #region summary

    [Test]
    public void SummaryLeadsWithTheRarestCards()
    {
        var cards = new List<ulong> { 11, 12, 18, 23, 1 };
        Assert.AreEqual("1 EPIC  1 PEPEMON  1 RARE  2 COMMON", PackLoot.Summarise(cards));
    }

    [Test]
    public void SummaryOmitsRaritiesThatArePresentZeroTimes()
    {
        Assert.AreEqual("2 COMMON", PackLoot.Summarise(new List<ulong> { 11, 12 }));
    }

    [Test]
    public void SummaryOfNothingIsEmpty()
    {
        Assert.AreEqual(string.Empty, PackLoot.Summarise(new List<ulong>()));
        Assert.AreEqual(string.Empty, PackLoot.Summarise(null));
    }

    #endregion

    #region price formatting

    [Test]
    public void PricesRenderAsPlainDecimalsNotScientificNotation()
    {
        // 0.0001 ETH is the cheapest tier. Naive double formatting renders this as 1E-04.
        Assert.AreEqual("0.0001 ETH", EthDisplay.FormatEth(BigInteger.Parse("100000000000000")));
        Assert.AreEqual("0.0003 ETH", EthDisplay.FormatEth(BigInteger.Parse("300000000000000")));
        Assert.AreEqual("0.001 ETH", EthDisplay.FormatEth(BigInteger.Parse("1000000000000000")));
    }

    [Test]
    public void WholeAndMixedAmountsRenderCorrectly()
    {
        Assert.AreEqual("1 ETH", EthDisplay.FormatEth(BigInteger.Pow(10, 18)));
        Assert.AreEqual("1.5 ETH", EthDisplay.FormatEth(BigInteger.Parse("1500000000000000000")));
        Assert.AreEqual("12.25 ETH", EthDisplay.FormatEth(BigInteger.Parse("12250000000000000000")));
    }

    [Test]
    public void ZeroReadsAsFreeRatherThanZeroEth()
    {
        Assert.AreEqual("FREE", EthDisplay.FormatEth(BigInteger.Zero));
    }

    [Test]
    public void AmountsTooSmallToShowDoNotRenderAsABareDecimalPoint()
    {
        // One wei rounds below six decimals; it must still read as a number.
        Assert.AreEqual("0 ETH", EthDisplay.FormatEth(BigInteger.One));
    }

    [Test]
    public void LargeBalancesDoNotOverflow()
    {
        // A decimal cannot hold this; the formatter uses integer maths precisely so it can.
        Assert.AreEqual("1000000 ETH", EthDisplay.FormatEth(BigInteger.Pow(10, 24)));
    }

    #endregion

    #region store links

    [Test]
    public void WebShopBoosterLinksAreRedirectedInGame()
    {
        // These links would otherwise sell the player cards on a different contract from the
        // one the game reads, so they would pay and receive nothing the game can show.
        Assert.IsTrue(StoreLinks.IsStoreLink("https://pepemon.world/store/boosterpacks"));
        Assert.IsTrue(StoreLinks.IsStoreLink("https://pepemon.world/store"));
        Assert.IsTrue(StoreLinks.IsStoreLink("HTTPS://PEPEMON.WORLD/Store/BoosterPacks"));
    }

    [Test]
    public void OtherLinksStillOpenInABrowser()
    {
        Assert.IsFalse(StoreLinks.IsStoreLink("https://pepemon.world"));
        Assert.IsFalse(StoreLinks.IsStoreLink("https://docs.pepemon.world/gaming/gaming"));
        Assert.IsFalse(StoreLinks.IsStoreLink("https://twitter.com/pepemon"));
    }

    [Test]
    public void AnUnrelatedStoreLinkIsNotHijacked()
    {
        // Only Pepemon's own shop is intercepted; a link to anyone else's store is left alone.
        Assert.IsFalse(StoreLinks.IsStoreLink("https://opensea.io/store/boosterpacks"));
    }

    [Test]
    public void EmptyLinksAreHarmless()
    {
        Assert.IsFalse(StoreLinks.IsStoreLink(null));
        Assert.IsFalse(StoreLinks.IsStoreLink(""));
        Assert.IsFalse(StoreLinks.IsStoreLink("   "));
    }

    #endregion

    #region catalogue

    [Test]
    public void EveryCatalogueTierHasCopyAndAUniqueId()
    {
        var seen = new HashSet<byte>();
        foreach (var tier in PackCatalogue.Tiers)
        {
            Assert.IsTrue(seen.Add(tier.Id), $"duplicate tier id {tier.Id}");
            Assert.IsNotEmpty(tier.Name, $"tier {tier.Id} has no name");
            Assert.IsNotEmpty(tier.Tagline, $"tier {tier.Id} has no tagline");
            Assert.IsNotEmpty(tier.Contents, $"tier {tier.Id} has no contents");
        }
    }

    [Test]
    public void CatalogueIdsMatchTheTiersConfiguredOnChain()
    {
        // The deploy script configures tiers 1, 2 and 3. A tier in the catalogue with no
        // matching on-chain tier would silently never appear in the store.
        CollectionAssert.AreEquivalent(new byte[] { 1, 2, 3 },
            new List<byte>(System.Linq.Enumerable.Select(PackCatalogue.Tiers, t => t.Id)));
    }

    #endregion
}
