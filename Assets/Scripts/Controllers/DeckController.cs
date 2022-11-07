using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    [BoxGroup("Deck Components"), SerializeField] private Text _deckDisplayName;
    [BoxGroup("Deck Components"), SerializeField] private Image _deckFrameImage;
    [BoxGroup("Deck Components"), SerializeField] private Image _fade;
    [BoxGroup("Deck Components"), SerializeField] private Text _pencil;
    [BoxGroup("Deck Components"), SerializeField] private Text _checkmark;

    /// <summary>
    /// Show/Hide pencil+fade when selecting or editing a deck
    /// </summary>
    public bool DisplayDeckEditMode  {
        set => _fade.enabled = _pencil.enabled = value;
        get => _fade.enabled;
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        // set on unity editor, doesn't works if set here
        // GetComponent<SelectionItem>().onDeselected.AddListener(onDeselected);
        // GetComponent<SelectionItem>().onSelected.AddListener(onSelected);
    }

    void OnButtonClicked()
    {
        // if not in edit mode, then it is in selection mode
        if (!DisplayDeckEditMode)
        {
            GetComponent<SelectionItem>().ToggleSelected();
        }
    }


    public async Task LoadDeckInfo(ulong deckId)
    {
        Debug.Log("LoadDeckInfo of deckId " + deckId);
        var battleCard = await PepemonCardDeck.GetBattleCard(deckId);

        var metadata = PepemonFactoryCardCache.GetMetadata(battleCard);

        _deckDisplayName.text = metadata?.name == null ? "New Deck" : metadata?.name + " Deck";
    }
}
