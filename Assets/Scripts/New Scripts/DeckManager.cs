using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Nethereum.RPC.DebugNode;
using Org.BouncyCastle.Crypto.Engines;
using Pepemon.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; set; }

    [SerializeField] private Transform offenseCardList;
    [SerializeField] private Transform supportCardList;
    [SerializeField] private Transform ownedCardList;


    [SerializeField] private GameObject battleCardPreview;
    [SerializeField] private GameObject supportCardPreview;

    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private DataContainer cardList;

    [SerializeField] private Image deckCardImage;

    [SerializeField] private TextMeshProUGUI cardsInDeckText;

    [Header("No Cards Variables")] [SerializeField]
    private GameObject ownedCardsButtons;

    [SerializeField] private GameObject offenseCardsButtons;
    [SerializeField] private GameObject defenseCardsButtons;
    [SerializeField] private GameObject noOwnedCardsGameObject;
    [SerializeField] private GameObject noOffenseCardsGameObject;
    [SerializeField] private GameObject noDefenseCardsGameObject;


    private Dictionary<ulong, int> oldDeckCards = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> newDeckCards = new Dictionary<ulong, int>();

    private ulong selectedDeckBattlecardID;

    private GridLayoutGroup ownedCardsGridLayout;


    private int numberOfCardsInDeck = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    private void Start()
    {
        numberOfCardsInDeck = 0;
    }


    private void Update()
    {
        if (ownedCardList.childCount == 0)
        {
            ShowNoOwnedCardsText();
        }
        else
        {
            HideNoOwnedCardsText();
        }


        if (GetNumberOfOffensiveCardsInDeck() == 0)
        {
            ShowNoOffenseCardsText();
        }
        else
        {
            HideNoOffensiveCardsText();
        }


        if (GetNumberOfSupportCardsInDeck() == 0)
        {
            ShowNoDefensiveCardsText();
        }
        else
        {
            HideNoDefensiveCardsText();
        }
    }


    public void AddCard(Card card, CardPreview cardPreview)
    {
        if (card == null)
            return;


        if (newDeckCards.ContainsKey((ulong)card.ID))
        {
            newDeckCards[(ulong)card.ID] += 1;
        }
        else
        {
            newDeckCards.Add((ulong)card.ID, 1);
        }


        if (card.Type == PlayCardType.Offense || card.Type == PlayCardType.SpecialOffense)
        {
            GameObject newCard = Instantiate(supportCardPreview, offenseCardList);

            CardPreview newCardPreview = newCard.GetComponent<CardPreview>();

            CardData cardData = newCard.GetComponent<CardData>();

            ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

            clickableCard.enabled = false;

            newCardPreview.id = cardPreview.id;
            newCardPreview.card = cardPreview.card;

            cardData.Card = card;
            cardData.IsSupportCard = true;

            newCardPreview.LoadCardData(cardPreview.id);

            UpdateOffenseSlots();
        }
        else
        {
            GameObject newCard = Instantiate(supportCardPreview, supportCardList);

            CardPreview newCardPreview = newCard.GetComponent<CardPreview>();

            CardData cardData = newCard.GetComponent<CardData>();

            ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

            clickableCard.enabled = false;

            newCardPreview.id = cardPreview.id;
            newCardPreview.card = cardPreview.card;
            cardData.Card = card;
            cardData.IsSupportCard = true;

            newCardPreview.LoadCardData(cardPreview.id);

            UpdateSupportSlots();
        }

        numberOfCardsInDeck++;
        cardsInDeckText.text = "Cards in deck : " + numberOfCardsInDeck;
    }

    public void AddCard(BattleCard battleCard, CardPreview cardPreview)
    {
        if (battleCard == null)
            return;

        if (!battleCard.ID.Equals(selectedDeckBattlecardID.ToString()))
        {
            if (deckCardImage.sprite != null)
                RemoveBattleCard();

            selectedDeckBattlecardID = Convert.ToUInt64(battleCard.ID);

            GameManager.Instance.SelectedBattleCard = Convert.ToUInt64(battleCard.ID);

            deckCardImage.sprite = cardPreview._cardImage.sprite;
        }
    }


    public void RemoveCards()
    {
        List<Transform> cardsToRemove = new List<Transform>();

        if (offenseCardList.childCount == 0 && supportCardList.childCount == 0)
        {
            return;
        }
        else
        {
            if (offenseCardList.childCount > 0)
            {
                foreach (Transform offenseCardTransform in offenseCardList.transform)
                {
                    GameObject offenseCardGameObject = offenseCardTransform.gameObject;

                    SelectionItem selectionItem = offenseCardGameObject.GetComponent<SelectionItem>();

                    if (selectionItem == null)
                        continue;

                    if (selectionItem.isSelected)
                    {
                        cardsToRemove.Add(offenseCardTransform);

                        RestoreSupportCard(offenseCardTransform.gameObject.GetComponent<CardData>().Card.ID);
                    }
                }
            }

            if (supportCardList.childCount > 0)
            {
                foreach (Transform supportCardTransform in supportCardList.transform)
                {
                    GameObject supporteCardGameObject = supportCardTransform.gameObject;

                    SelectionItem selectionItem = supporteCardGameObject.GetComponent<SelectionItem>();

                    if (selectionItem == null)
                        continue;

                    if (selectionItem.isSelected)
                    {
                        cardsToRemove.Add(supportCardTransform);

                        RestoreSupportCard(supportCardTransform.gameObject.GetComponent<CardData>().Card.ID);
                        
                    }
                }
            }


            foreach (var cardToRemove in cardsToRemove)
            {
                CardData cardData = cardToRemove.gameObject.GetComponent<CardData>();

                if (newDeckCards.ContainsKey((ulong)cardData.Card.ID))
                {
                    int amount = newDeckCards[(ulong)cardData.Card.ID];

                    if (amount > 1)
                    {
                        newDeckCards[(ulong)cardData.Card.ID] -= 1;
                    }
                    else
                    {
                        newDeckCards.Remove((ulong)cardData.Card.ID);
                    }

                    numberOfCardsInDeck--;
                    Destroy(cardToRemove.gameObject);
                }
            }

            UpdateOffenseSlots();
            UpdateSupportSlots();
        }


        cardsInDeckText.text = "Cards in deck : " + numberOfCardsInDeck;
    }


    public void RemoveBattleCard()
    {
        deckCardImage.sprite = null;

        foreach (Transform ownedCard in ownedCardList.transform)
        {
            CardData ownedCardData = ownedCard.gameObject.GetComponent<CardData>();

            if ((ownedCardData.BattleCard.ID == selectedDeckBattlecardID.ToString()))
            {
                ownedCard.gameObject.SetActive(true);

                break;
            }
        }

        selectedDeckBattlecardID = 999999;
    }

    public void RestoreSupportCard(int cardId)
    {
        foreach (Transform ownedCard in ownedCardList.transform)
        {
            CardData offenseCardData = ownedCard.gameObject.GetComponent<CardData>();

            if (offenseCardData.IsSupportCard && (offenseCardData.Card.ID == cardId))
            {
                ownedCard.gameObject.SetActive(true);
                return;
            }
        }
    }

    public void LoadDeckCards()
    {
        ClearCards();

        foreach (var oldCard in oldDeckCards)
        {
            Card card = cardList.GetCardById(oldCard.Key);

            for (int i = 0; i < oldCard.Value; i++)
            {
                numberOfCardsInDeck++;

                if (card.Type == PlayCardType.Offense || card.Type == PlayCardType.SpecialOffense)
                {
                    GameObject newCard = Instantiate(supportCardPreview, offenseCardList);

                    ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

                    CardPreview cardPreview = newCard.GetComponent<CardPreview>();

                    CardData cardData = newCard.GetComponent<CardData>();

                    cardData.Card = card;
                    cardData.IsSupportCard = true;

                    cardPreview.LoadCardData((ulong)card.ID);

                    clickableCard.enabled = false;

                    UpdateOffenseSlots();
                }
                else
                {
                    GameObject newCard = Instantiate(supportCardPreview, supportCardList);
                    ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

                    CardData cardData = newCard.GetComponent<CardData>();


                    CardPreview cardPreview = newCard.GetComponent<CardPreview>();

                    cardData.Card = card;
                    cardData.IsSupportCard = true;

                    cardPreview.LoadCardData((ulong)card.ID);

                    clickableCard.enabled = false;

                    UpdateSupportSlots();
                }


                cardsInDeckText.text = "Cards in deck : " + numberOfCardsInDeck;
            }

            GameManager.Instance.SelectedBattleCard = selectedDeckBattlecardID;

            foreach (Transform ownedCard in ownedCardList.transform)
            {
                CardData ownedCardData = ownedCard.gameObject.GetComponent<CardData>();

                if ((!ownedCardData.IsSupportCard) &&
                    (ownedCardData.BattleCard.ID == selectedDeckBattlecardID.ToString()))
                {
                    CardPreview battleCardPreviewComponent = ownedCard.GetComponent<CardPreview>();

                    deckCardImage.sprite = battleCardPreviewComponent._cardImage.sprite;

                    ownedCard.gameObject.SetActive(false);

                    break;
                }
            }
        }

        UncheckSelectedCards();
    }


    private void UncheckSelectedCards()
    {
        if (ownedCardList.childCount == 0)
            return;

        foreach (Transform ownedCard in ownedCardList.transform)
        {
            CardData ownedCardData = ownedCard.gameObject.GetComponent<CardData>();

            foreach (var card in newDeckCards)
            {
                if (ownedCardData.IsSupportCard)
                {
                    if (card.Key == (ulong)ownedCardData.Card.ID)
                    {
                        ownedCard.gameObject.SetActive(false);
                    }
                }
            }
        }
    }


    public void ClearCards()
    {
        ClearOffensiveCards();
        ClearSupportCards();

        ClearSupportSlots();
        ClearOffenseSlots();

        numberOfCardsInDeck = 0;
        cardsInDeckText.text = "Cards in deck : " + numberOfCardsInDeck;

        deckCardImage.sprite = null;
    }

    private void ClearOffensiveCards()
    {
        if (offenseCardList.childCount > 0)
        {
            foreach (Transform child in offenseCardList.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }


    private void ClearSupportCards()
    {
        if (supportCardList.childCount > 0)
        {
            foreach (Transform child in supportCardList.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }


    private void UpdateOffenseSlots()
    {
        ClearOffenseSlots();

        if (GetNumberOfOffensiveCardsInDeck() < 10)
        {
            SetSlots(offenseCardList, 10 - GetNumberOfOffensiveCardsInDeck());
        }
    }

    private void UpdateSupportSlots()
    {
        ClearSupportSlots();

        if (GetNumberOfSupportCardsInDeck() < 10)
        {
            SetSlots(supportCardList, 10 - GetNumberOfSupportCardsInDeck());
        }
    }


    private void ClearOffenseSlots()
    {
        if (offenseCardList.childCount > 0)
        {
            foreach (Transform child in offenseCardList.transform)
            {
                GameObject childGameobject = child.gameObject;
                CardData cardData = childGameobject.GetComponent<CardData>();
                if (cardData != null)
                {
                    if (cardData.IsSlot)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }


    private void ClearSupportSlots()
    {
        if (supportCardList.childCount > 0)
        {
            foreach (Transform child in supportCardList.transform)
            {
                GameObject childGameobject = child.gameObject;
                CardData cardData = childGameobject.GetComponent<CardData>();
                if (cardData != null)
                {
                    if (cardData.IsSlot)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }


    private void SetSlots(Transform list, int numberOfSlots)
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            Instantiate(slotPrefab, list);
        }
    }


    private void ShowNoOwnedCardsText()
    {
        ownedCardsButtons.SetActive(false);
        noOwnedCardsGameObject.SetActive(true);
    }


    private void ShowNoOffenseCardsText()
    {
        offenseCardsButtons.SetActive(false);
        noOffenseCardsGameObject.SetActive(true);
        offenseCardList.gameObject.SetActive(false);
    }


    private void ShowNoDefensiveCardsText()
    {
        defenseCardsButtons.SetActive(false);
        noDefenseCardsGameObject.SetActive(true);
        supportCardList.gameObject.SetActive(false);
    }


    private void HideNoOwnedCardsText()
    {
        ownedCardsButtons.SetActive(true);
        noOwnedCardsGameObject.SetActive(false);
    }

    private void HideNoOffensiveCardsText()
    {
        offenseCardsButtons.SetActive(true);
        noOffenseCardsGameObject.SetActive(false);
        offenseCardList.gameObject.SetActive(true);
    }

    private void HideNoDefensiveCardsText()
    {
        defenseCardsButtons.SetActive(true);
        noDefenseCardsGameObject.SetActive(false);
        supportCardList.gameObject.SetActive(true);
    }

    private int GetNumberOfSupportCardsInDeck()
    {
        int nbCards = 0;

        foreach (var card in newDeckCards)
        {
            Card supportCard = cardList.GetCardById(card.Key);
            if (supportCard.Type == PlayCardType.Defense || supportCard.Type == PlayCardType.SpecialDefense)
            {
                nbCards += card.Value;
            }
        }

        return nbCards;
    }


    private int GetNumberOfOffensiveCardsInDeck()
    {
        int nbCards = 0;

        foreach (var card in newDeckCards)
        {
            Card offenseCard = cardList.GetCardById(card.Key);
            if (offenseCard.Type == PlayCardType.Offense || offenseCard.Type == PlayCardType.SpecialOffense)
            {
                nbCards += card.Value;
            }
        }

        return nbCards;
    }


    public DataContainer CardList => cardList;


    public Dictionary<ulong, int> OldDeckCards
    {
        get => oldDeckCards;
        set => oldDeckCards = value;
    }


    public Dictionary<ulong, int> NewDeckCards
    {
        get => newDeckCards;
        set => newDeckCards = value;
    }

    public ulong SelectedDeckBattlecardID
    {
        get => selectedDeckBattlecardID;
        set => selectedDeckBattlecardID = value;
    }
}