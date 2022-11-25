using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

public class SupportCardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardList;
    [TitleGroup("Component References"), SerializeField] GameObject _saveDeckButtonHandler;

    public void ReloadAllSupportCards(
        Dictionary<ulong, int> availableCardIds,
        Dictionary<ulong, int> unavailableCardIds,
        Dictionary<ulong, int> selectedSupportCards)
    {
        // destroy before re-creating
        foreach (var battlecard in _supportCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        // selected cards will appear first, makes it easier to de-select them
        foreach (var cardId in selectedSupportCards.Keys)
        {
            addCard(cardId, selectedSupportCards[cardId], false, true);
        }

        // available cards appear after selected ones
        foreach (var cardId in availableCardIds.Keys)
        {
            addCard(cardId, availableCardIds[cardId], false, false);
        }

        // unavailable cards appear last
        foreach (var cardId in unavailableCardIds.Keys)
        {
            addCard(cardId, unavailableCardIds[cardId], true, false);
        }
    }

    private void addCard(ulong cardId, int count, bool disabled, bool isSelected)
    {
        if (cardId == 0)
        {
            // if this shows up there could be a bug in the contract
            Debug.LogWarning("Invalid card");
            return;
        }
        var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

        var supportCardAttribute = new CardAttribute { value = "Pepemon Support", trait_type = "Set" };

        // skip battle cards
        if (!metadata?.attributes.Contains(supportCardAttribute) ?? false)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var supportCardInstance = Instantiate(_supportCardPrefab);
            var cardPreviewComponent = supportCardInstance.GetComponent<CardPreview>();

            supportCardInstance.transform.SetParent(_supportCardList.transform, false);
            cardPreviewComponent.LoadCardData(cardId);

            // gray out unavailable cards, unless it is from current deck
            cardPreviewComponent.Enabled(!disabled);

            // set checkmark
            if (isSelected)
            {
                cardPreviewComponent.ToggleSelected();
            }

            // Only add these listeners after ToggleSelected
            supportCardInstance.GetComponent<SelectionItem>().onSelected.AddListener(
                delegate {
                    _saveDeckButtonHandler.GetComponent<SaveDeckButtonHandler>().addSupportCard(cardId);
                });

            supportCardInstance.GetComponent<SelectionItem>().onDeselected.AddListener(
                delegate {
                    _saveDeckButtonHandler.GetComponent<SaveDeckButtonHandler>().removeSupportCard(cardId);
                });
        }
    }
}
