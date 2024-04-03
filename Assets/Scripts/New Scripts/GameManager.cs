using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }
    
    private Dictionary<ulong, int> selectedDeck = new Dictionary<ulong, int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        DontDestroyOnLoad(this);
    }


    public Dictionary<ulong, int> SelectedDeck
    {
        get => selectedDeck;
        set => selectedDeck = value;
    }
}
