using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MintDeckButtonHandler : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    async void OnButtonClicked()
    {
        GetComponent<Button>().interactable = false;
        var tx = await PepemonCardDeck.CreateDeck();
        // TODO: show error label on failure
        if (string.IsNullOrEmpty(tx))
        {
            // TODO: enum screens
            FindObjectOfType<MainMenuController>().ShowScreen(5);
        }
        GetComponent<Button>().interactable = true;
    }
}
