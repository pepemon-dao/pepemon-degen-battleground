using System.Collections.Generic;
using System.Linq;

namespace Pepemon.Store
{
    public enum CardRarity
    {
        Pepemon,
        Common,
        Rare,
        Epic,
    }

    /// <summary>
    /// Works out what a player actually received from a pack.
    ///
    /// Deliberately free of Unity and chain types so it can be tested in EditMode. The reveal is
    /// the payoff moment of the whole store, and it is not worth finding out it is wrong from a
    /// 35-minute cloud build.
    ///
    /// Named PackLoot rather than the more obvious PackRewards because the vendored Thirdweb SDK
    /// already exposes a Thirdweb.PackRewards, and any file with both `using Thirdweb;` and
    /// `using Pepemon.Store;` then fails to compile on an ambiguous reference.
    /// </summary>
    public static class PackLoot
    {
        // Rarity per card id, as recorded in the contract repo's deploy/cards.ts. Ids 1-10 are
        // the Pepemon battle cards; 11-49 are support cards.
        private static readonly HashSet<ulong> PepemonIds = new HashSet<ulong> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private static readonly HashSet<ulong> CommonIds =
            new HashSet<ulong> { 11, 12, 13, 14, 15, 16, 17, 27, 28, 29 };

        private static readonly HashSet<ulong> RareIds = new HashSet<ulong>
            { 18, 19, 20, 21, 22, 25, 30, 31, 32, 33, 34, 36, 37, 40, 41, 42, 43, 44, 46, 47, 48 };

        private static readonly HashSet<ulong> EpicIds = new HashSet<ulong> { 23, 24, 26, 35, 38, 39, 45, 49 };

        public static CardRarity RarityOf(ulong cardId)
        {
            if (PepemonIds.Contains(cardId)) return CardRarity.Pepemon;
            if (EpicIds.Contains(cardId)) return CardRarity.Epic;
            if (RareIds.Contains(cardId)) return CardRarity.Rare;
            // Anything added to the factory later reads as Common rather than throwing. A new
            // card should never stop a pack from being revealed.
            return CardRarity.Common;
        }

        /// <summary>
        /// The cards gained between two balance snapshots, one entry per copy received.
        ///
        /// Balances are diffed rather than the PackOpened event being decoded: the game already
        /// reads balances everywhere, whereas event decoding differs between the WebGL bridge
        /// and native and would need its own code path on each.
        /// </summary>
        public static List<ulong> Gained(IDictionary<ulong, int> before, IDictionary<ulong, int> after)
        {
            var gained = new List<ulong>();
            if (after == null) return gained;

            foreach (var entry in after)
            {
                var had = 0;
                if (before != null) before.TryGetValue(entry.Key, out had);

                for (var i = 0; i < entry.Value - had; i++) gained.Add(entry.Key);
            }

            gained.Sort();
            return gained;
        }

        /// <summary>Counts per rarity, richest first, for the reveal summary line.</summary>
        public static string Summarise(IEnumerable<ulong> cardIds)
        {
            if (cardIds == null) return string.Empty;

            var byRarity = cardIds.GroupBy(RarityOf).ToDictionary(g => g.Key, g => g.Count());
            var parts = new List<string>();

            foreach (var rarity in new[] { CardRarity.Epic, CardRarity.Pepemon, CardRarity.Rare, CardRarity.Common })
            {
                if (byRarity.TryGetValue(rarity, out var count) && count > 0)
                {
                    parts.Add($"{count} {rarity.ToString().ToUpperInvariant()}");
                }
            }

            return string.Join("  ", parts);
        }
    }
}
