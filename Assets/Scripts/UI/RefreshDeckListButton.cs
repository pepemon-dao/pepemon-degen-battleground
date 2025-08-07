using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RefreshDeckListButton : MonoBehaviour
{
    [SerializeField] DeckListLoader loader;
    
    void Start()
    {
        // Use Start() instead of Awake() to ensure Web3/socket systems are initialized
        GetComponent<Button>().onClick.AddListener(() => loader?.ReloadAllDecks(true));
    }
}
