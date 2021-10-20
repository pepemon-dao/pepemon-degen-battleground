using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameController : MonoBehaviour
{
    [BoxGroup("Debug"), SerializeField] Deck _player1Deck;
    [BoxGroup("Debug"), SerializeField] Deck _player2Deck;

    [Button()]
    void StartGame()
    {
        Debug.Log("X");
    }
}
