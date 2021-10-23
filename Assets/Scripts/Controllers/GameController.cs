using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameController : MonoBehaviour
{
    [TitleGroup("Opponents"), SerializeField] Player _player1;
    [TitleGroup("Opponents"), SerializeField] Player _player2;

    [Header("temp public")]
    public Deck _p1Deck = new Deck();
    public Deck _p2Deck = new Deck();

    public Hand _p1Hand = new Hand();
    public Hand _p2Hand = new Hand();

    public int _attackFirstIndex; // index of player who is attacking first 1 : 2

    public int _p1HP;
    public int _p2HP;

    public bool _gameHasFinished;



    [Button()]
    void StartGame()
    {
        InitFirstRound();
    }

    [Button()]
    void ResetGame()
    {
        _p1Deck = new Deck();
        _p2Deck = new Deck();
        _p1Hand.ClearHand();
        _p2Hand.ClearHand();
        _p1HP = 0;
        _p2HP = 0;
        _gameHasFinished = false;
    }

    // We check the Pepemon speed stat and cache the result as either 1 (player1) or 2 (player2)
    void CalculateFirstAttacker()
    {
        _attackFirstIndex = _player1.PlayerPepemon.Speed > _player2.PlayerPepemon.Speed ? 1 : 2;
    }

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void InitFirstRound()
    {
        // Get local copy of deck and shuffle
        _p1Deck.GetDeck().AddRange(_player1.PlayerDeck.GetDeck());
        _p2Deck.GetDeck().AddRange(_player2.PlayerDeck.GetDeck());
        _p1Deck.ShuffelDeck(1); // todo pass in random seed from client
        _p2Deck.ShuffelDeck(2); // todo pass in random seed from client

        // Draw cards to hand
        List<Card> cacheList1 = new List<Card>();
        for (int i = 0; i < _player1.PlayerPepemon.Intelligence; i++)
        {
            _p1Hand.AddCardToHand(_p1Deck.GetDeck()[i]);
            cacheList1.Add(_p1Deck.GetDeck()[i]);
        }

        // Draw cards to hand
        List<Card> cacheList2 = new List<Card>();
        for (int i = 0; i < _player2.PlayerPepemon.Intelligence; i++)
        {
            _p2Hand.AddCardToHand(_p2Deck.GetDeck()[i]);
            cacheList2.Add(_p2Deck.GetDeck()[i]);
        }

        // Cleanup working decks
        foreach (var item in cacheList1) _p1Deck.RemoveCard(item);
        foreach (var item in cacheList2) _p2Deck.RemoveCard(item);

        // PepemonHP
        _p1HP = _player1.PlayerPepemon.HealthPoints;
        _p2HP = _player2.PlayerPepemon.HealthPoints;

        // Calculate first attacker
        CalculateFirstAttacker();
        PlayRound(_attackFirstIndex);
    }

    // Plays out the round with the leading attackingIndex
    void PlayRound(int attackingIndex)
    {
        int totalAttackPower = attackingIndex == 1 ? _p1Hand.GetTotalAttackPower() : _p2Hand.GetTotalAttackPower();
        int totalDefensePower = attackingIndex == 1 ? _p2Hand.GetTotalDefensePower() : _p1Hand.GetTotalDefensePower();
        int delta = totalAttackPower - totalDefensePower;

        Debug.Log("attack first: p" + attackingIndex);
        Debug.Log("totalAP: " + totalAttackPower);
        Debug.Log("totalDP: " + totalDefensePower);
        Debug.Log("p1hp: " + _p1HP);
        Debug.Log("p2hp: " + _p2HP);
        Debug.Log("delta: " + delta);

        if (attackingIndex == 1)
        {
            _p2HP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            if (_p2HP <= 0) Winner(_player1);
        }
        else
        {
            _p1HP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            if (_p1HP <= 0) Winner(_player2);
        }
    }

    // CARRY ON:
    // We are on step 8.. now we switch roles





    void Winner(Player player)
    {
        Debug.Log("WINNER: " + player.PlayerPepemon.DisplayName);
        _gameHasFinished = true;
    }
}
