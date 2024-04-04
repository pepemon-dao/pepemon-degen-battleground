using System.Collections;
using System.Collections.Generic;
<<<<<<< Updated upstream
=======
using System.Diagnostics;
using Pepemon.Battle;
>>>>>>> Stashed changes
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;
using Debug = UnityEngine.Debug;

/// <summary>
/// MonoBehavior for BattleCardTemplate and SupportCardTemplate
/// </summary>
public class CardPreview : MonoBehaviour
{
    [BoxGroup("Card Components"), SerializeField] public Image _cardImage;
    [BoxGroup("Card Components"), SerializeField] public TMP_Text _text;
    [BoxGroup("Card Components"), SerializeField] public Text _checkmark;
<<<<<<< Updated upstream
    public ulong cardId { get; private set; }
=======
    [BoxGroup("Card Components"), SerializeField] public TMP_Text cardTypeText;
    [BoxGroup("Card Components"), SerializeField] public Image cardTypeImage;
    [Header("Icons")]
    [BoxGroup("Card Components"), SerializeField] public Sprite offenseIcon;
    [BoxGroup("Card Components"), SerializeField] public Sprite defenseIcon;
    [BoxGroup("Card Components"), SerializeField] public Sprite monsterIcon;
    
    
    public ulong cardId { get; private set; }
    
   [SerializeField] public ulong id { get;  set; }
   
   [SerializeField] public Card card { get;  set; }
   
   public BattleCard battleCard{ get;  set; }
>>>>>>> Stashed changes
    public bool isSelected { get =>  GetComponentInParent<SelectionGroup>().selection.Contains(GetComponent<SelectionItem>()); }

    public void ToggleSelected()
    {
        // setting SelectionItem.SetSelected directly would mess up the internal state of SelectionGroup
        GetComponentInParent<SelectionGroup>().ToggleSelected(GetComponent<SelectionItem>());
    }

    public void LoadCardData(ulong cardId)
    {
        this.cardId = cardId;
<<<<<<< Updated upstream
=======
        id = cardId;

        bool isSupportCard = GetComponent<CardData>().IsSupportCard;

        if (isSupportCard)
        {
            card = GetComponent<CardData>().Card;
        }
        else
        {
            battleCard = GetComponent<CardData>().BattleCard;
        }
      
        
>>>>>>> Stashed changes
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

        Texture2D cardTexture = tex == null ? new Texture2D(8, 8) : tex;
        _cardImage.sprite = Sprite.Create(
            cardTexture,
            new Rect(0, 0, cardTexture.width, cardTexture.height),
            new Vector2());

        _text.text = metadata?.name ?? "Unknown Card";

        if (isSupportCard)
        {
            switch (card.Type)
            {
                case PlayCardType.Offense:
                    cardTypeText.text = "Offense";
                    cardTypeImage.sprite = offenseIcon;
                    break;
                case PlayCardType.SpecialOffense:
                    cardTypeText.text = "SP.Offense";
                    cardTypeImage.sprite = offenseIcon;
                    break;
                case PlayCardType.Defense:
                    cardTypeText.text = "Defense";
                    cardTypeImage.sprite = defenseIcon;
                    break;
                case PlayCardType.SpecialDefense:
                    cardTypeText.text = "SP.Defense";
                    cardTypeImage.sprite = defenseIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            cardTypeText.text = "Monster";
            cardTypeImage.sprite = monsterIcon;
        }
        
      
        
    }
}
