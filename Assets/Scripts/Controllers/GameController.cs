using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// Manages the automation of the game. Each round is composed of two hands being played (offense and defense)
public class GameController : MonoBehaviour
{
    [TitleGroup("Opponents"), SerializeField] Player _player1;
    [TitleGroup("Opponents"), SerializeField] Player _player2;

    [Header("temp public")]
    public int _attackFirstIndex; // index of player who is attacking first 1 : 2
    public bool _gameHasFinished;
    public int _roundNumber;



    [Button()]
    public void StartGame()
    {
        InitFirstRound();
    }

    [Button()]
    void ResetGame()
    {
        _gameHasFinished = false;
        _attackFirstIndex = 0;
        _roundNumber = 0;
        _player1.Reset();
        _player2.Reset();
    }

    // We check the Pepemon speed stat and cache the result as either 1 (player1) or 2 (player2)
    void CalculateFirstAttacker()
    {
        _attackFirstIndex = _player1.PlayerPepemon.Speed > _player2.PlayerPepemon.Speed ? 1 : 2;
    }

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void InitFirstRound()
    {
        _roundNumber = 1;
        _player1.Initialise(1); // todo: pass in seed from client connection index
        _player2.Initialise(2); // todo: pass in seed from client connection index

        // Calculate first attacker
        CalculateFirstAttacker();
        StartRound();
    }

    [Button()]
    void StartRound()
    {
        Debug.Log("<b>DRAWING HANDS:</b>");
        _player1.DrawNewHand();
        _player2.DrawNewHand();

        Debug.Log("<b>STARTING ROUND: </b>" + _roundNumber);
        for (int i = 0; i < 2; i++)
        {
            if (_gameHasFinished == false)
            {
                PlayHand(_attackFirstIndex);
                _attackFirstIndex = _attackFirstIndex == 1 ? 2 : 1;
            }
        }
        _roundNumber++;
    }

    // Plays out each round split (offense & defense) lead by the attacking index
    void PlayHand(int attackingIndex)
    {
        int totalAttackPower = attackingIndex == 1 ? _player1.CurrentHand.GetTotalAttackPower() : _player2.CurrentHand.GetTotalAttackPower();
        int totalDefensePower = attackingIndex == 1 ? _player2.CurrentHand.GetTotalDefensePower() : _player1.CurrentHand.GetTotalDefensePower();
        int delta = totalAttackPower - totalDefensePower;

        Debug.Log("attacker p: " + attackingIndex);
        Debug.Log("totalAP: " + totalAttackPower);
        Debug.Log("totalDP: " + totalDefensePower);
        Debug.Log("p1hp: " + _player1.CurrentHP);
        Debug.Log("p2hp: " + _player2.CurrentHP);
        Debug.Log("delta: " + delta);

        // Remove played cards from current hand
        if (attackingIndex == 1)
        {
            _player2.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            _player2.CurrentHand.RemoveAllDefenseCards();
            _player1.CurrentHand.RemoveAllOffenseCards();

            if (_player2.CurrentHP <= 0) Winner(_player1);
        }
        else
        {
            _player1.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            _player1.CurrentHand.RemoveAllDefenseCards();
            _player2.CurrentHand.RemoveAllOffenseCards();

            if (_player1.CurrentHP <= 0) Winner(_player2);
        }
    }




    void Winner(Player player)
    {
        Debug.Log("WINNER: " + player.PlayerPepemon.DisplayName);
        _gameHasFinished = true;
    }
}
