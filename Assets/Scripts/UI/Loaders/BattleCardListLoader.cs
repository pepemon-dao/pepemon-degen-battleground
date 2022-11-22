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
            var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

            // skip support cards
            if (!metadata?.attributes.Contains(battleCardAttribute) ?? false)
            {
                continue;
            }

            var battleCardInstance = Instantiate(_battleCardPrefab);
            var cardPreviewComponent = battleCardInstance.GetComponent<CardPreview>();

            battleCardInstance.transform.SetParent(_battleCardList.transform, false);
            battleCardInstance.GetComponent<CardPreview>().LoadCardData(cardId);

            var isSelected = selectedBattleCard == cardId;

            // set checkmark
            if (isSelected)
            {
                cardPreviewComponent.ToggleSelected();
            }

            // gray out unavailable cards, unless it is from current deck
            if (unavailableCardIds.Contains(cardId) && !isSelected)
            {
                cardPreviewComponent.Enabled(false);
            }

            // Only add this listener after setting it selected
            battleCardInstance.GetComponent<SelectionItem>().onSelected.AddListener(
                delegate {
                    saveDeckButtonHandler.setBattleCard(cardPreviewComponent.cardId);
                });
        }
    }
}
