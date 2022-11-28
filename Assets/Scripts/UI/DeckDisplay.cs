using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

public class DeckDisplay : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardList;
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardList;

    public ulong GetSelectedBattleCard()
    {
        return _battleCardList.GetComponentsInChildren<CardPreview>()?.Where(it => it.isSelected).FirstOrDefault()?.cardId ?? 0;
    }

    public void ReloadAllBattleCards(List<ulong> availableCardIds, ulong selectedBattleCard)
    {
        // destroy before re-creating
        foreach (var battlecard in _battleCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        var battleCardAttribute = new CardAttribute { value = "Pepemon Battle", trait_type = "Set" };

        foreach (var cardId in availableCardIds)
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
                cardPreviewComponent.ToggleSelected();
            }
        }
    }

    public Dictionary<ulong, int> GetSelectedSupportCards()
    {
        var result = new Dictionary<ulong, int>();
        var supportCards = _supportCardList.GetComponentsInChildren<CardPreview>();
        foreach (var supportCard in supportCards)
        {
            if (supportCard.isSelected)
            {
                result[supportCard.cardId] = result.ContainsKey(supportCard.cardId) ? result[supportCard.cardId] + 1 : 1;
            }
        }
        return result;
    }

    public void ReloadAllSupportCards(Dictionary<ulong, int> availableCardIds, Dictionary<ulong, int> selectedSupportCards)
    {
        // destroy before re-creating
        foreach (var battlecard in _supportCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        // selected cards will appear first, makes it easier to de-select them
        foreach (var cardId in selectedSupportCards.Keys)
        {
            AddSupportCard(cardId, selectedSupportCards[cardId], true);
        }

        // available cards appear after selected ones
        foreach (var cardId in availableCardIds.Keys)
        {
            AddSupportCard(cardId, availableCardIds[cardId], false);
        }
    }

    private void AddSupportCard(ulong cardId, int count, bool isSelected)
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

            // set checkmark
            if (isSelected)
            {
                cardPreviewComponent.ToggleSelected();
            }
        }
    }
}
