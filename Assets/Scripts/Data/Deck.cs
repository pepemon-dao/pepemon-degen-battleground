using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Deck
{
    public List<Card> AllCards = new List<Card>();





    // This uses the Fisher-Yates shuffle algorithm to randomly sort elements.
    // It is an accurate, effective shuffling method for all array types.
    [Button()]
    public void ShuffelDeck()
    {
        System.Random _random = new System.Random();
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
}
