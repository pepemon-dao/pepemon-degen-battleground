using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ScreenLeaderboard : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] Button _refresh;
    [TitleGroup("Component References"), SerializeField] Button _previousScreenButton;
    [TitleGroup("Component References"), SerializeField] LeaderboardListLoader _rankingList;

    void Start()
    {
        _refresh.onClick.AddListener(ReloadDefaultLeaderboard);
        _previousScreenButton.onClick.AddListener(OnPreviousScreenButtonClick);
    }

    // TODO: Allow selecting different leaderboards
    public void ReloadDefaultLeaderboard()
    {
        _rankingList.ReloadLeaderboard(PepemonMatchmaker.PepemonLeagues.Rice);
    }

    void OnPreviousScreenButtonClick()
    {
        FindObjectOfType<MainMenuController>().ShowScreen(MainSceneScreensEnum.Menu);
    }
}
