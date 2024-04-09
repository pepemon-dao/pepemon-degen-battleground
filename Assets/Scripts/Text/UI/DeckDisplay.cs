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

    public void ClearBattleCardsList()
    {
        foreach (var battlecard in _battleCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }
    }

    public void ClearSupportCardsList()
    {
        foreach (var battlecard in _supportCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }
    }

    public void LoadAllBattleCards(Dictionary<ulong, int> availableCardIds, ulong selectedBattleCard)
    {
        if (selectedBattleCard != 0)
        {
            // selected card will appear first, makes it easier to de-select it
            AddCard(selectedBattleCard, 1, true, isSupportCard: false);
        }

        // available cards appear after selected one
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, isSupportCard: false);
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

    public void LoadAllSupportCards(Dictionary<ulong, int> availableCardIds, Dictionary<ulong, int> selectedSupportCards)
    {
        // selected cards will appear first, makes it easier to de-select them
        foreach (var cardId in selectedSupportCards.Keys)
        {
            AddCard(cardId, selectedSupportCards[cardId], true, true);
        }

        // available cards appear after selected ones
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, true);
        }
    }

    private void AddCard(ulong cardId, int count, bool isSelected, bool isSupportCard)
    {
        if (cardId == 0)
        {
            // if this shows up there could be a bug in the contract
            Debug.LogWarning("Invalid card");
            return;
        }
        var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

        var cardAttribute = new CardAttribute 
        { 
            value = isSupportCard ? "Pepemon Support" : "Pepemon Battle",
            trait_type = "Set" 
        };

        // skip battlecards if isSupportCard=true and skip supportcards if isSupportCard=false
        if (!metadata?.attributes.Contains(cardAttribute) ?? false)
        {
            return;
        }

        var prefab = _battleCardPrefab;
        var uiList = _battleCardList;
        if(isSupportCard)
        {
            prefab = _supportCardPrefab;
            uiList = _supportCardList;
        }

        for (int i = 0; i < count; i++)
        {
<<<<<<< Updated upstream:Assets/Scripts/UI/DeckDisplay.cs
            var cardInstance = Instantiate(prefab);
            var cardPreviewComponent = cardInstance.GetComponent<CardPreview>();

            cardInstance.transform.SetParent(uiList.transform, false);
            cardPreviewComponent.LoadCardData(cardId);

            // set checkmark
            if (isSelected)
            {
                cardPreviewComponent.ToggleSelected();
            }
=======
            
                var cardInstance = Instantiate(prefab);
                var cardPreviewComponent = cardInstance.GetComponent<CardPreview>();

                cardInstance.transform.SetParent(uiList.transform, false);
                
                CardData cardData = cardInstance.GetComponent<CardData>();

                if (isSupportCard)
                {
                    cardData.Card = DeckManager.Instance.CardList.GetCardById(cardId);
                    cardData.IsSupportCard = true;
                }
                else
                {
                    cardData.BattleCard = DeckManager.Instance.CardList.GetPepemonById(cardId.ToString());
                    cardData.IsSupportCard = false;
                }
                
            
                
                cardPreviewComponent.LoadCardData(cardId);

                cardInstance.GetComponent<CardPreview>().card = DeckManager.Instance.CardList.GetCardById(cardId);

               

                // set checkmark
                if (isSelected)
                {
                    cardPreviewComponent.ToggleSelected();
                }
            
>>>>>>> Stashed changes:Assets/Scripts/Text/UI/DeckDisplay.cs
        }
    }
}
