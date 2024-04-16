using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableBattleCard : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            HandleDoubleClick();
            Debug.Log("true");
        }
    }


    private void HandleDoubleClick()
    {
       DeckManager.Instance.RemoveBattleCard(); 
    }
}
