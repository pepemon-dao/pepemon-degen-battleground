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

    public void ReloadAllBattleCards(
        List<ulong> availableCardIds,
        List<ulong> unavailableCardIds, 
        ulong selectedBattleCard)
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
                // if this shows up there could be a bug in the contract
                Debug.LogWarning("Invalid card");
                continue;
            }
            var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

            // skip support cards
            if (!metadata?.attributes.Contains(battleCardAttribute) ?? false)
            {
                continue;
            }

            var battleCardInstance = Instantiate(_battleCardPrefab);
            var cardPreviewComponent = battleCardInstance.GetComponent<CardPreview>();

            battleCardInstance.transform.SetParent(_battleCardList.transform, false);
            cardPreviewComponent.LoadCardData(cardId);

            var isSelected = selectedBattleCard == cardId;

            // set checkmark
            if (isSelected)
            {
                cardPreviewComponent.ToggleSelected(updateSelectionGroup: true);
            }

            // gray out unavailable cards, unless it is from current deck
            if (unavailableCardIds.Contains(cardId) && !isSelected)
            {
                cardPreviewComponent.Enabled(false);
            }

            // Only add these listeners after ToggleSelected
            battleCardInstance.GetComponent<SelectionItem>().onSelected.AddListener(
                delegate {
                    cardPreviewComponent.ToggleSelected(updateSelectionGroup: false);
                    saveDeckButtonHandler.setBattleCard(cardPreviewComponent.cardId);
                });

            battleCardInstance.GetComponent<SelectionItem>().onDeselected.AddListener(
                delegate {
                    // TODO: fix cardPreviewComponent.isSelected which is somehow false but this IF thinks its ok (?)
                    if (cardPreviewComponent.cardId == saveDeckButtonHandler.oldBattleCard
                        && cardPreviewComponent.isSelected)
                    {
                        cardPreviewComponent.ToggleSelected(updateSelectionGroup: false);
                        saveDeckButtonHandler.setBattleCard(0);
                    }
                });
        }
    }
}
