using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RefreshDeckListButton : MonoBehaviour
{
    [SerializeField] DeckListLoader loader;
    private Button button;
    
    void Start()
    {
        // Use Start() instead of Awake() to ensure Web3/socket systems are initialized
        // Add comprehensive safety checks for WebGL compatibility
        InitializeButton();
    }
    
    private void InitializeButton()
    {
        try
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"RefreshDeckListButton: No Button component found on {gameObject.name}");
                return;
            }
            
            // Add click listener with comprehensive error handling
            button.onClick.AddListener(OnRefreshButtonClicked);
            Debug.Log($"RefreshDeckListButton: Successfully initialized on {gameObject.name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"RefreshDeckListButton: Initialization failed on {gameObject.name}: {ex.Message}");
        }
    }
    
    private void OnRefreshButtonClicked()
    {
        try
        {
            if (loader == null)
            {
                Debug.LogWarning($"RefreshDeckListButton: loader is null on {gameObject.name}");
                return;
            }
            
            if (loader.gameObject == null)
            {
                Debug.LogWarning($"RefreshDeckListButton: loader.gameObject is null on {gameObject.name}");
                return;
            }
            
            Debug.Log($"RefreshDeckListButton: Triggering force reload from {gameObject.name}");
            loader.ReloadAllDecks(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"RefreshDeckListButton: Click handler failed on {gameObject.name}: {ex.Message}");
        }
    }
    
    void OnDestroy()
    {
        // Clean up event listeners to prevent memory leaks in WebGL
        if (button != null)
        {
            button.onClick.RemoveListener(OnRefreshButtonClicked);
        }
    }
}
