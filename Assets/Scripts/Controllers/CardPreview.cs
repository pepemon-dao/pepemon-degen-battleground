using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

/// <summary>
/// MonoBehavior for BattleCardTemplate and SupportCardTemplate
/// </summary>
public class CardPreview : MonoBehaviour
{
    [BoxGroup("Card Components"), SerializeField] public Image _cardImage;
    [BoxGroup("Card Components"), SerializeField] public Text _text;
    [BoxGroup("Card Components"), SerializeField] public Text _checkmark;
    public ulong cardId { get; private set; }

    public void SetSelectionState(bool enabled)
    {
        _cardImage.color = new Color(1, 1, 1, enabled ? 1f : 0.3f);
        GetComponent<Button>().enabled = enabled;
    }

    public void LoadCardData(ulong cardId)
    {
        this.cardId = cardId;
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

        _cardImage.GetComponent<Image>().sprite = Sprite.Create(
            tex != null ? tex : new Texture2D(8, 8),
            new Rect(0, 0, tex.width, tex.height),
            new Vector2());

        _text.GetComponent<Text>().text = metadata?.name ?? "Unknown Card";
    }
}
