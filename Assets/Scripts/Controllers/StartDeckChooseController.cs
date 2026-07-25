using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pepemon.Battle;
using Pepemon.Telemetry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The starter-selection screen.
///
/// This is the only real decision a player makes before an automatic battle, so it carries all
/// of the pre-battle agency. It previously showed a name and a sentence and no stats at all -
/// while the very next screen lectured the player about ATK, DEF, SPD and INT.
///
/// The buttons in StartScreen.unity still drive this through SetName/SetDesc. When
/// <see cref="cardsData"/> is wired, the name is used to look up the Pepemon and the stat block
/// and authored copy come from the ScriptableObject instead of scene-embedded strings.
/// </summary>
public class StartDeckChooseController : MonoBehaviour
{
    [SerializeField] private Image currentPepemonImg;
    [SerializeField] private TMP_Text pepemonName;
    [SerializeField] private TMP_Text descDisplay;
    [SerializeField] private Button defaultSetting;

    [Header("Stat block - optional, null-safe until wired")]
    [SerializeField] private DataContainer cardsData;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text defText;
    [SerializeField] private TMP_Text spdText;
    [SerializeField] private TMP_Text intText;
    [SerializeField] private TMP_Text taglineText;

    private string _selectedName;

    private void Start()
    {
        // Load-bearing: this is what sets Web3Controller.StarterPepemonID, so the battle scene
        // has a Pepemon even if the player presses START without touching either option.
        if (defaultSetting != null) defaultSetting.onClick.Invoke();
    }

    public void SetImg(Sprite sprite)
    {
        if (currentPepemonImg != null) currentPepemonImg.sprite = sprite;
    }

    public void SetName(string name)
    {
        _selectedName = name;

        if (pepemonName != null) pepemonName.text = name;

        Funnel.Track(Funnel.PepemonSelected, "name", name);

        ShowStatsFor(name);
    }

    public void SetDesc(string desc)
    {
        // Authored copy on the ScriptableObject wins over the string embedded in the scene.
        var pepemon = FindPepemonByName(_selectedName);
        if (pepemon != null && !string.IsNullOrEmpty(pepemon.Description))
        {
            desc = pepemon.Description;
        }

        if (descDisplay != null) descDisplay.text = desc;
    }

    private void ShowStatsFor(string name)
    {
        var pepemon = FindPepemonByName(name);
        if (pepemon == null) return;

        SetStat(hpText, pepemon.HealthPoints);
        SetStat(atkText, pepemon.Attack);
        SetStat(defText, pepemon.Defense);
        SetStat(spdText, pepemon.Speed);
        SetStat(intText, pepemon.Intelligence);

        if (taglineText != null) taglineText.text = pepemon.Tagline;
    }

    private BattleCard FindPepemonByName(string name)
    {
        if (cardsData == null || cardsData.Pepemons == null || string.IsNullOrEmpty(name)) return null;

        return cardsData.Pepemons.FirstOrDefault(
            p => p != null && string.Equals(p.DisplayName, name, System.StringComparison.OrdinalIgnoreCase));
    }

    private static void SetStat(TMP_Text label, int value)
    {
        if (label != null) label.text = value.ToString();
    }
}
