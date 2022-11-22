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

    public void ReloadAllSupportCards(List<ulong> availableCardIds, List<ulong> unavailableCardIds, List<ulong> selectedSupportCards)
    {
        // destroy before re-creating
        foreach (var battlecard in _supportCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        var supportCardAttribute = new CardAttribute { value = "Pepemon Support", trait_type = "Set" };
        var saveDeckButtonHandler = _saveDeckButtonHandler.GetComponent<SaveDeckButtonHandler>();

        // add everything to the list, both available and unavailable
        foreach (var cardId in availableCardIds.Concat(unavailableCardIds))
        {
            if (cardId == 0)
            {
                Debug.LogWarning("Invalid card");
                continue;
            }
            var metadata = (CardMetadata)PepemonFactoryCardCache.GetMetadata(cardId);

            // skip battle cards
            if (!metadata.attributes.Contains(supportCardAttribute))
                continue;

            var supportCardInstance = Instantiate(_supportCardPrefab);
            supportCardInstance.transform.SetParent(_supportCardList.transform, false);
            supportCardInstance.GetComponent<CardPreview>().LoadCardData(cardId);

            var isSelected = selectedSupportCards.Contains(cardId);

            // set checkmark
            if (isSelected)
            {
                // has to be done this way, otherwise (ie. using SelectionItem.SetSelected)
                // the internal state of SelectionGroup would not be correct
                supportCardInstance.GetComponentInParent<SelectionGroup>().ToggleSelected(
                    supportCardInstance.GetComponent<SelectionItem>());
            }

            // gray out unavailable cards, unless it is from current deck
            if (unavailableCardIds.Contains(cardId) && !isSelected)
            {
                supportCardInstance.GetComponent<CardPreview>().SetSelectionState(false);
            }

            // Only add these listeners after SetSelected
            supportCardInstance.GetComponent<SelectionItem>().onSelected.AddListener(
                delegate {
                    saveDeckButtonHandler.addSupportCard(cardId);
                });

            supportCardInstance.GetComponent<SelectionItem>().onDeselected.AddListener(
                delegate {
                    saveDeckButtonHandler.removeSupportCard(cardId);
                });
        }
    }
}
