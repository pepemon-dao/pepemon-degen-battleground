using UnityEngine;
using Sirenix.OdinInspector;

namespace Pepemon.Battle
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/pepemon", order = 1)]
    public class BattleCard : ScriptableObject
    {
        [ReadOnly, ShowInInspector] public string UID = System.Guid.NewGuid().ToString();

        [HideLabel, PreviewField(55), AssetsOnly]
        [HorizontalGroup("General/Left", 55)]
        public Sprite DisplayIcon;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public string ID;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(120)]
        public string DisplayName;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(200)]
        public Sprite CardContentBackdrop;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(200)]
        public Sprite CardContent;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(200)]
        public Sprite CardBG;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public string Level = "1";

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public PepemonType Type;


        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public PepemonType Weakness;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public PepemonType Resistence;

        [BoxGroup("General")]
        [VerticalGroup("General/Left/Right")]
        [LabelWidth(80)]
        public CardRarity Rarity;

        /// <summary>
        /// One-line playstyle hook shown on the starter-selection screen.
        ///
        /// This copy previously lived as a UnityEvent string argument inside StartScreen.unity,
        /// where it could not be reviewed or edited without opening the scene.
        /// </summary>
        [BoxGroup("Flavour")]
        [LabelWidth(80)]
        public string Tagline;

        [BoxGroup("Flavour")]
        [LabelWidth(80), TextArea(2, 4)]
        public string Description;

        [TitleGroup("Properties")] public int HealthPoints = 400;
        [TitleGroup("Properties")] public int Speed;
        [TitleGroup("Properties")] public int Intelligence;
        [TitleGroup("Properties")] public int Attack;
        [TitleGroup("Properties")] public int Defense;
        [TitleGroup("Properties")] public int SAttack;
        [TitleGroup("Properties")] public int SDeffense;

    }
}
