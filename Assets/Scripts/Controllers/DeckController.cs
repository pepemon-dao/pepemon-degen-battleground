using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pepemon.Battle;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    [BoxGroup("Deck Components"), SerializeField] private Image _deckFrameImage;
    [BoxGroup("Deck Components"), SerializeField] private Button _editButton;
    [BoxGroup("Deck Components"), SerializeField] private Button _selectButton;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _deckName;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _battleCard;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _supportCardCount;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _winCount;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _lossCount;
    [BoxGroup("Deck Components"), SerializeField] private GameObject _errorDisplay;
    [BoxGroup("Deck Components"), SerializeField] private TMP_Text _errorText;

    public UnityEvent onSelectButtonClicked;
    public UnityEvent onEditButtonClicked;

    [Header("Starter Deck")]
    [SerializeField] private bool isStarterDeck = false;
    [SerializeField] private ulong starterDeckId = 10001; //making it a huge number to a normal deck id would never reach it
    [SerializeField] private BattlePrepController _battlePrepController;
    [SerializeField] private List<Card> starterDeck;

    private bool notValidDeck = false;

    /// <summary>
    /// Show/Hide edit/select depending on the screen
    /// </summary>
    public bool DisplayDeckEditMode
    {
        set
        {
            _editButton.gameObject.SetActive(value);
            if (!isStarterDeck)
            {
                _selectButton.gameObject.SetActive(!value);
                if (!value)
                {
                    UpdateNotValidDeckIsNotShowWhenSelecting();
                }
            }
        }

        get => _editButton.gameObject.activeSelf;
    }

    void Start()
    {
        _editButton.onClick.AddListener(OnEditClicked);
        _selectButton.onClick.AddListener(OnSelectClicked);
        // set on unity editor, doesn't works if set here
        // GetComponent<SelectionItem>().onDeselected.AddListener(onDeselected);
        // GetComponent<SelectionItem>().onSelected.AddListener(onSelected);

        if (isStarterDeck)
        {
            int supportCardCount = 0;
            if (starterDeckId == 10001)
            {
                supportCardCount = _battlePrepController.starterDeck1.Count;
            }
            else
            {
                supportCardCount = _battlePrepController.starterDeck2.Count;
            }

            _selectButton.gameObject.SetActive(true);

            _supportCardCount.text = supportCardCount + " / " + 60;
        }
    }

    /// <summary>
    /// Keeps an unplayable deck visible in the selection list instead of removing it.
    ///
    /// This used to do gameObject.SetActive(!notValidDeck), which deleted invalid decks from
    /// the picker. A player whose decks were all incomplete - or whose valid decks were
    /// misclassified by a metadata cache miss - got an empty screen with nothing to click and
    /// no explanation of why. The deck now stays listed with its error message showing; only
    /// the Select button is withheld.
    /// </summary>
    private void UpdateNotValidDeckIsNotShowWhenSelecting()
    {
        if (!_editButton.gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            _selectButton.gameObject.SetActive(!notValidDeck);
        }
    }

    void OnEditClicked()
    {
        onEditButtonClicked?.Invoke();
    }

    public void OnSelectClicked()
    {
        if (!isStarterDeck)
        {
            GetComponent<SelectionItem>().ToggleSelected();
            onSelectButtonClicked?.Invoke();
        }
        else
        {
            _battlePrepController.OnDeckSelected(starterDeckId, true);
        }
    }

    public async UniTask<bool> LoadDeckInfo(ulong deckId, bool selectionMode)
    {
        bool isStarterDeck = deckId == 1234;

        // Example starter deck configuration
        IDictionary<ulong, int> supportCards = new Dictionary<ulong, int>();

        ulong battleCard = 0;

        if (isStarterDeck)
        {
            battleCard = DeckDisplay.battleCardId == 0 ? 7 : DeckDisplay.battleCardId;
            foreach (var card in starterDeck)
            {
                ulong id = (ulong)card.ID;
                supportCards[id] = supportCards.ContainsKey(id) ? supportCards[id] + 1 : 1;
            }
        }
        else
        {
            // Fetch the battle card from the contract if it's not a starter deck
            
            battleCard = await PepemonCardDeck.GetBattleCard(deckId);

            // Fetch support cards from the contract
            supportCards = await PepemonCardDeck.GetAllSupportCards(deckId);
        }

        var metadata = PepemonFactoryCardCache.GetMetadata(battleCard);

        // TODO: Populate wins/losses
        // TODO: Use on-chain MAX_SUPPORT_CARDS value
        _deckName.text = (metadata?.name ?? "New") + " Deck";
        _battleCard.text = metadata?.name ?? "None";
        int supportCardCount = 0;
        foreach (var card in supportCards)
        {
            supportCardCount += card.Value;
        }
        _supportCardCount.text = supportCardCount + " / " + 60;

        if (isStarterDeck)
        {
            _supportCardCount.text = starterDeck.Count.ToString();
        }

        // Validity is decided purely by what the deck contains on-chain.
        //
        // This previously keyed off metadata?.name, which comes from the card metadata cache.
        // A cache miss - which happens routinely for a freshly minted card, or any time the
        // cache has not finished loading - made a deck that was perfectly valid on-chain look
        // invalid, and invalid decks were then removed from the selection list entirely. The
        // result was an empty deck picker with nothing to select and no way to play.
        bool hasBattleCard = battleCard != 0;
        bool hasSupportCards = supportCardCount > 0;

        notValidDeck = !hasBattleCard || !hasSupportCards;

        if (!isStarterDeck)
        {
            if (!hasBattleCard)
            {
                _errorDisplay.SetActive(true);
                _errorText.text = "Pepemon card missing";
                _selectButton.gameObject.SetActive(false);
                UpdateNotValidDeckIsNotShowWhenSelecting();
                return true;
            }
            if (!hasSupportCards)
            {
                _errorDisplay.SetActive(true);
                _errorText.text = "Support cards missing";
                _selectButton.gameObject.SetActive(false);
                UpdateNotValidDeckIsNotShowWhenSelecting();
                return true;
            }
            _selectButton.gameObject.SetActive(selectionMode);
            if (selectionMode)
            {
                UpdateNotValidDeckIsNotShowWhenSelecting();
            }
            _errorDisplay.SetActive(false);
        }
        
        return true;
    }
}
