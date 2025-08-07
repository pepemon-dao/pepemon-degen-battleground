using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RefreshButtonHandler : MonoBehaviour
{
    void Start()
    {
        // Create a new GameObject for the button
        GameObject buttonGO = new GameObject("RefreshButton");
        buttonGO.transform.SetParent(transform, false);

        // Add Image component for the button background
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

        // Add Button component
        buttonGO.AddComponent<Button>();

        // Set the position and size of the button
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-20, -20);
        rectTransform.sizeDelta = new Vector2(160, 50);

        // Create a child GameObject for the text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        // Add TextMeshProUGUI component for the text
        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Refresh";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        // Set the text's RectTransform to fill the button
        RectTransform textRectTransform = textGO.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.sizeDelta = new Vector2(0, 0);

        // Add the onClick listener
        Button button = buttonGO.GetComponent<Button>();
        ScreenManageDecks screenManageDecks = GetComponent<ScreenManageDecks>();
        if (screenManageDecks != null)
        {
            button.onClick.AddListener(screenManageDecks.ReloadAllDecks);
        }
        else
        {
            Debug.LogError("ScreenManageDecks component not found on the same GameObject.", this);
        }
    }
}
