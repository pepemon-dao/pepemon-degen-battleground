using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Player
{
    public Pepemon PlayerPepemon;
    public Deck PlayerDeck;


    // idea! we just move everything into here?
    // Cache the decks and run instances here (removes bloat from GC)
    public Deck currentDeck;
    public Hand currentHand;


#if UNITY_EDITOR
    [Button("Add All Cards")]
    public void AddAllCardsToDeck()
    {
        PlayerDeck.ClearDeck();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Card).Name);  //FindAssets uses tags check documentation for more info
        Card[] a = new Card[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Card>(path);
            PlayerDeck.GetDeck().Add(a[i]);
        }
    }
#endif
}
