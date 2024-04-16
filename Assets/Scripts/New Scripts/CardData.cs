using System;
using System.Collections;
using System.Collections.Generic;
using Pepemon.Battle;
using UnityEngine;

[Serializable]
public class CardData : MonoBehaviour
{
   [SerializeField] private Card card;
   [SerializeField] private BattleCard battleCard;
   [SerializeField] private bool isSupportCard;
   [SerializeField] private bool isSlot;

   public Card Card
   {
      get => card;
      set => card = value;
   }

   public BattleCard BattleCard
   {
      get => battleCard;
      set => battleCard = value;
   }

   public bool IsSupportCard
   {
      get => isSupportCard;
      set => isSupportCard = value;
   }


   public bool IsSlot
   {
      get => isSlot;
      set => isSlot = value;
   }
}
