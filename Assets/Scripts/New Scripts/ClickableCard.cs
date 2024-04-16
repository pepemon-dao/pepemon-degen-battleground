using System.Collections;
using System.Collections.Generic;
using Pepemon.Battle;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableCard : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            HandleDoubleClick();
        }
    }


    private void HandleDoubleClick()
    {
        CardData cardData = GetComponent<CardData>();

        CardPreview cardPreview = GetComponent<CardPreview>();


        if (cardData.IsSupportCard)
        {
            DeckManager.Instance.AddCard(cardData.Card,cardPreview);
        }
        else
        {
            DeckManager.Instance.AddCard(cardData.BattleCard,cardPreview);
        }
        
        
        gameObject.SetActive(false);
        
    }
}