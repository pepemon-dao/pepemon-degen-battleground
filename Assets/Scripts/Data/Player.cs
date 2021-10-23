using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Player
{
    [BoxGroup("Player Loadout")] public Pepemon PlayerPepemon;
    [BoxGroup("Player Loadout")] public Deck PlayerDeck;

    [BoxGroup("Runtime")] public int CurrentHP;
    [BoxGroup("Runtime")] public Deck CurrentDeck;
    [BoxGroup("Runtime")] public Hand CurrentHand;


    public void Initialise(int seed)
    {
        CurrentHP = PlayerPepemon.HealthPoints;

        // Get local copy of deck and shuffle
        CurrentDeck.GetDeck().AddRange(PlayerDeck.GetDeck());
        CurrentDeck.ShuffelDeck(seed);
    }

    public void DrawNewHand()
    {
        // Clear previous hand
        CurrentHand.ClearHand();

        // Draw cards to hand
        List<Card> cacheList1 = new List<Card>();
        for (int i = 0; i < PlayerPepemon.Intelligence; i++)
        {
            CurrentHand.AddCardToHand(CurrentDeck.GetDeck()[i]);
            cacheList1.Add(CurrentDeck.GetDeck()[i]);
        }

        // Cleanup working decks
        foreach (var item in cacheList1) CurrentDeck.RemoveCard(item);
    }


    // editor only func
    public void Reset()
    {
        CurrentHand.ClearHand();
        CurrentDeck.ClearDeck();
        CurrentHP = 0;
    }


#if UNITY_EDITOR
    [Button("Add All Cards")]
    public void AddAllCardsToDeck()
    {
        PlayerDeck.ClearDeck();
        // All all cards to deck 4x
        for (int i = 0; i < 4; i++)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Card).Name);  //FindAssets uses tags check documentation for more info
            Card[] a = new Card[guids.Length];
            for (int j = 0; j < guids.Length; j++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[j]);
                a[j] = UnityEditor.AssetDatabase.LoadAssetAtPath<Card>(path);
                PlayerDeck.GetDeck().Add(a[j]);
            }
        }
    }
#endif
}
