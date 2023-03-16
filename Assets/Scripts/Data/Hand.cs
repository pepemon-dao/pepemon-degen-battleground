using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;

// Represents the current players hand
[System.Serializable]
public class Hand
{
    [ShowInInspector]
    private List<Card> _cardsInHand = new List<Card>();
    private List<Card> _tableSupportCards = new List<Card>();

    public List<Card> AllOffenseCards => _cardsInHand.Where(x => x.Type == PlayCardType.Offense).ToList();
    public List<Card> AllDefenseCards => _cardsInHand.Where(x => x.Type == PlayCardType.Defense).ToList();
    public List<Card> AllSpecialOffenseCards => _cardsInHand.Where(x => x.Type == PlayCardType.SpecialOffense).ToList();
    public List<Card> AllSpecialDefenseCards => _cardsInHand.Where(x => x.Type == PlayCardType.SpecialDefense).ToList();

    public void ClearHand() => _cardsInHand.Clear();
    public void AddCardToHand(Card card) => _cardsInHand.Add(card);
    public List<Card> GetCardsInHand => _cardsInHand;
    public List<Card> GetTableSupportCards => _tableSupportCards;
    public void AddCardToTable(Card card) => _tableSupportCards.Add(card);
    public void RemoveCardFromTable(Card card) => _tableSupportCards.Remove(card);


    public void RemoveAllOffenseCards()
    {
        foreach (var card in AllOffenseCards) _cardsInHand.Remove(card);
        foreach (var card in AllSpecialOffenseCards) _cardsInHand.Remove(card);
    }

    public void RemoveAllDefenseCards()
    {
        foreach (var card in AllDefenseCards) _cardsInHand.Remove(card);
        foreach (var card in AllSpecialDefenseCards) _cardsInHand.Remove(card);
    }

    public int GetTotalAttackPower(int baseAttack)
    {
        int totalAP = baseAttack;
        for (int i = 0; i < AllOffenseCards.Count; i++) totalAP += AllOffenseCards[i].AttackPower;
        for (int i = 0; i < AllSpecialOffenseCards.Count; i++) totalAP += AllSpecialOffenseCards[i].AttackPower;
        return totalAP;
    }

    public int GetTotalDefensePower(int baseDefense)
    {
        int totalDP = baseDefense;
        for (int i = 0; i < AllDefenseCards.Count; i++) totalDP += AllDefenseCards[i].DefensePower;
        for (int i = 0; i < AllSpecialDefenseCards.Count; i++) totalDP += AllSpecialDefenseCards[i].DefensePower;
        return totalDP;
    }
}