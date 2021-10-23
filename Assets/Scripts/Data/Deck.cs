using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// Container class for holding all of the players cards. This object is usually copied as value not reference
[System.Serializable]
public class Deck
{
    [SerializeField] private List<Card> AllCards = new List<Card>();

    public List<Card> GetDeck() => AllCards;
    public void ClearDeck() => AllCards.Clear();


    // This uses the Fisher-Yates shuffle algorithm to randomly sort elements.
    // It is an accurate, effective shuffling method for all array types.
    public void ShuffelDeck(int seed)
    {
        System.Random _random = new System.Random(seed);
        Card _cacheCard;

        int n = AllCards.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random()
            int r = i + (int)(_random.NextDouble() * (n - i));
            _cacheCard = AllCards[r];
            AllCards[r] = AllCards[i];
            AllCards[i] = _cacheCard;
        }
    }

    // Remove a card from the deck via reference
    public void RemoveCard(Card card)
    {
        AllCards.Remove(card);
    }
}
