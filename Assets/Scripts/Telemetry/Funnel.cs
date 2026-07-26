using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Pepemon.Telemetry
{
    /// <summary>
    /// Destination for tracked events. Swap via <see cref="Funnel.SetBackend"/> once a
    /// real provider is chosen; nothing at the call sites changes.
    /// </summary>
    public interface ITelemetryBackend
    {
        void Track(string eventName, IReadOnlyDictionary<string, object> properties);
    }

    /// <summary>Default backend. Writes to the Unity console so the funnel is visible in dev builds.</summary>
    public class DebugLogTelemetryBackend : ITelemetryBackend
    {
        public void Track(string eventName, IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                Debug.Log($"[telemetry] {eventName}");
                return;
            }

            var sb = new StringBuilder();
            sb.Append("[telemetry] ").Append(eventName).Append(" {");
            var first = true;
            foreach (var kvp in properties)
            {
                if (!first) sb.Append(", ");
                sb.Append(kvp.Key).Append('=').Append(kvp.Value ?? "null");
                first = false;
            }
            sb.Append('}');
            Debug.Log(sb.ToString());
        }
    }

    /// <summary>Discards everything. Use to disable telemetry without touching call sites.</summary>
    public class NullTelemetryBackend : ITelemetryBackend
    {
        public void Track(string eventName, IReadOnlyDictionary<string, object> properties) { }
    }

    /// <summary>
    /// Thin funnel-instrumentation facade.
    ///
    /// Named Funnel rather than Analytics or Telemetry so the type can never collide with the
    /// UnityEngine.Analytics namespace, nor with its own Pepemon.Telemetry namespace - a class
    /// sharing its namespace's name becomes ambiguous the moment anyone adds `using Pepemon;`.
    ///
    /// Tracking must never affect gameplay, so every call is swallowed on failure.
    /// </summary>
    public static class Funnel
    {
        #region Event names
        public const string AppLoaded = "app_loaded";
        public const string StartPressed = "start_pressed";
        public const string PepemonSelected = "pepemon_selected";
        public const string FirstBattleStarted = "first_battle_started";
        public const string TutorialBeatShown = "tutorial_beat_shown";
        public const string TutorialBeatDismissed = "tutorial_beat_dismissed";
        public const string FirstBattleEnded = "first_battle_ended";
        public const string ClaimClicked = "claim_clicked";
        public const string WalletConnectRequested = "wallet_connect_requested";
        public const string WalletConnectResult = "wallet_connect_result";
        public const string MintResult = "mint_result";
        public const string ReturnedToMenu = "returned_to_menu";
        #endregion

        private static ITelemetryBackend _backend = new DebugLogTelemetryBackend();

        public static void SetBackend(ITelemetryBackend backend)
        {
            _backend = backend ?? new NullTelemetryBackend();
        }

        public static void Track(string eventName)
        {
            Track(eventName, null);
        }

        public static void Track(string eventName, IReadOnlyDictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            try
            {
                _backend?.Track(eventName, properties);
            }
            catch (Exception e)
            {
                // Never let instrumentation break the game.
                Debug.LogWarning($"[telemetry] dropped '{eventName}': {e.Message}");
            }
        }

        /// <summary>Convenience for the common one-to-three property case.</summary>
        public static void Track(string eventName, string key, object value)
        {
            Track(eventName, new Dictionary<string, object> { [key] = value });
        }

        public static void Track(string eventName, string k1, object v1, string k2, object v2)
        {
            Track(eventName, new Dictionary<string, object> { [k1] = v1, [k2] = v2 });
        }

        public static void Track(string eventName, string k1, object v1, string k2, object v2, string k3, object v3)
        {
            Track(eventName, new Dictionary<string, object> { [k1] = v1, [k2] = v2, [k3] = v3 });
        }
    }
}
