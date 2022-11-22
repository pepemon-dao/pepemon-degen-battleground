using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

public class BattleCardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardList;
    [TitleGroup("Component References"), SerializeField] GameObject _saveDeckButtonHandler;

    public void ReloadAllBattleCards(List<ulong> availableCardIds, List<ulong> unavailableCardIds, ulong selectedBattleCard)
    {
        // destroy before re-creating
        foreach (var battlecard in _battleCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        var battleCardAttribute = new CardAttribute { value = "Pepemon Battle", trait_type = "Set" };
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

            // skip support cards
            if (!metadata.attributes.Contains(battleCardAttribute))
            {
                continue;
            }

            var battleCardInstance = Instantiate(_battleCardPrefab);
            battleCardInstance.transform.SetParent(_battleCardList.transform, false);
            battleCardInstance.GetComponent<CardPreview>().LoadCardData(cardId);

            var isSelected = selectedBattleCard == cardId;

            // set checkmark
            if (isSelected)
            {
                // has to be done this way, otherwise (ie. using SelectionItem.SetSelected)
                // the internal state of SelectionGroup would not be correct
                battleCardInstance.GetComponentInParent<SelectionGroup>().ToggleSelected(
                    battleCardInstance.GetComponent<SelectionItem>());
            }

            // gray out unavailable cards, unless it is from current deck
            if (unavailableCardIds.Contains(cardId) && !isSelected)
            {
                battleCardInstance.GetComponent<CardPreview>().SetSelectionState(false);
            }

            // Only add this listener after setting it selected
            battleCardInstance.GetComponent<SelectionItem>().onSelected.AddListener(
                delegate {
                    saveDeckButtonHandler.setBattleCard(battleCardInstance.GetComponent<CardPreview>().cardId);
                });

            /*
            // not possible at the moment
            battleCardInstance.GetComponent<SelectionItem>().onDeselected.AddListener(
                delegate {
                    if (battleCardInstance.GetComponent<CardPreview>().cardId == saveDeckButtonHandler.oldBattleCard)
                    {
                        saveDeckButtonHandler.setBattleCard(0);
                    }
                });
            */
        }
    }
}
