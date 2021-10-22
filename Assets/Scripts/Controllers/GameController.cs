using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameController : MonoBehaviour
{
    [BoxGroup("Debug"), SerializeField] Player _player1;
    [BoxGroup("Debug"), SerializeField] Player _player2;

    [Header("temp public")]
    public Deck _p1Hand;
    public Deck _p2Hand;

    [Button()]
    void StartGame()
    {
        DrawHands();
    }

    [Button()]
    void Reset()
    {
        _p1Hand = null;
        _p2Hand = null;
    }

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void DrawHands()
    {
        _p1Hand = _player1.PlayerDeck;
        _p2Hand = _player2.PlayerDeck;

        _p1Hand.ShuffelDeck(1);
        _p2Hand.ShuffelDeck(2);
        // while (_p1Hand.Count < _player1Pepemon.Intelligence)
        // {
        //     _p1Hand.Add(_player1Deck)
        // }
    }

    // todo: think of a way to handle card managment that are only played  once? perhaps a temp card list cached here for example?


}
