using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI;
using Nethereum.RLP;
using Sirenix.OdinInspector;
using UnityEngine;

// Container class for holding all of the players cards. This object is usually copied as value not reference
[System.Serializable]
public class Deck
{
    [SerializeField] private List<Card> AllCards = new List<Card>();

    public List<Card> GetDeck() => AllCards;
    public void ClearDeck() => AllCards.Clear();


    // This uses the Fisher-Yates shuffle algorithm to randomly sort elements.
    // It is an accurate, effective shuffling method for all array types.
    public void ShuffelDeck(BigInteger seed)
    {
        // calculate random seed like in solidity
        var abiEncode = new ABIEncode();

        for (int i = 0; i < AllCards.Count; i++)
        {
            var n = i + seed % (AllCards.Count - i);
            var temp = AllCards[(int)n];
            AllCards[(int)n] = AllCards[i];
            AllCards[i] = temp;

            seed = abiEncode
                .GetSha3ABIEncodedPacked(new ABIValue("uint256", seed))
                .ToBigIntegerFromRLPDecoded();
        }
    }

    // Remove a card from the deck via reference
    public void RemoveCard(Card card)
    {
        AllCards.Remove(card);
    }
}