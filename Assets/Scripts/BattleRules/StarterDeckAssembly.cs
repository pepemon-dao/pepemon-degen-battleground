using System.Collections.Generic;

namespace Pepemon.BattleRules
{
    /// <summary>
    /// Decides which support cards go into a freshly claimed starter deck.
    ///
    /// Pure and engine-free so it can be tested. The rules matter because the inputs are not
    /// trustworthy: the faucet's granted token ids are not knowable ahead of time, the local
    /// starter list may name cards the player does not own, and the contract enforces a cap
    /// that would revert the whole assembly transaction if exceeded.
    /// </summary>
    public static class StarterDeckAssembly
    {
        /// <summary>
        /// Maps the desired starter list to per-card amounts, clamped by ownership and by the
        /// per-deck cap.
        ///
        /// Repeats in <paramref name="desiredIds"/> are counted, so a starter list naming the
        /// same card three times asks for three copies - but never more than the player owns.
        /// </summary>
        /// <param name="desiredIds">Starter list, may contain duplicates.</param>
        /// <param name="owned">Card id to owned quantity.</param>
        /// <param name="maxSupportCards">Contract cap on total support cards per deck.</param>
        /// <returns>Card id to amount to add. Empty when nothing can be added.</returns>
        public static Dictionary<ulong, int> SelectSupportCards(
            IReadOnlyList<ulong> desiredIds,
            IReadOnlyDictionary<ulong, int> owned,
            int maxSupportCards)
        {
            var selected = new Dictionary<ulong, int>();

            if (desiredIds == null || owned == null || maxSupportCards <= 0) return selected;

            // Preserve the starter list's order so a truncated deck keeps its intended core
            // rather than an arbitrary hash ordering.
            var wanted = new List<ulong>();
            var counts = new Dictionary<ulong, int>();

            foreach (var id in desiredIds)
            {
                if (counts.ContainsKey(id))
                {
                    counts[id]++;
                }
                else
                {
                    counts[id] = 1;
                    wanted.Add(id);
                }
            }

            var budget = maxSupportCards;

            foreach (var id in wanted)
            {
                if (budget <= 0) break;

                if (!owned.TryGetValue(id, out var ownedAmount) || ownedAmount <= 0) continue;

                var amount = counts[id];
                if (amount > ownedAmount) amount = ownedAmount;
                if (amount > budget) amount = budget;
                if (amount <= 0) continue;

                selected[id] = amount;
                budget -= amount;
            }

            return selected;
        }

        /// <summary>Total cards a selection will add. Never exceeds the cap passed in.</summary>
        public static int TotalCards(IReadOnlyDictionary<ulong, int> selection)
        {
            var total = 0;
            if (selection == null) return total;

            foreach (var kvp in selection) total += kvp.Value;
            return total;
        }
    }
}
