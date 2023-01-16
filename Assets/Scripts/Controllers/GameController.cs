using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;

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
        _attackFirstIndex = 1;
        _roundNumber = 0;
        _player1.Reset();
        _player2.Reset();
    }

    // Each player shuffles their deck and draws cards equal to pepemon intelligence 
    void InitFirstRound()
    {
        _roundNumber = 0;
        _player1.Initialise(1);
        _player2.Initialise(2);
        _uiController.InitialiseGame(_player1, _player2);

        _player1.GetAndShuffelDeck(69, _roundNumber, BattlePrepController.battleData.battleRngSeed);
        _player2.GetAndShuffelDeck(420, _roundNumber, BattlePrepController.battleData.battleRngSeed);

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
        if (_roundNumber % 5 == 0)
        {
            _player1.GetAndShuffelDeck(69, _roundNumber, BattlePrepController.battleData.battleRngSeed);
            _player2.GetAndShuffelDeck(420, _roundNumber, BattlePrepController.battleData.battleRngSeed);
        }
        else
        {
            //Don't need to refresh cards now

            // Get temp support info of previous turn's hands and calculate their effect for the new turn
            _player1.CalcSupportCardsOnTable(_player2);
            _player2.CalcSupportCardsOnTable(_player1);
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
        //Debug.Log("<b>STARTING ROUND: </b>" + _roundNumber);
        for (int i = 0; i < 2; i++)
        {
            if (_gameHasFinished == false)
            {
                // PlayHand(_attackFirstIndex);
                int attackingIndex = _attackFirstIndex;


                int totalAttackPower = attackingIndex == 1 ? _player1.CurrentPepemonStats.atk : _player2.CurrentPepemonStats.atk;
                int totalDefensePower = attackingIndex == 1 ? _player2.CurrentPepemonStats.def : _player1.CurrentPepemonStats.def;
                //int delta = totalAttackPower - totalDefensePower;
                Debug.Log("fight current round =" + _roundNumber);
                Debug.Log("fight attackingIndex == 1? " + (attackingIndex == 1));
                Debug.Log("fight totalAttackPower=" + totalAttackPower);
                Debug.Log("fight totalDefensePower=" + totalDefensePower);

                //Debug.Log("Who is attacking: " + attackingIndex);
                // Remove played cards from current hand
                if (attackingIndex == 1)
                {
                    _uiController.FlipCards(1);

                    //wait for animations showing the attacking/defending cards
                    yield return new WaitForSeconds(.5f);

                    _uiController.StartCoroutine(_uiController.DisplayTotalValues(1, totalAttackPower, totalDefensePower));

                    yield return new WaitForSeconds(2f);

                    _player2.CurrentHP -= totalAttackPower > totalDefensePower ? (totalAttackPower - totalDefensePower) : 1;
                    // _player2.CurrentHand.RemoveAllDefenseCards();
                    // _player1.CurrentHand.RemoveAllOffenseCards();

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
                    _player1.CurrentHP -= totalAttackPower > totalDefensePower ? (totalAttackPower - totalDefensePower) : 1;
                    //_player1.CurrentHand.RemoveAllDefenseCards();
                    //_player2.CurrentHand.RemoveAllOffenseCards();

                    _uiController.UpdateUI();
                    player1Controller.UpdateCard(_player1);
                    player2Controller.UpdateCard(_player2);

                    if (_player1.CurrentHP <= 0) Winner(_player2);
                }
                Debug.Log("goForBattle _player1.CurrentHP=" + _player1.CurrentHP);
                Debug.Log("goForBattle _player2.CurrentHP=" + _player2.CurrentHP);

                //Debug.Log("waiting 2f");
                yield return new WaitForSeconds(1f);

                // cleanup UI
                _uiController.FlipCards(3);
                //Debug.Log(" after slow");


                _attackFirstIndex = _attackFirstIndex == 1 ? 2 : 1;
            }
        }
        //Debug.Log("<b>FINISHED ROUND: </b>" + _roundNumber);
        _roundNumber++;
        _isPlayingRound = false;
    }


    // We calculate the effect of every card in the player's hand
    private void CalcSupportCardsInHand(Player atkPlayer, Player defPlayer)
    {
        // If this card is included in player's hand, adds an additional power equal to the total of
        // all normal offense/defense cards
        bool isPower0CardIncluded = false;

        // Total sum of normal support cards
        int totalNormalPower = 0;

        // used to check whether or not an unstackable card was already used
        var unstackableCards = new List<int>();

        // Calc attacker hand
        // Loop through every card the attacker has in his hand
        foreach (var card in atkPlayer.CurrentHand.GetCardsInHand)
        {
            if (card.Type == PlayCardType.Offense)
            {
                // Card type is OFFENSE.
                // Calc effects of EffectOne array
                foreach (var effectOne in card.effectOnes)
                {
                    (bool isTriggered, int multiplier) = CheckReqCode(atkPlayer, defPlayer, effectOne.reqCode, true);
                    if (isTriggered)
                    {
                        //use triggeredPower if triggered
                        atkPlayer.CurrentPepemonStats.atk += effectOne.triggeredPower * multiplier;
                        totalNormalPower += effectOne.triggeredPower * multiplier;
                    }
                    else
                    {
                        //use basePower if not
                        atkPlayer.CurrentPepemonStats.atk += effectOne.basePower;
                        totalNormalPower += effectOne.basePower;
                    }
                }
            }
            else if (card.Type == PlayCardType.SpecialOffense)
            {
                // Card type is STRONG OFFENSE.
                //Make sure unstackable cards can't be stacked
                if (card.Unstackable)
                {
                    // Check if card is new to previous cards
                    if (unstackableCards.Contains(card.ID) ||
                        // Check if card is new to temp support info cards
                        atkPlayer.CurrentHand.GetCardsInHand.Any(c => c.ID == card.ID))
                    {
                        // If it isn't being used for the first time - skip card
                        continue;
                    }
                    unstackableCards.Add(card.ID);
                }

                // Calc effects of EffectOne array
                foreach (var effectOne in card.effectOnes)
                {
                    (bool isTriggered, int multiplier) = CheckReqCode(atkPlayer, defPlayer, effectOne.reqCode, true);
                    if (isTriggered)
                    {
                        if (multiplier > 1)
                        {
                            atkPlayer.CurrentPepemonStats.atk += effectOne.triggeredPower * multiplier;
                        }
                        else
                        {
                            if (effectOne.effectTo == EffectTo.SpecialAttack)
                            {
                                // If it's a use Special Attack instead of Attack card
                                atkPlayer.CurrentPepemonStats.atk = atkPlayer.CurrentPepemonStats.sAtk;
                                continue;
                            }
                            else if (effectOne.triggeredPower == 0)
                            {
                                // We have a card that says ATK is increased by amount
                                // Equal to the total of all offense cards in the current turn
                                isPower0CardIncluded = true;
                                continue;
                            }
                            atkPlayer.CurrentPepemonStats.atk += effectOne.triggeredPower;
                        }
                    }
                    else
                    {
                        //If not triggered: use base power instead
                        atkPlayer.CurrentPepemonStats.atk += effectOne.basePower;
                        totalNormalPower += effectOne.basePower;
                    }
                }

                // If card lasts for >1 turns, Add card to table if <5 on table currently
                if (card.effectMany.power != 0 && atkPlayer.CurrentHand.GetCardsInHand.Count < 5)
                {
                    // todo: add animation to display card being added
                    atkPlayer.CurrentHand.AddCardToHand(card);
                }
            }

        }

        if (isPower0CardIncluded)
        {
            //If we have a card that says ATK is increased by amount equal to total of all offense cards
            atkPlayer.CurrentPepemonStats.atk += totalNormalPower;
        }

        // Calc defense hand
        isPower0CardIncluded = false;
        totalNormalPower = 0;
        unstackableCards.Clear();

        foreach (var card in defPlayer.CurrentHand.GetCardsInHand)
        {
            if (card.Type == PlayCardType.Defense)
            {
                // Card type is OFFENSE.
                // Calc effects of EffectOne array
                foreach (var effectOne in card.effectOnes)
                {
                    (bool isTriggered, int multiplier) = CheckReqCode(atkPlayer, defPlayer, effectOne.reqCode, true);
                    if (isTriggered)
                    {
                        //use triggeredPower if triggered
                        defPlayer.CurrentPepemonStats.def += effectOne.triggeredPower * multiplier;
                        totalNormalPower += effectOne.triggeredPower * multiplier;
                    }
                    else
                    {
                        //use basePower if not
                        defPlayer.CurrentPepemonStats.def += effectOne.basePower;
                        totalNormalPower += effectOne.basePower;
                    }
                }
            }
            else if (card.Type == PlayCardType.SpecialDefense)
            {
                // Card type is STRONG DEFENSE
                //Make sure unstackable cards can't be stacked
                if (card.Unstackable)
                {
                    // Check if card is new to previous cards
                    if (unstackableCards.Contains(card.ID) ||
                        // Check if card is new to temp support info cards
                        defPlayer.CurrentHand.GetCardsInHand.Any(c => c.ID == card.ID))
                    {
                        // If it isn't being used for the first time - skip card
                        continue;
                    }
                    unstackableCards.Add(card.ID);
                }

                // Calc effects of EffectOne array
                foreach (var effectOne in card.effectOnes)
                {
                    (bool isTriggered, int multiplier) = CheckReqCode(atkPlayer, defPlayer, effectOne.reqCode, true);
                    if (isTriggered)
                    {
                        if (multiplier > 1)
                        {
                            defPlayer.CurrentPepemonStats.def += effectOne.triggeredPower * multiplier;
                        }
                        else
                        {
                            if (effectOne.effectTo == EffectTo.SpecialDefense)
                            {
                                // If it's a use Special Defense instead of Defense card
                                defPlayer.CurrentPepemonStats.def = defPlayer.CurrentPepemonStats.sDef;
                                continue;
                            }
                            else if (effectOne.triggeredPower == 0)
                            {
                                // Equal to the total of all defense cards in the current turn
                                isPower0CardIncluded = true;
                                continue;
                            }
                            defPlayer.CurrentPepemonStats.def += effectOne.triggeredPower;
                        }
                    }
                    else
                    {
                        //If not triggered: use base power instead
                        defPlayer.CurrentPepemonStats.def += effectOne.basePower;
                        totalNormalPower += effectOne.basePower;
                    }
                }

                // If card lasts for >1 turns, Add card to table if <5 on table currently
                if (card.effectMany.power != 0 && defPlayer.CurrentHand.GetCardsInHand.Count < 5)
                {
                    // todo: add animation to display card being added
                    defPlayer.CurrentHand.AddCardToHand(card);
                }
            }
        }

        if (isPower0CardIncluded)
        {
            //If we have a card that says DEF is increased by amount equal to total of all defense cards
            defPlayer.CurrentPepemonStats.def += totalNormalPower;
        }
    }


    /// <summary>
    /// Checks if the requirements are satisfied for a certain code
    /// </summary>
    /// <returns>bool telling if a requirement was satisfied, and a multiplier for the card's attack power</returns>
    private static Tuple<bool, int> CheckReqCode(Player atkPlayer, Player defPlayer, int reqCode, bool isAttacker)
    {
        bool isTriggered = false;
        int multiplier = 1;
        switch (reqCode)
        {
            case 0:
                // No requirement
                isTriggered = true;
                break;
            case 1:
                // Intelligence of offense pepemon <= 5.
                isTriggered = atkPlayer.CurrentPepemonStats.inte <= 5;
                break;
            case 2:
                // Number of defense cards of defense pepemon is 0.
                isTriggered = defPlayer.CurrentHand.AllDefenseCards.Count == 0;
                break;
            case 3:
                // Each +2 offense cards of offense pepemon.
                multiplier = atkPlayer.CurrentHand.AllOffenseCards.Count(
                    card => card.effectOnes.FirstOrDefault(e => e.basePower == 2).effectTo == EffectTo.Attack);
                isTriggered = multiplier > 0;
                break;
            case 4:
                // Each +3 offense cards of offense pepemon.
                multiplier = atkPlayer.CurrentHand.AllOffenseCards.Count(
                    card => card.effectOnes.FirstOrDefault(e => e.basePower == 3).effectTo == EffectTo.Attack);
                isTriggered = multiplier > 0;
                break;
            case 5:
                // Each offense card of offense pepemon.
                multiplier = atkPlayer.CurrentHand.AllOffenseCards.Count;
                isTriggered = multiplier > 0;
                break;
            case 6:
                // Each +3 defense card of defense pepemon.
                multiplier = defPlayer.CurrentHand.AllOffenseCards.Count(
                    card => card.effectOnes.FirstOrDefault(e => e.basePower == 3).effectTo == EffectTo.Defense);
                isTriggered = multiplier > 0;
                break;
            case 7:
                // Each +3 defense card of defense pepemon.
                multiplier = defPlayer.CurrentHand.AllOffenseCards.Count(
                    card => card.effectOnes.FirstOrDefault(e => e.basePower == 4).effectTo == EffectTo.Defense);
                isTriggered = multiplier > 0;
                break;
            case 8:
                // Intelligence of defense pepemon <= 5.
                isTriggered = (defPlayer.CurrentPepemonStats.inte <= 5);
                break;
            case 9:
                // Intelligence of defense pepemon >= 7.
                isTriggered = (defPlayer.CurrentPepemonStats.inte >= 7);
                break;
            case 10:
                // Offense pepemon is using strong attack
                isTriggered = atkPlayer.CurrentHand.AllSpecialOffenseCards.Count > 0;
                break;
            case 11:
                // The current HP is less than 50% of max HP.
                var player = defPlayer;
                if (isAttacker)
                {
                    player = atkPlayer;
                }
                isTriggered = player.CurrentHP * 2 <= player.PlayerPepemon.HealthPoints;
                break;
            default:
                break;
        }
        return new(isTriggered, multiplier);
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
