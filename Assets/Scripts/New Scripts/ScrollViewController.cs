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

    private float scrollSpeed = 0.15f;

    private void Start()
    {
        scrollLeftButton.onClick.AddListener(ScrollLeft);
        scrollRightButton.onClick.AddListener(ScrollRight);
    }

    private void UpdateButtonInteractivity()
    {
        float contentLeftEdge = content.localPosition.x;
        float contentRightEdge = contentLeftEdge + content.rect.width;
        float viewportLeftEdge = -scrollRect.viewport.rect.width / 2;
        float viewportRightEdge = scrollRect.viewport.rect.width / 2;

        scrollLeftButton.interactable = contentLeftEdge < viewportLeftEdge;
        scrollRightButton.interactable = contentRightEdge > viewportRightEdge;
    }

    private void ScrollLeft()
    {
        scrollRect.horizontalNormalizedPosition -= scrollSpeed;
        UpdateButtonInteractivity();
    }

    private void ScrollRight()
    {
        scrollRect.horizontalNormalizedPosition += scrollSpeed;
        UpdateButtonInteractivity();
    }
}