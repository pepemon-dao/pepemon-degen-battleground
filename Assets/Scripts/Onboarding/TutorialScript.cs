using System.Collections.Generic;

namespace Pepemon.Onboarding
{
    /// <summary>How a tutorial beat interrupts the battle.</summary>
    public enum TutorialBeatMode
    {
        /// <summary>Freezes the battle and waits for the player to advance. Use sparingly.</summary>
        Modal,

        /// <summary>Shown alongside the battle and dismisses itself. Never blocks.</summary>
        Toast
    }

    public class TutorialBeat
    {
        public int Id;
        public string Text;
        public TutorialBeatMode Mode;

        /// <summary>Seconds a Toast stays up. Ignored for Modal beats.</summary>
        public float ToastSeconds;
    }

    /// <summary>
    /// The authored first-battle script and its cast.
    ///
    /// Lives in code rather than scene YAML so the copy is reviewable in a diff. The
    /// previous version was serialized into SandboxNew.unity, which is why it drifted
    /// out of sync with the battle rules it described.
    ///
    /// Every mechanical claim below is checked against GameController:
    ///  - win condition is HP reaching 0 (there is no round cap - LoopGame runs until
    ///    someone dies, so the old "each battle lasts 5 rounds" line was simply wrong)
    ///  - decks reshuffle on every 5th round (GameController._roundNumber % 5 == 0)
    ///  - cards drawn per round equals INT (Player.DrawNewHand)
    ///  - attack order is highest SPD first (GameController.ResolveAttacker)
    ///  - damage is attack minus defense, floored at 1 (GameController dmg calculation)
    /// </summary>
    public static class TutorialScript
    {
        /// <summary>The rival trainer's handle. The bot always fields a Mandraky.</summary>
        public const string RivalName = "PAPERHANDS";

        public const string RivalTaunt = "Nice deck. I'll be taking that starter pack.";
        public const string RivalDefeatLine = "Rugged by a rookie. GG.";

        public const string WinHeadline = "You beat " + RivalName + "!";
        public const string WinSubline = "Claim your starter pack";
        public const string LoseSubline = "Run it back";

        public const int BeatIntro = 1;
        public const int BeatDraw = 2;
        public const int BeatDamage = 3;
        public const int BeatComeback = 4;

        // There is deliberately no in-battle victory beat. The post-battle screen appears
        // immediately after the killing blow and already carries the payoff copy, and a modal
        // beat at that moment would hold the game at timeScale 0 while that screen tries to
        // play its show animation - which would stall it forever.

        /// <summary>Fraction of max HP below which the comeback beat fires.</summary>
        public const float ComebackHpThreshold = 0.4f;

        private static readonly Dictionary<int, TutorialBeat> Beats = new Dictionary<int, TutorialBeat>
        {
            [BeatIntro] = new TutorialBeat
            {
                Id = BeatIntro,
                Mode = TutorialBeatMode.Modal,
                Text = RivalName + " wants your starter pack.\n" +
                       "Drop their Mandraky to 0 HP and it's yours, Pepetrainer."
            },
            [BeatDraw] = new TutorialBeat
            {
                Id = BeatDraw,
                Mode = TutorialBeatMode.Toast,
                ToastSeconds = 4.5f,
                Text = "Your INT sets how many cards you draw each round.\n" +
                       "Highest SPD swings first."
            },
            [BeatDamage] = new TutorialBeat
            {
                Id = BeatDamage,
                Mode = TutorialBeatMode.Toast,
                ToastSeconds = 5f,
                Text = "Damage = your ATK plus offense cards, minus their defense.\n" +
                       "At least 1 always gets through."
            },
            [BeatComeback] = new TutorialBeat
            {
                Id = BeatComeback,
                Mode = TutorialBeatMode.Toast,
                ToastSeconds = 4f,
                Text = RivalName + " is connecting.\n" +
                       "Every Pepetrainer's first comeback starts about here."
            }
        };

        public static int BeatCount => Beats.Count;

        public static bool TryGetBeat(int id, out TutorialBeat beat) => Beats.TryGetValue(id, out beat);

        public static IEnumerable<TutorialBeat> AllBeats => Beats.Values;
    }
}
