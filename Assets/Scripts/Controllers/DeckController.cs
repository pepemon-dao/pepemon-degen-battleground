using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pepemon.Battle;
using Sirenix.OdinInspector;
using TMPro;
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

    public UnityEvent onSelectButtonClicked;
    public UnityEvent onEditButtonClicked;

    [Header("Starter Deck")]
    [SerializeField] private bool isStarterDeck = false;
    [SerializeField] private ulong starterDeckId = 10001; //making it a huge number to a normal deck id would never reach it
    [SerializeField] private BattlePrepController _battlePrepController;
    [SerializeField] private List<Card> starterDeck;

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
        var supportCardCount = (supportCards.Values?.Count ?? 0);
        _supportCardCount.text = supportCardCount + " / " + 60;

        if (isStarterDeck)
        {
            _supportCardCount.text = starterDeck.Count.ToString();
        }

        //TODO: check if the deck is valid - disable select button, only edit and add a flag that the deck needs to be edited
        if (!isStarterDeck)
        {
            if (selectionMode && (metadata?.name == null || supportCardCount == 0))
            {
                gameObject.SetActive(false);
                return false;
            }
        }
        
        return true;
    }
}
