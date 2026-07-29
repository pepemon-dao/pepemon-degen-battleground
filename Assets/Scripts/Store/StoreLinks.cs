namespace Pepemon.Store
{
    /// <summary>
    /// Decides whether an outbound link should be replaced by the in-game store.
    ///
    /// This is not only a convenience. The web shop's booster packs mint onto a different cards
    /// contract (0xae0b8933...) from the one the game reads (0x888c8302...), so a player who
    /// followed those links and bought a pack would receive cards the game can never show them.
    /// Sending them to the in-game store instead is the difference between a sale and a
    /// support ticket.
    /// </summary>
    public static class StoreLinks
    {
        public static bool IsStoreLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            var lower = url.ToLowerInvariant();
            if (lower.IndexOf("pepemon", System.StringComparison.Ordinal) < 0) return false;

            return lower.Contains("/store") || lower.Contains("boosterpack");
        }
    }
}
