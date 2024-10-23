using System;
using System.Collections.Generic;
using System.Linq;
using Pepemon.Battle;
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
    [TitleGroup("Component References"), SerializeField] GameObject _heroCardList;
    [TitleGroup("Component References"), SerializeField] GameObject _myCardsList;
    [TitleGroup("Helpers"), SerializeField] FilterController _filter;

    private List<CardPreview> _cardPreviews = new List<CardPreview>();

    private ulong battleCardId = 0;

    public enum CardType
    {
        Battle,
        Support
    }

    public static DeckDisplay Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

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

    public void ClearMyCardsList()
    {
        List<Transform> children = new List<Transform>();

        foreach (Transform child in _myCardsList.transform)
        {
            children.Add(child);
        }
        
        foreach (Transform child in _battleCardList.transform)
        {
            children.Add(child);
        }
        
        foreach (Transform child in _supportCardList.transform)
        {
            children.Add(child);
        }
        
        foreach (Transform child in _heroCardList.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
        }

        foreach (var child in _cardPreviews)
        {
            Destroy(child.gameObject);
        }

        _cardPreviews = new List<CardPreview>();
    }
    /*
    public void LoadNotSelectedBattleCards(Dictionary<ulong, int> availableCardIds, int filter)
    {
        if (filter == 1 || filter == 2)
        {
            Debug.Log("battle cards filtered out");
            return;
        }

        // available cards appear after selected one
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, isSupportCard: false, filter);
        }
    }
    
    public void LoadSelectedBattleCard(ulong selectedBattleCard, int filter)
    {
        if (filter == 1 || filter == 2)
        {
            Debug.Log("battle cards filtered out");
            return;
        }

        if (selectedBattleCard != 0)
        {
            // selected card will appear first, makes it easier to de-select it
            AddCard(selectedBattleCard, 1, true, isSupportCard: false, filter);
        }
    }*/

    public void LoadAllBattleCards(Dictionary<ulong, int> availableCardIds, ulong selectedBattleCard, int filter)
    {
        if (filter == 1 || filter == 2)
        {
            Debug.Log("battle cards filtered out");
            return;
        }

        if (selectedBattleCard != 0)
        {
            // selected card will appear first, makes it easier to de-select it
            AddCard(selectedBattleCard, 1, true, isSupportCard: false, filter);
        }

        // available cards appear after selected one
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, isSupportCard: false, filter);
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

    public void LoadAllSupportCards(Dictionary<ulong, int> availableCardIds, Dictionary<ulong, int> selectedSupportCards, int filter)
    {
        if (filter == 3)
        {
            Debug.Log("support cards filtered out");
            return;
        }

        // selected cards will appear first, makes it easier to de-select them
        foreach (var cardId in selectedSupportCards.Keys)
        {
            AddCard(cardId, selectedSupportCards[cardId], true, true, filter);
        }

        // available cards appear after selected ones
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, true, filter);
        }
    }
    /*
    public void LoadSelectedSupportCards(Dictionary<ulong, int> selectedSupportCards, int filter)
    {
        if (filter == 3)
        {
            Debug.Log("support cards filtered out");
            return;
        }

        // selected cards will appear first, makes it easier to de-select them
        foreach (var cardId in selectedSupportCards.Keys)
        {
            AddCard(cardId, selectedSupportCards[cardId], true, true, filter);
        }
    }
    
    public void LoadNotSelectedSupportCards(Dictionary<ulong, int> availableCardIds, int filter)
    {
        if (filter == 3)
        {
            Debug.Log("support cards filtered out");
            return;
        }

        // available cards appear after selected ones
        foreach (var cardId in availableCardIds.Keys)
        {
            AddCard(cardId, availableCardIds[cardId], false, true, filter);
        }
    }*/

    public void SetCardEquip(bool toEquip, ulong cardId, CardType type)
    {
        switch (type)
        {
            case CardType.Battle:
                if (toEquip)
                {
                    EquipCard(cardId, false);
                }
                else
                {
                    UnEquipCard(cardId, false);
                }
                break;
            case CardType.Support:
                if (toEquip)
                {
                    EquipCard(cardId, true);
                }
                else
                {
                    UnEquipCard(cardId, true);
                }
                break;
            default:
                break;
        }
    }

    public void UnEquipCard(ulong cardId, bool isSupportCard)
    {
        if (!isSupportCard)
        {
            battleCardId = 0;
        }

        RemoveCardFromDeck(cardId);
    }

    public void EquipCard(ulong cardId, bool isSupportCard)
    {
        if (!isSupportCard)
        {
            DeselectPrevBattleCard();
            RemoveBattleCard();
        }

        AddCardToDeck(cardId, isSupportCard);
    }

    private void DeselectPrevBattleCard()
    {
        foreach (var item in FindObjectsOfType<SelectionItem>())
        {
            var cardPreview = item.GetComponent<CardPreview>();
            bool isTheMyCardsListAndIsInteractable = cardPreview.GetComponent<Button>().enabled;
            if (cardPreview != null && isTheMyCardsListAndIsInteractable && cardPreview.cardId == battleCardId)
            {
                item.SetSelected(false);
                return;
            }
        }
    }


    private void RemoveCardFromDeck(ulong cardId)
    {
        var cardPreviewToRemove = _cardPreviews.FirstOrDefault(item => item.cardId == cardId);

        if (cardPreviewToRemove != null)
        {
            Destroy(cardPreviewToRemove.gameObject);

            _cardPreviews.Remove(cardPreviewToRemove);
        }
    }

    private void RemoveBattleCard()
    {
        var cardPreviewToRemove = _cardPreviews.FirstOrDefault(item => !item.isSupport);

        if (cardPreviewToRemove != null)
        {
            Destroy(cardPreviewToRemove.gameObject);

            _cardPreviews.Remove(cardPreviewToRemove);
        }
    }


    private void AddCardToDeck(ulong cardId, bool isSupportCard)
    {
        if (!isSupportCard)
        {
            battleCardId = cardId;
        }

        if (cardId == 0)
        {
            // if this shows up there could be a bug in the contract
            Debug.LogWarning("Invalid card");
            return;
        }
        var cardIsSupportCard = PepemonFactoryCardCache.GetMetadata(cardId)?.isSupportCard ?? false;

        // skip battlecards if supportCard=true and skip supportcards if supportCard=false
        if (cardIsSupportCard != isSupportCard)
        {
            return;
        }

        var prefab = _battleCardPrefab;
        GameObject uiList = null;

        var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

        bool isOffense = false;
        bool isDefense = false;

        if (metadata != null && metadata.Value.description != null)
        {
            isOffense = metadata.Value.description.ToLower().Contains("attack");
            isDefense = metadata.Value.description.ToLower().Contains("defense");
        }

        if (isOffense && isDefense)
        {
            Debug.LogError("not a valid card");
            return;
        }

        if (isSupportCard)
        {
            if (isDefense)
            {
                prefab = _supportCardPrefab;
                uiList = _supportCardList;
            }
            else
            {
                uiList = _battleCardList;
            }
        }
        else
        {
            uiList = _heroCardList;
        }

        var cardInstance = Instantiate(prefab);
        var cardPreviewComponent = cardInstance.GetComponent<CardPreview>();

        cardInstance.transform.SetParent(uiList.transform, false);
        cardPreviewComponent.LoadCardData(cardId, isSupportCard);

        cardPreviewComponent.GetComponent<Button>().enabled = false;

        _cardPreviews.Add(cardPreviewComponent);
    }

    private void AddCard(ulong cardId, int count, bool isSelected, bool isSupportCard, int filter)
    {
        if (cardId == 0)
        {
            // if this shows up there could be a bug in the contract
            Debug.LogWarning("Invalid card");
            return;
        }
        var cardIsSupportCard = PepemonFactoryCardCache.GetMetadata(cardId)?.isSupportCard ?? false;
        
        // skip battlecards if supportCard=true and skip supportcards if supportCard=false
        if (cardIsSupportCard != isSupportCard)
        {
            Debug.LogError(cardId);
            return;
        }

        var prefab = _battleCardPrefab;
        GameObject uiList = null;

        var metadata = PepemonFactoryCardCache.GetMetadata(cardId);

        bool isOffense = false; 
        bool isDefense = false;

        if (metadata != null && metadata.Value.description != null)
        {
            isOffense = metadata.Value.description.ToLower().Contains("attack");
            isDefense = metadata.Value.description.ToLower().Contains("defense");
        }

        if (isOffense && isDefense)
        {
            Debug.LogError("not a valid card");
            return;
        }
        

        if (filter == 1 && isDefense)
        {
            return; // filter out the defense cards
        }
        
        if (filter == 2 && isOffense)
        {
            return; // filter out the offense cards
        }

        if (isSupportCard)
        {
            prefab = _supportCardPrefab;
        }

        uiList = _myCardsList;

        for (int i = 0; i < count; i++)
        {
            var cardInstance = Instantiate(prefab, uiList.transform);
            var cardPreviewComponent = cardInstance.GetComponent<CardPreview>();
            cardPreviewComponent.LoadCardData(cardId, isSupportCard);

            // set checkmark
            if (isSelected)
            {
                AddCardToDeck(cardId, isSupportCard);
                cardPreviewComponent.ToggleSelected();
            }
        }

    }
}
