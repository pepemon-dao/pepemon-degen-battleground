using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

    /// <summary>
    /// Show/Hide edit/select depending on the screen
    /// </summary>
    public bool DisplayDeckEditMode
    {
        set
        {
            _editButton.gameObject.SetActive(value);
            _selectButton.gameObject.SetActive(!value);
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
    }

    void OnEditClicked()
    {
        onEditButtonClicked?.Invoke();
    }

    void OnSelectClicked()
    {
        GetComponent<SelectionItem>().ToggleSelected();
        onSelectButtonClicked?.Invoke();
    }

    public async UniTask<bool> LoadDeckInfo(ulong deckId, bool selectionMode)
    {
        Debug.Log("LoadDeckInfo of deckId " + deckId);
        var battleCard = await PepemonCardDeck.GetBattleCard(deckId);

        var metadata = PepemonFactoryCardCache.GetMetadata(battleCard);
        var supportCards = await PepemonCardDeck.GetAllSupportCards(deckId);

        // TODO: Populate wins/losses
        // TODO: Use on-chain MAX_SUPPORT_CARDS value
        _deckName.text = (metadata?.name ?? "New") + " Deck";
        _battleCard.text = metadata?.name ?? "None";
        var supportCardCount = (supportCards.Values?.Count ?? 0);
        _supportCardCount.text = supportCardCount + " / " + 60;

        if (selectionMode && (metadata?.name == null || supportCardCount == 0))
        {
            gameObject.SetActive(false);
            return false;
        }
        return true;
    }
}
