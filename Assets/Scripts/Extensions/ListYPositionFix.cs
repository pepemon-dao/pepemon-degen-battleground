using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListYPositionFix : MonoBehaviour
{
  [SerializeField] private float YPosition;
  
  private RectTransform rectTransform;


  private void Start()
  {
      rectTransform = GetComponent<RectTransform>();
      ChangePosition();
  }

  public void ChangePosition()
  {
      Vector2 newPosition = rectTransform.anchoredPosition;
      newPosition.y = YPosition;
      rectTransform.anchoredPosition = newPosition;
  }
}

