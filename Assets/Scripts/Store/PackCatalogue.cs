using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Pepemon.Store
{
    /// <summary>Shop copy for one pack tier. Prices and sizes come from the contract, not here.</summary>
    public class PackTierInfo
    {
        public byte Id;
        public string Name;
        public string Tagline;
        public string Contents;
    }

    /// <summary>
    /// Names, taglines and rarity breakdowns for the store.
    ///
    /// Only the descriptive half lives here. Price and pack size are always read from the
    /// contract, because those two decide whether the transaction succeeds: the contract
    /// rejects both underpayment and overpayment, so a stale price in the client would mean
    /// every purchase reverts.
    /// </summary>
    public static class PackCatalogue
    {
        public static readonly IReadOnlyList<PackTierInfo> Tiers = new List<PackTierInfo>
        {
            new PackTierInfo
            {
                Id = 1,
                Name = "STARTER PACK",
                Tagline = "Cheap way to round out a deck.",
                Contents = "4 Common + 1 Rare",
            },
            new PackTierInfo
            {
                Id = 2,
                Name = "TRAINER PACK",
                Tagline = "A new Pepemon and a guaranteed Epic.",
                Contents = "1 Pepemon + 5 Common + 3 Rare + 1 Epic",
            },
            new PackTierInfo
            {
                Id = 3,
                Name = "DEGEN PACK",
                Tagline = "Two Pepemon. Three Epics. Send it.",
                Contents = "2 Pepemon + 8 Common + 7 Rare + 3 Epic",
            },
        };
    }

    /// <summary>Formats wei for display. Kept engine-free so it can be tested in EditMode.</summary>
    public static class EthDisplay
    {
        private static readonly BigInteger WeiPerEth = BigInteger.Pow(10, 18);

        /// <summary>
        /// Renders wei as a short ETH string.
        ///
        /// Done with integer maths rather than casting wei to decimal: a decimal cannot hold
        /// 10^18 wei for larger balances, and the prices here are small enough that a naive
        /// double conversion would render as scientific notation.
        /// </summary>
        public static string FormatEth(BigInteger wei, int decimals = 6)
        {
            if (wei <= BigInteger.Zero) return "FREE";

            var whole = BigInteger.Divide(wei, WeiPerEth);
            var remainder = wei - whole * WeiPerEth;

            // Take the leading `decimals` digits of the fractional part.
            var scale = BigInteger.Pow(10, 18 - decimals);
            var fraction = BigInteger.Divide(remainder, scale);

            var text = whole.ToString(CultureInfo.InvariantCulture);
            if (fraction > BigInteger.Zero)
            {
                var digits = fraction.ToString(CultureInfo.InvariantCulture).PadLeft(decimals, '0').TrimEnd('0');
                if (digits.Length > 0) text += "." + digits;
            }

            return text + " ETH";
        }
    }
}
