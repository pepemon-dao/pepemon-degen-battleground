using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameController : MonoBehaviour
{
    [BoxGroup("Debug"), SerializeField] Player _player1;
    [BoxGroup("Debug"), SerializeField] Player _player2;

    [Header("temp public")]
    public Deck _p1Deck = new Deck();
    public Deck _p2Deck = new Deck();

    public List<Card> _p1Hand = new List<Card>();
    public List<Card> _p2Hand = new List<Card>();


    [Button()]
    void StartGame()
    {
        DrawHands();
    }

    [Button()]
    void ResetGame()
    {
        _p1Deck = new Deck();
        _p2Deck = new Deck();
        _p1Hand.Clear();
        _p2Hand.Clear();
    }



    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void DrawHands()
    {
        _p1Deck.AllCards.AddRange(_player1.PlayerDeck.AllCards);
        _p2Deck.AllCards.AddRange(_player2.PlayerDeck.AllCards);
        _p1Deck.ShuffelDeck(1);
        _p2Deck.ShuffelDeck(2);

        List<Card> cacheList1 = new List<Card>();
        for (int i = 0; i < _player1.PlayerPeepemon.Intelligence; i++)
        {
            _p1Hand.Add(_p1Deck.AllCards[i]);
            cacheList1.Add(_p1Deck.AllCards[i]);
        }

        List<Card> cacheList2 = new List<Card>();
        for (int i = 0; i < _player2.PlayerPeepemon.Intelligence; i++)
        {
            _p2Hand.Add(_p2Deck.AllCards[i]);
            cacheList2.Add(_p2Deck.AllCards[i]);
        }

        // Cleanup working decks
        foreach (var item in cacheList1) _p1Deck.RemoveCard(item);
        foreach (var item in cacheList2) _p2Deck.RemoveCard(item);
    }

}
