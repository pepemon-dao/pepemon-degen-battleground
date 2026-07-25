using System.Collections.Generic;
using Pepemon.Onboarding;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor helpers for re-testing the first-battle flow.
///
/// Tutorial progress is monotonic and stored in PlayerPrefs, so without a reset the
/// onboarding can only be observed once per machine - which is why regressions in it
/// went unnoticed.
/// </summary>
public static class OnboardingDevTools
{
    [MenuItem("Pepemon/Reset Onboarding", priority = 100)]
    public static void ResetOnboarding()
    {
        OnboardingState.Reset();
        Debug.Log("[onboarding] Reset. The tutorial and starter-pack claim will run again on next play.");
    }

    [MenuItem("Pepemon/Log Onboarding State", priority = 101)]
    public static void LogOnboardingState()
    {
        var seen = new List<int>();
        foreach (var beat in TutorialScript.AllBeats)
        {
            if (OnboardingState.HasSeenBeat(beat.Id)) seen.Add(beat.Id);
        }

        Debug.Log($"[onboarding] beatsSeen=[{string.Join(",", seen)}] " +
                  $"of {TutorialScript.BeatCount} " +
                  $"hasClaimedStarterPack={OnboardingState.HasClaimedStarterPack}");
    }

    [MenuItem("Pepemon/Mark Starter Pack As Claimed", priority = 102)]
    public static void MarkStarterPackClaimed()
    {
        OnboardingState.HasClaimedStarterPack = true;
        Debug.Log("[onboarding] Starter pack marked claimed - the bot battle will now behave as a replay.");
    }
}
