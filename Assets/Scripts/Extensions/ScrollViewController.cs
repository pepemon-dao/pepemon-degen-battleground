using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private Button scrollLeftButton;
    [SerializeField] private Button scrollRightButton;

    private ButtonController rightButtonController;
    private ButtonController leftButtonController;
    private void Start()
    {
        scrollLeftButton.onClick.AddListener(ScrollLeft);
        scrollRightButton.onClick.AddListener(ScrollRight);
        rightButtonController= scrollRightButton.GetComponent<ButtonController>();
        leftButtonController = scrollLeftButton.GetComponent<ButtonController>();
    }


    void UpdateScrollButtonsInteractivity()
    {
        scrollLeftButton.interactable = scrollRect.horizontalNormalizedPosition > 0;
        scrollRightButton.interactable = scrollRect.horizontalNormalizedPosition < 1 - scrollRect.viewport.rect.width / content.rect.width;
    }
    
    

    private void ScrollLeft()
    {
        scrollRect.horizontalNormalizedPosition -= 0.1f; 
        UpdateScrollButtonsInteractivity();
    }

    private void ScrollRight()
    {
        scrollRect.horizontalNormalizedPosition += 0.1f; 
        UpdateScrollButtonsInteractivity();
    }
}