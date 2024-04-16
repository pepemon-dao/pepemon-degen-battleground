using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Pepemon.Battle;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Modules;
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
    public bool isSelected { get =>  GetComponentInParent<SelectionGroup>().selection.Contains(GetComponent<SelectionItem>()); }
    public Image imagePreview;

    void Start()
    {
        imagePreview = GameObject.Find("Image Preview").GetComponent<Image>(); 
    }

   

    public void ToggleSelected()
    {
        PreviewImage();
        // setting SelectionItem.SetSelected directly would mess up the internal state of SelectionGroup
        GetComponentInParent<SelectionGroup>().ToggleSelected(GetComponent<SelectionItem>());
        
    }

    public void LoadCardData(ulong cardId)
    {
        
        this.cardId = cardId;
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
                    cardTypeText.text = "S.Offense";
                    cardTypeImage.sprite = offenseIcon;
                    break;
                case PlayCardType.Defense:
                    cardTypeText.text = "Defense";
                    cardTypeImage.sprite = defenseIcon;
                    break;
                case PlayCardType.SpecialDefense:
                    cardTypeText.text = "S.Defense";
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

    public void PreviewImage()
    {
        imagePreview = _cardImage;
    }
}
