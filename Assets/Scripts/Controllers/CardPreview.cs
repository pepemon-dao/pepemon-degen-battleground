using System.Collections;
using System.Collections.Generic;
using Amazon.Lambda.Model;
using Sirenix.OdinInspector;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

/// <summary>
/// MonoBehavior for BattleCardTemplate and SupportCardTemplate
/// </summary>
public class CardPreview : MonoBehaviour
{
    [BoxGroup("Card Components"), SerializeField] public Image _cardImage;
    [BoxGroup("Card Components"), SerializeField] public TMP_Text _text;
    [BoxGroup("Card Components"), SerializeField] public GameObject _checkmark;

    [SerializeField] private GameObject defenseIcon;
    [SerializeField] private GameObject offenseIcon;

    public ulong cardId { get; private set; }
    public bool isSelected { get => transform.parent.GetComponent<SelectionGroup>().selection.Contains(GetComponent<SelectionItem>()); }

    public bool isSupport { get; private set; } = false;

    private bool isEquipped = false;

    public int previewId { get; private set; }

    public void ToggleSelected()
    {
        // setting SelectionItem.SetSelected directly would mess up the internal state of SelectionGroup
        transform.parent.GetComponent<SelectionGroup>().ToggleSelected(GetComponent<SelectionItem>());
        //GetComponentInParent<SelectionGroup>().ToggleSelected(GetComponent<SelectionItem>());
    }

    public void LoadCardData(ulong cardId, bool isSupport, int previewId = 0)
    {
        this.cardId = cardId;
        this.isSupport = isSupport;
        this.previewId = previewId;
        var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

        // set card image. blank if not found
        var tex = PepemonFactoryCardCache.GetImage(cardId);
        if (tex == null)
        {
            Debug.LogWarning("Unable to locate texture for card " + cardId);
        }

        if (metadata == null)
        {
            Debug.LogWarning("Unable to locate metadata for card " + cardId);
        }

        //bool isSupport = metadata.Value.attributes[0].value == "Pepemon Support";

        bool isOffense = false;
        bool isDefense = false;

        if (metadata != null && metadata.Value.description != null)
        {
            isOffense = metadata.Value.description.ToLower().Contains("attack");
            isDefense = metadata.Value.description.ToLower().Contains("defense");
        }

        if (defenseIcon != null)
        {
            defenseIcon.SetActive(isDefense);
        }
        if (offenseIcon != null)
        {
            offenseIcon.SetActive(isOffense);
        }

        Texture2D cardTexture = tex == null ? new Texture2D(8, 8) : tex;
        _cardImage.sprite = Sprite.Create(
            cardTexture,
            new Rect(0, 0, cardTexture.width, cardTexture.height),
            new Vector2());

        _text.text = metadata?.name ?? "Unknown Card";
    }

    public void ToggleEquipped()
    {
        _checkmark.SetActive(!_checkmark.activeSelf);
        isEquipped = _checkmark.activeSelf;
        SetEquipped(isEquipped);
    }

    public void SetEquip(bool toEquip)
    {
        _checkmark.SetActive(toEquip);
    }

    private void SetEquipped(bool toEquip)
    {
        int previewId = gameObject.GetInstanceID();
        if (isSupport)
        {
            DeckDisplay.Instance.SetCardEquip(toEquip, cardId, previewId, DeckDisplay.CardType.Support);
        }
        else
        {
            DeckDisplay.Instance.SetCardEquip(toEquip, cardId, previewId, DeckDisplay.CardType.Battle);
        }
    }
}
