using UnityEngine;

namespace Pepemon.Onboarding
{
    /// <summary>
    /// Single source of truth for first-battle onboarding progress.
    ///
    /// State is per-device (PlayerPrefs), not per-wallet: a new wallet in the same browser
    /// keeps this progress, and the same wallet on another device starts over. Moving this
    /// to a wallet-scoped record requires a backend and is out of scope here.
    /// </summary>
    public static class OnboardingState
    {
        /// <summary>
        /// Bitmask of tutorial beats already seen, one bit per beat id.
        ///
        /// A bitmask rather than a high-water mark because beats are triggered by gameplay
        /// events, not in a fixed order: the comeback beat fires on an HP threshold and could
        /// otherwise arrive before the damage beat and suppress it permanently.
        /// </summary>
        public const string TutorialBeatsKey = "TUTORIAL_BEATS_SEEN";

        /// <summary>
        /// Progress counter written by the previous tutorial. Deliberately not migrated - the
        /// script was rewritten, so the old value describes beats that no longer exist. Cleared
        /// on reset so no stale key is left behind.
        /// </summary>
        public const string LegacyTutorialStateKey = "TUTORIAL_STATE_INDEX";

        public const string StarterPackKey = "GotStarterPack";

        /// <summary>
        /// The Pepemon and starter deck the player chose before their first battle.
        ///
        /// Persisted at selection time rather than read from Web3Controller at claim time:
        /// the post-battle screen zeroes StarterPepemonID/StarterDeckID before loading the
        /// menu scene, so by the time the claim runs the choice is already gone.
        /// </summary>
        public const string PendingPepemonKey = "PENDING_STARTER_PEPEMON";
        public const string PendingDeckKey = "PENDING_STARTER_DECK";

        public static int PendingStarterPepemonId
        {
            get => PlayerPrefs.GetInt(PendingPepemonKey, 0);
            set
            {
                PlayerPrefs.SetInt(PendingPepemonKey, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Starter deck id (10001 / 10002). 0 when the player has not chosen yet.</summary>
        public static int PendingStarterDeckId
        {
            get => PlayerPrefs.GetInt(PendingDeckKey, 0);
            set
            {
                PlayerPrefs.SetInt(PendingDeckKey, value);
                PlayerPrefs.Save();
            }
        }

        public static int TutorialBeatsSeen
        {
            get => PlayerPrefs.GetInt(TutorialBeatsKey, 0);
            set
            {
                PlayerPrefs.SetInt(TutorialBeatsKey, value);
                PlayerPrefs.Save();
            }
        }

        public static bool HasSeenBeat(int beatId)
        {
            if (beatId < 0 || beatId > 30) return false;
            return (TutorialBeatsSeen & (1 << beatId)) != 0;
        }

        public static void MarkBeatSeen(int beatId)
        {
            if (beatId < 0 || beatId > 30) return;
            TutorialBeatsSeen |= 1 << beatId;
        }

        /// <summary>
        /// True only once the starter pack has actually been minted on-chain.
        /// Never set this optimistically - a failed claim must stay claimable.
        /// </summary>
        public static bool HasClaimedStarterPack
        {
            get => PlayerPrefs.GetInt(StarterPackKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(StarterPackKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(TutorialBeatsKey);
            PlayerPrefs.DeleteKey(LegacyTutorialStateKey);
            PlayerPrefs.DeleteKey(StarterPackKey);
            PlayerPrefs.DeleteKey(PendingPepemonKey);
            PlayerPrefs.DeleteKey(PendingDeckKey);
            PlayerPrefs.Save();
        }
    }
}
