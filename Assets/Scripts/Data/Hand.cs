using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

// Represents the current players hand
[System.Serializable]
public class Hand
{
    [ShowInInspector]
    private List<Card> _cardsInHand = new List<Card>();

    public List<Card> AllOffenseCards => _cardsInHand.Where(x => x.CType == CardType.Offense).ToList();
    public List<Card> AllDefenseCards => _cardsInHand.Where(x => x.CType == CardType.Defense).ToList();
    public List<Card> AllSpecialOffenseCards => _cardsInHand.Where(x => x.CType == CardType.SpecialOffense).ToList();
    public List<Card> AllSpecialDefenseCards => _cardsInHand.Where(x => x.CType == CardType.SpecialDefense).ToList();




    public void ClearHand() => _cardsInHand.Clear();
    public void AddCardToHand(Card card) => _cardsInHand.Add(card);


    public int GetTotalAttackPower()
    {
        int totalAP = 0;
        for (int i = 0; i < AllOffenseCards.Count; i++) totalAP += AllOffenseCards[i].AttackPower;
        for (int i = 0; i < AllSpecialOffenseCards.Count; i++) totalAP += AllSpecialOffenseCards[i].AttackPower;
        return totalAP;
    }

    public int GetTotalDefensePower()
    {
        int totalDP = 0;
        for (int i = 0; i < AllDefenseCards.Count; i++) totalDP += AllDefenseCards[i].DefensePower;
        for (int i = 0; i < AllSpecialDefenseCards.Count; i++) totalDP += AllSpecialDefenseCards[i].DefensePower;
        return totalDP;
    }
}