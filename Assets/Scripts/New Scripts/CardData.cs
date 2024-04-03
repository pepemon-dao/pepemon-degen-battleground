using System;
using System.Collections;
using System.Collections.Generic;
using Pepemon.Battle;
using UnityEngine;

[Serializable]
public class CardData : MonoBehaviour
{
   [SerializeField] private Card card;

   public Card Card
   {
      get => card;
      set => card = value;
   }
}
