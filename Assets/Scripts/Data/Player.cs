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


    public void Initialise()
    {
        CurrentHP = PlayerPepemon.HealthPoints;
    }

    public void GetAndShuffelDeck(int seed)
    {
        // Get local copy of deck and shuffle
        CurrentDeck.ClearDeck();
        CurrentDeck.GetDeck().AddRange(PlayerDeck.GetDeck());
        CurrentDeck.ShuffelDeck(seed);
    }

    public void DrawNewHand()
    {
        // Clear previous hand
        CurrentHand.ClearHand();

        // Draw cards to hand
        List<Card> cacheList = new List<Card>();
        for (int i = 0; i < PlayerPepemon.Intelligence; i++)
        {
            if (i >= 0 && i < CurrentDeck.GetDeck().Count)
            {
                CurrentHand.AddCardToHand(CurrentDeck.GetDeck()[i]);
                cacheList.Add(CurrentDeck.GetDeck()[i]);
            }
        }

        // Cleanup working decks
        foreach (var item in cacheList) CurrentDeck.RemoveCard(item);
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
        for (int i = 0; i < 5; i++)
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
