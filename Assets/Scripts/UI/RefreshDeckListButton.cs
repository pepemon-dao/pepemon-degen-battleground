using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RefreshDeckListButton : MonoBehaviour
{
    [SerializeField] DeckListLoader loader;
    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
        loader?.ReloadAllDecks(true));
}
