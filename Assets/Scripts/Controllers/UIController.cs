using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using Pepemon.Battle;
using Pepemon.Onboarding;
using static UnityEngine.ParticleSystem;
using System.Reflection;
// Handles displaying game state
public class UIController : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _cardPrefab;
    [TitleGroup("Component References"), SerializeField] GameController _gameController;
    [TitleGroup("Component References"), SerializeField] Sprite _defendIcon;
    [TitleGroup("Component References"), SerializeField] Sprite _attackIcon;
    [TitleGroup("Component References"), SerializeField] TextMeshProUGUI _player1Health;
    [TitleGroup("Component References"), SerializeField] TextMeshProUGUI _player2Health;
    [TitleGroup("Component References"), SerializeField] VersusScreenDisplay _versusScreen;

    [BoxGroup("Sidebar")]
    [SerializeField, BoxGroup("Sidebar")] TextMeshProUGUI _roundCount;
    [SerializeField, BoxGroup("Sidebar/ID1")] TextMeshProUGUI _index1DeckCount;
    [SerializeField, BoxGroup("Sidebar/ID2")] TextMeshProUGUI _index2DeckCount;

    [SerializeField, BoxGroup("Board")] Transform _index1CardContainer;
    [SerializeField, BoxGroup("Board")] Transform _index2CardContainer;
    [SerializeField, BoxGroup("Board")] Transform _deck1Transform;
    [SerializeField, BoxGroup("Board")] Transform _deck2Transform;
    [SerializeField, BoxGroup("Board")] GameObject _blurryScreen;
    [SerializeField, BoxGroup("Board")] Transform _board;
    [SerializeField, BoxGroup("Board")] Image _player1TotalDisplay;
    [SerializeField, BoxGroup("Board")] Image _player2TotalDisplay;
    [SerializeField, BoxGroup("Board")] TextMeshProUGUI _player1TotalTextDisplay;
    [SerializeField, BoxGroup("Board")] TextMeshProUGUI _player2TotalTextDisplay;

    [SerializeField, BoxGroup("Board")] List<CardController> _player1Cards = new List<CardController>();
    [SerializeField, BoxGroup("Board")] List<CardController> _player2Cards = new List<CardController>();
    [SerializeField, BoxGroup("Board")] GameObject _newRoundDisplay;
    [SerializeField, BoxGroup("Board")] TextMeshProUGUI _newRoundDisplayText;

    [SerializeField, BoxGroup("Effects")] GameObject _attackTallyPS;         //the effect spawned by the card when tally the defense/attack amount

    [TitleGroup("Pacing"), SerializeField] float _cardDealStagger = 0.15f;
    [TitleGroup("Pacing"), SerializeField] float _cardFlipStagger = 0.22f;

    [TitleGroup("Post battle control"), SerializeField] PostBattleScreenController PostScreenController;

    /// <summary>Colour of a card that is not being played this turn.</summary>
    private static readonly Color DimmedCardColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    Player _player1;
    Player _player2;
    Transform _sidebar;

    private Coroutine routine;

    public void InitialiseGame(Player player1, Player player2)
    {
        _player1 = player1;
        _player2 = player2;

        var sidebarGo = GameObject.Find("Sidebar");
        _sidebar = sidebarGo != null ? sidebarGo.transform : null;

        UpdateUI();

        _index1DeckCount.text = player1.PlayerDeck.GetDeck().Count + "cards";
        _index2DeckCount.text = player2.PlayerDeck.GetDeck().Count + "cards";

        SetVersusScreen(player1, player2);
    }

    private void SetVersusScreen(Player player1, Player player2)
    {
        Sprite imgR = player2.PlayerPepemon.CardContent;
        Sprite imgB = player1.PlayerPepemon.CardContent;
        Sprite bgR = player2.PlayerPepemon.CardBG;
        Sprite bgB = player1.PlayerPepemon.CardBG;
        string rN = player2.PlayerPepemon.DisplayName;
        string bN = player1.PlayerPepemon.DisplayName;

        // In the first battle the opponent is a named rival rather than an anonymous bot.
        string taunt = BattlePrepController.battleData.isBotMatch
            ? $"{TutorialScript.RivalName}: \"{TutorialScript.RivalTaunt}\""
            : null;

        _versusScreen.SetVersusScreen(imgR, imgB, bgB, bgR, bN, rN, taunt);
    }

    public void NewRoundDisplay()
    {
        _newRoundDisplay.SetActive(true);
        _newRoundDisplayText.text = "Round: " + _gameController.GetRoundNumber();
    }

    public void HideNewRoundDisplay()
    {
        _newRoundDisplay.SetActive(false);
    }


    // part1 lay down all AD + DF cards
    // play the hand
    // Reverse role
    // next round
    public void DisplayHands()
    {
        // Cleanup old hand
        foreach (CardController _card in _player1Cards) { GameObject.Destroy(_card.gameObject); }
        foreach (CardController _card in _player2Cards) { GameObject.Destroy(_card.gameObject); }
        foreach (Transform child in _index1CardContainer) GameObject.Destroy(child.gameObject);
        foreach (Transform child in _index2CardContainer) GameObject.Destroy(child.gameObject);
        _player1Cards.Clear();
        _player2Cards.Clear();
        // Display new hand
        StartCoroutine(DrawCards(_player1));
        StartCoroutine(DrawCards(_player2));

        // Update deck count
        _index1DeckCount.text = _player1.CurrentDeck.GetDeck().Count + "cards";
        _index2DeckCount.text = _player2.CurrentDeck.GetDeck().Count + "cards";
        _roundCount.text = "R: " + _gameController.GetRoundNumber();
    }

    IEnumerator DrawCards(Player _whichPlayer)
    {
        for (int i = 0; i < _whichPlayer.CurrentHand.GetCardsInHand.Count; i++)
        {
            //delay between each card spawn for effect
            yield return new WaitForSeconds(_cardDealStagger);
            GameObject go = new GameObject("Card Container", typeof(RectTransform));
            GameObject card;
            if (_whichPlayer == _player1) card = Instantiate(_cardPrefab, _deck2Transform.position, Quaternion.identity);
            else card = Instantiate(_cardPrefab, _deck1Transform.position, Quaternion.identity);
            card.transform.SetParent(_board);


            if (_whichPlayer == _player1) go.transform.SetParent(_index1CardContainer);
            else go.transform.SetParent(_index2CardContainer);
            go.transform.localPosition = Vector3.zero;
            card.GetComponent<CardController>().PopulateCard(_whichPlayer.CurrentHand.GetCardsInHand[i]);

            card.GetComponent<CardController>().SetTargetTransform(go.transform);
            if (_whichPlayer == _player1) _player1Cards.Add(card.GetComponent<CardController>());
            else _player2Cards.Add(card.GetComponent<CardController>());

            if (SFXManager.Instance != null) SFXManager.Instance.DealSFX();
        }
    }

    public void FlipCards(int attackIndex)
    {
        if(routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlipCardsRoutine(attackIndex));
    }


    /// <summary>
    /// Lifts the cards actually played this turn and dims the rest.
    ///
    /// This previously greyed the played cards - the opposite of the intended emphasis - and
    /// never restored the colour, so every card in the battle was permanently grey after the
    /// first turn. Now the played cards are the only ones at full colour, which doubles as
    /// the spotlight the tutorial needs when it explains what support cards do.
    /// </summary>
    public IEnumerator FlipCardsRoutine(int attackIndex)
    {
        if (attackIndex == 3) // reset
        {
            foreach (var card in _player2Cards) ResetCard(card);
            foreach (var card in _player1Cards) ResetCard(card);

            BringSidebarToFront();
            yield break;
        }

        var attackerCards = attackIndex == 1 ? _player1Cards : _player2Cards;
        var defenderCards = attackIndex == 1 ? _player2Cards : _player1Cards;

        foreach (var card in attackerCards) SetCardHighlight(card, false);
        foreach (var card in defenderCards) SetCardHighlight(card, false);

        // The attacker plays its offense cards.
        foreach (var card in attackerCards)
        {
            if (card == null || card.HostedCard == null || !card.HostedCard.IsAttackingCard()) continue;

            card.SetAttackingTransform(new Vector3(0, 5f, 0));
            SetCardHighlight(card, true);

            yield return new WaitForSeconds(_cardFlipStagger);
        }

        // The defender answers with its defense cards.
        foreach (var card in defenderCards)
        {
            if (card == null || card.HostedCard == null || card.HostedCard.IsAttackingCard()) continue;

            card.SetAttackingTransform(new Vector3(0, 5f, 0));
            SetCardHighlight(card, true);

            yield return new WaitForSeconds(_cardFlipStagger);
        }

        BringSidebarToFront();
    }

    private static void ResetCard(CardController card)
    {
        if (card == null) return;

        card.ReturnToBaseTransform();
        SetCardHighlight(card, true);
    }

    private static void SetCardHighlight(CardController card, bool highlighted)
    {
        if (card == null) return;

        var image = card.GetComponent<Image>();
        if (image == null) return;

        image.color = highlighted ? Color.white : DimmedCardColor;
    }

    private void BringSidebarToFront()
    {
        // Make pepemon card, stage & hp points appear in front of support cards
        if (_sidebar != null) _sidebar.SetAsLastSibling();
    }

    /// <summary>
    /// Sequence where the shielf and sword icons are displayed and the total attk/def are displayed
    /// </summary>
    /// <param name="attackIndex"></param>
    /// <param name="_totalAttack"></param>
    /// <param name="_totalDef"></param>
    /// <returns></returns>
    public IEnumerator DisplayTotalValues(int attackIndex, int _totalAttack, int _totalDef, float clashSeconds = 1.5f)
    {
        //display the proper attack and defend symbols
        if (attackIndex == 1)
        {
            _player1TotalTextDisplay.text = _totalAttack.ToString();
            _player2TotalTextDisplay.text = _totalDef.ToString();
            _player1TotalDisplay.sprite = _attackIcon;
            _player2TotalDisplay.sprite = _defendIcon;
        }
        else if (attackIndex == 2)
        {
            _player1TotalTextDisplay.text = _totalDef.ToString();
            _player2TotalTextDisplay.text = _totalAttack.ToString();
            _player1TotalDisplay.sprite = _defendIcon;
            _player2TotalDisplay.sprite = _attackIcon;
        }

        _player1TotalDisplay.gameObject.SetActive(true);
        _player2TotalDisplay.gameObject.SetActive(true);
        TallyUpCardValues(attackIndex);


        _player1TotalDisplay.GetComponent<Animator>().SetTrigger("Clash");
        _player2TotalDisplay.GetComponent<Animator>().SetTrigger("Clash");

        _blurryScreen.SetActive(true);

        // The caller now awaits this routine and applies damage immediately afterwards, so the
        // whole clash has to fit inside clashSeconds. Previously this ran ~3.5s regardless
        // while the caller waited 2s, leaving the blur up over the damage number.
        yield return new WaitForSeconds(Mathf.Max(0.2f, clashSeconds));

        _blurryScreen.SetActive(false);

        _player1TotalDisplay.gameObject.SetActive(false);
        _player2TotalDisplay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Impact shake on the board. <paramref name="severity"/> is 0..1 as a fraction of max HP.
    /// The project had no impact feedback of any kind before this.
    /// </summary>
    public void ShakeBoard(float severity)
    {
        if (_board == null) return;

        var strength = Mathf.Lerp(5f, 24f, Mathf.Clamp01(severity));

        // Complete any in-flight shake first so repeated hits cannot accumulate drift.
        _board.DOComplete();
        _board.DOShakePosition(0.22f, strength, 18, 90f, false, true);
    }

    /// <summary>
    /// Show the effect/animation of the player's total def/attack being tallied up. This involves particle systems spawning and moving toward the display.
    /// </summary>
    /// <param name="_attackIndex"></param>
    void TallyUpCardValues(int _attackIndex)
    {
        if (_attackIndex == 1)
        {
            foreach (CardController _card in _player1Cards)
            {
                if (_card.HostedCard.IsAttackingCard())
                {
                    GameObject _ps = Instantiate(_attackTallyPS, _card.transform.position, Quaternion.identity);
                    _ps.GetComponent<TallyParticleEffect>().targetPosition = _player1TotalDisplay.transform.position;
                    var mainModule = _ps.GetComponent<ParticleSystem>().main;
                    Color startColor = Color.red;
                    startColor.a = 0.5f;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(startColor);
                }
            }
            foreach (CardController _card in _player2Cards)
            {
                if (_card.HostedCard.Type == PlayCardType.Defense)
                {
                    GameObject _ps = Instantiate(_attackTallyPS, _card.transform.position, Quaternion.identity);
                    _ps.GetComponent<TallyParticleEffect>().targetPosition = _player2TotalDisplay.transform.position;
                    var mainModule = _ps.GetComponent<ParticleSystem>().main;
                    Color startColor = Color.blue;
                    startColor.a = 0.5f;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(startColor);
                }
            }
        }
        else if (_attackIndex == 2)
        {
            foreach (CardController _card in _player1Cards)
            {
                if (_card.HostedCard.Type == PlayCardType.Defense)
                {
                    GameObject _ps = Instantiate(_attackTallyPS, _card.transform.position, Quaternion.identity);
                    //particle system moves toward the tally display for effect
                    _ps.GetComponent<TallyParticleEffect>().targetPosition = _player1TotalDisplay.transform.position;
                    var mainModule = _ps.GetComponent<ParticleSystem>().main;
                    Color startColor = Color.blue;
                    startColor.a = 0.5f;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(startColor);
                }
            }
            foreach (CardController _card in _player2Cards)
            {
                if (_card.HostedCard.IsAttackingCard())
                {
                    GameObject _ps = Instantiate(_attackTallyPS, _card.transform.position, Quaternion.identity);
                    //particle system moves toward the tally display for effect
                    _ps.GetComponent<TallyParticleEffect>().targetPosition = _player2TotalDisplay.transform.position;
                    var mainModule = _ps.GetComponent<ParticleSystem>().main;
                    Color startColor = Color.red;
                    startColor.a = 0.5f;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(startColor);
                }
            }
        }
    }


    public void UpdateUI()
    {
        _index1DeckCount.text = _player1.CurrentDeck.GetDeck().Count + " Cards";
        _index2DeckCount.text = _player2.CurrentDeck.GetDeck().Count + " Cards";

        _player1Health.text = "Health " + _player1.CurrentHP.ToString();
        _player2Health.text = "Health " + _player2.CurrentHP.ToString();

        _roundCount.text = "R: " + _gameController.GetRoundNumber();
    }

    public void DisplayBattleResult(Player winner, bool currentPlayerWon)
    {
        Debug.Log("WINNER: " + winner.PlayerPepemon.DisplayName + " - " + (currentPlayerWon ? "Victory" : "Defeat"));

        // The post-battle screen animates itself in and waits on its Animator, which cannot
        // advance while a tutorial beat holds the game frozen.
        if (BotTextTutorial.Instance != null) BotTextTutorial.Instance.ForceDismiss();
        PostScreenController.LoadPepemonDisplay(ulong.Parse(winner.PlayerPepemon.ID));
        PostScreenController.SetResult(currentPlayerWon);
        PostScreenController.Show();
    }
}
