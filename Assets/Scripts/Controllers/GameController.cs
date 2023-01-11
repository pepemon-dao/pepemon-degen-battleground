using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// Manages the automation of the game. Each round is composed of two hands being played (offense and defense)
public class GameController : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] UIController _uiController;

    [TitleGroup("Opponents"), SerializeField] Player _player1;
    [TitleGroup("Opponents"), SerializeField] Player _player2;

    [TitleGroup("Behaviour"), SerializeField] bool playAutomatically;

    [TitleGroup("Debug"), ShowInInspector, ReadOnly] int _attackFirstIndex; // index of player who is attacking first 1 : 2
    [TitleGroup("Debug"), ShowInInspector, ReadOnly] int _roundNumber;
    [TitleGroup("Debug"), ShowInInspector, ReadOnly] bool _gameHasFinished;
    [TitleGroup("Debug"), ShowInInspector, ReadOnly] bool _isPlayingRound;

    [BoxGroup("Pepemon Controller")] public PepemonCardController player1Controller;
    [BoxGroup("Pepemon Controller")] public PepemonCardController player2Controller;

    [TitleGroup("Scriptable objects list"), SerializeField] DataContainer CardsScriptableObjsData;

    private void Start()
    {
        PrepareDecksBeforeBattle();

        player1Controller.PopulateCard(_player1.PlayerPepemon);
        player2Controller.PopulateCard(_player2.PlayerPepemon);

        if (playAutomatically)
        {
            InitFirstRound();
            StartCoroutine(LoopGame());
        }
    }

    private void PrepareDecksBeforeBattle()
    {
        // might be null if ran from unity editor
        if (BattlePrepController.battleData != null)
        {
            _player1.SetPlayerDeck(
                pepemon: CardsScriptableObjsData.GetPepemonById(BattlePrepController.battleData.player1BattleCard.ToString()),
                supportCards: CardsScriptableObjsData.GetAllCardsByIds(BattlePrepController.battleData.player1SupportCards));

            _player2.SetPlayerDeck(
                pepemon: CardsScriptableObjsData.GetPepemonById(BattlePrepController.battleData.player2BattleCard.ToString()),
                supportCards: CardsScriptableObjsData.GetAllCardsByIds(BattlePrepController.battleData.player2SupportCards));
        }
        else
        {
            Debug.LogWarning("Battle data was not set");
        }
    }

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

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void InitFirstRound()
    {
        _roundNumber = 1;
        _player1.Initialise(1);
        _player2.Initialise(2);
        _uiController.InitialiseGame(_player1, _player2);

        _player1.GetAndShuffelDeck(2); // todo: pass in seed from client connection index
        _player2.GetAndShuffelDeck(1); // todo: pass in seed from client connection index

        // Calculate first attacker
        _attackFirstIndex = _player1.PlayerPepemon.Speed > _player2.PlayerPepemon.Speed ? 1 : 2;
    }

    IEnumerator LoopGame()
    {
        yield return new WaitForSeconds(1f);
        while (_gameHasFinished == false)
        {
            yield return new WaitUntil(() => _isPlayingRound == false);
            yield return StartCoroutine(StartRound());
        }
    }

    [Button()]
    void sStartGame()
    {
        StartCoroutine(StartRound());
    }

    IEnumerator StartRound()
    {
        if (_gameHasFinished == true)
        {
            yield return null;
        }
        _uiController.NewRoundDisplay();
        yield return new WaitForSeconds(1.6f);
        _uiController.HideNewRoundDisplay();
        yield return new WaitForSeconds(.3f);

        // Check if we passed 5 and if so reshuffel decks
        if (_roundNumber >= 5)
        {
            _player1.GetAndShuffelDeck(2 + _roundNumber); // todo: pass in seed from client connection index
            _player2.GetAndShuffelDeck(1 + _roundNumber); // todo: pass in seed from client connection index
        }

        Debug.Log("<b>DRAWING HANDS</b>");
        _player1.DrawNewHand();
        _player2.DrawNewHand();

        // Display hands in the UI
        _uiController.DisplayHands();

        //delay to show drawing of cards
        yield return new WaitForSeconds(2f);

        //! need to think of a better way to display the cards being played

        _isPlayingRound = true;
        Debug.Log("<b>STARTING ROUND: </b>" + _roundNumber);
        for (int i = 0; i < 2; i++)
        {
            if (_gameHasFinished == false)
            {
                // PlayHand(_attackFirstIndex);
                int attackingIndex = _attackFirstIndex;


                int totalAttackPower = attackingIndex == 1 ? _player1.CurrentHand.GetTotalAttackPower(_player1.PlayerPepemon.Attack) : _player2.CurrentHand.GetTotalAttackPower(_player2.PlayerPepemon.Attack);
                int totalDefensePower = attackingIndex == 1 ? _player2.CurrentHand.GetTotalDefensePower(_player2.PlayerPepemon.Defense) : _player1.CurrentHand.GetTotalDefensePower(_player1.PlayerPepemon.Defense);
                int delta = totalAttackPower - totalDefensePower;

                Debug.Log("Who is attacking: " + attackingIndex);
                // Remove played cards from current hand
                if (attackingIndex == 1)
                {
                    _uiController.FlipCards(1);

                    //wait for animations showing the attacking/defending cards
                    yield return new WaitForSeconds(.5f);

                    _uiController.StartCoroutine(_uiController.DisplayTotalValues(1, totalAttackPower, totalDefensePower));

                    yield return new WaitForSeconds(2f);

                    _player2.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
                    _player2.CurrentHand.RemoveAllDefenseCards();
                    _player1.CurrentHand.RemoveAllOffenseCards();

                    _uiController.UpdateUI();
                    player1Controller.UpdateCard(_player1);
                    player2Controller.UpdateCard(_player2);

                    if (_player2.CurrentHP <= 0) Winner(_player1);
                }
                else
                {
                    _uiController.FlipCards(2);

                    yield return new WaitForSeconds(2f);
                    _uiController.StartCoroutine(_uiController.DisplayTotalValues(2, totalAttackPower, totalDefensePower));

                    yield return new WaitForSeconds(1f);
                    _player1.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
                    _player1.CurrentHand.RemoveAllDefenseCards();
                    _player2.CurrentHand.RemoveAllOffenseCards();

                    _uiController.UpdateUI();
                    player1Controller.UpdateCard(_player1);
                    player2Controller.UpdateCard(_player2);

                    if (_player1.CurrentHP <= 0) Winner(_player2);
                }

                Debug.Log("waiting 2f");
                yield return new WaitForSeconds(1f);

                // cleanup UI
                _uiController.FlipCards(3);
                Debug.Log(" after slow");


                _attackFirstIndex = _attackFirstIndex == 1 ? 2 : 1;
            }
        }
        Debug.Log("<b>FINISHED ROUND: </b>" + _roundNumber);
        _roundNumber++;
        _isPlayingRound = false;
    }

    // Plays out each round split (offense & defense) lead by the attacking index
    void PlayHand(int attackingIndex)
    {
        int totalAttackPower = attackingIndex == 1 ? _player1.CurrentHand.GetTotalAttackPower(_player1.PlayerPepemon.Attack) : _player2.CurrentHand.GetTotalAttackPower(_player2.PlayerPepemon.Attack);
        int totalDefensePower = attackingIndex == 1 ? _player2.CurrentHand.GetTotalDefensePower(_player2.PlayerPepemon.Defense) : _player1.CurrentHand.GetTotalDefensePower(_player1.PlayerPepemon.Defense);
        int delta = totalAttackPower - totalDefensePower;

        Debug.Log("Who is attacking: " + attackingIndex);

        //      Debug.Log("<b>STARTING HAND </b>");
        //      Debug.Log("attacker: " + attackingIndex);
        //      Debug.Log("totalAP: " + totalAttackPower);
        //      Debug.Log("totalDP: " + totalDefensePower);
        //      Debug.Log("p1hp: " + _player1.CurrentHP);
        //      Debug.Log("p2hp: " + _player2.CurrentHP);
        //      Debug.Log("delta: " + delta);

        // Remove played cards from current hand
        if (attackingIndex == 1)
        {
            _player2.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            _player2.CurrentHand.RemoveAllDefenseCards();
            _player1.CurrentHand.RemoveAllOffenseCards();

            _uiController.FlipCards(1);
            _uiController.UpdateUI();
            player1Controller.UpdateCard(_player1);
            player2Controller.UpdateCard(_player2);

            if (_player2.CurrentHP <= 0) Winner(_player1);
        }
        else
        {
            _player1.CurrentHP -= delta > 0 ? (totalAttackPower - totalDefensePower) : 1;
            _player1.CurrentHand.RemoveAllDefenseCards();
            _player2.CurrentHand.RemoveAllOffenseCards();

            _uiController.FlipCards(2);
            _uiController.UpdateUI();
            player1Controller.UpdateCard(_player1);
            player2Controller.UpdateCard(_player2);

            if (_player1.CurrentHP <= 0) Winner(_player2);
        }

        // cleanup UI
        _uiController.FlipCards(3);
        Debug.Log(" after slow");

        //   Debug.Log("attacker: " + attackingIndex);
        //   Debug.Log("totalAP: " + totalAttackPower);
        //   Debug.Log("totalDP: " + totalDefensePower);
        //   Debug.Log("p1hp: " + _player1.CurrentHP);
        //   Debug.Log("p2hp: " + _player2.CurrentHP);
    }

    public int GetRoundNumber() => _roundNumber;


    void Winner(Player player)
    {
        _uiController.DisplayWinner(player);
        _gameHasFinished = true;
    }
}
