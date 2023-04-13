using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TopPanelCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject hoverStats;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!hoverStats.activeInHierarchy)
        hoverStats.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverStats.SetActive(false);
    }
}
