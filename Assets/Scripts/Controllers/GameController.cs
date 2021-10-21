using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameController : MonoBehaviour
{
    [BoxGroup("Debug")]
    [BoxGroup("Debug/P1"), SerializeField] Pepemon _player1Pepemon;
    [BoxGroup("Debug/P1"), SerializeField] Deck _player1Deck;

    [BoxGroup("Debug/P2"), SerializeField] Pepemon _player2Pepemon;
    [BoxGroup("Debug/P2"), SerializeField] Deck _player2Deck;

    public List<Card> _p1Hand = new List<Card>();
    public List<Card> _p2Hand = new List<Card>();

    [Button()]
    void StartGame()
    {
        Debug.Log("X");
    }

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void DrawHands()
    {
        _player1Deck.ShuffelDeck();
        _player2Deck.ShuffelDeck();

        while (_p1Hand.Count < _player1Pepemon.Intelligence)
        {
            _p1Hand.Add(_player1Deck)
        }
    }

    // todo: think of a way to handle card managment that are only played  once? perhaps a temp card list cached here for example?


}
