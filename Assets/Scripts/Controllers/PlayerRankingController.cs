using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the instance of a player ranking element
/// </summary>
public class PlayerRankingController : MonoBehaviour, IPointerClickHandler
{
    [BoxGroup("Display Elements"), SerializeField] private TMP_Text _playerAddress;
    [BoxGroup("Display Elements"), SerializeField] private TMP_Text _playerRanking;

    public void SetInfo(string playerAddress, string playerRanking)
    {
        _playerAddress.text = playerAddress;
        _playerRanking.text = playerRanking;
    }

    // implementing IPointerClickHandler to avoid this problem:
    // https://answers.unity.com/questions/902929/scroll-not-working-when-elements-inside-have-click.html
    public void OnPointerClick(PointerEventData eventData)
    {
        var url = Web3Controller.instance.GetChainConfig().blockExplorerUrl + _playerAddress.text;
        Debug.Log($"Opening {url}");
        Application.OpenURL(url);
    }
}
