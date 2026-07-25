using System.Collections.Generic;
using UnityEngine;

namespace Pepemon.Battle
{
    /// <summary>
    /// Arbiter for Time.timeScale.
    ///
    /// Three separate systems freeze the game (tutorial beats, card preview) or change its
    /// speed (battle fast-forward). When each wrote Time.timeScale directly the last writer
    /// won: closing a card preview while a tutorial beat was up unfroze the battle behind it,
    /// and leaving the battle scene mid-freeze left the menu scene stuck at timeScale 0.
    ///
    /// Freezes are reference-counted by holder key; speed only applies when nothing is
    /// freezing. Statics survive scene loads, so <see cref="ResetAll"/> must be called when
    /// entering a scene.
    /// </summary>
    public static class TimeControl
    {
        public const string HolderTutorial = "tutorial";
        public const string HolderCardPreview = "cardPreview";

        private static readonly HashSet<string> FreezeHolders = new HashSet<string>();
        private static float _speed = 1f;

        public static bool IsFrozen => FreezeHolders.Count > 0;
        public static float Speed => _speed;

        public static void Freeze(string holder)
        {
            if (string.IsNullOrEmpty(holder)) return;
            FreezeHolders.Add(holder);
            Apply();
        }

        public static void Unfreeze(string holder)
        {
            if (string.IsNullOrEmpty(holder)) return;
            FreezeHolders.Remove(holder);
            Apply();
        }

        /// <summary>Playback speed used whenever nothing is freezing the game.</summary>
        public static void SetSpeed(float speed)
        {
            _speed = Mathf.Clamp(speed, 0.25f, 4f);
            Apply();
        }

        /// <summary>
        /// Clears every freeze and restores normal speed. Call on scene entry so a scene
        /// change during a freeze can never leave the next scene stuck.
        /// </summary>
        public static void ResetAll()
        {
            FreezeHolders.Clear();
            _speed = 1f;
            Apply();
        }

        private static void Apply()
        {
            Time.timeScale = FreezeHolders.Count > 0 ? 0f : _speed;
        }
    }
}
