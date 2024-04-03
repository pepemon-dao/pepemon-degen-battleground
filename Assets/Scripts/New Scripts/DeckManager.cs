using System;
using System.Collections;
using System.Collections.Generic;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Org.BouncyCastle.Crypto.Engines;
using Pepemon.Battle;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; set; }

    [SerializeField] private Transform offenseCardList;
    [SerializeField] private Transform supportCardList;
    [SerializeField] private Transform ownedCardList;

    [SerializeField] private GameObject battleCardPreview;
    [SerializeField] private GameObject supportCardPreview;

    [SerializeField] private DataContainer cardList;

    private Dictionary<ulong, int> oldDeckCards = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> newDeckCards = new Dictionary<ulong, int>();
    
    private GridLayoutGroup ownedCardsGridLayout;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
            newCardPreview.LoadCardData(cardPreview.id);

            cardData.Card = card;
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
            newCardPreview.LoadCardData(cardPreview.id);

            cardData.Card = card;
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

                    if (selectionItem.isSelected)
                    {
                        cardsToRemove.Add(offenseCardTransform);
                        GameObject newCard = Instantiate(battleCardPreview, ownedCardList);
                        CardPreview cardPreview = newCard.GetComponent<CardPreview>();
                        CardPreview currentCardPreview = offenseCardGameObject.GetComponent<CardPreview>();

                        cardPreview.id = currentCardPreview.id;
                        cardPreview.card = currentCardPreview.card;
                        cardPreview.LoadCardData(currentCardPreview.id);
                    }
                }
            }

            if (supportCardList.childCount > 0)
            {
                foreach (Transform supportCardTransform in supportCardList.transform)
                {
                    GameObject supporteCardGameObject = supportCardTransform.gameObject;

                    SelectionItem selectionItem = supporteCardGameObject.GetComponent<SelectionItem>();

                    if (selectionItem.isSelected)
                    {
                        cardsToRemove.Add(supportCardTransform);
                        GameObject newCard = Instantiate(battleCardPreview, ownedCardList);
                        CardPreview cardPreview = newCard.GetComponent<CardPreview>();
                        CardPreview currentCardPreview = supporteCardGameObject.GetComponent<CardPreview>();

                        cardPreview.id = currentCardPreview.id;
                        cardPreview.card = currentCardPreview.card;
                        cardPreview.LoadCardData(currentCardPreview.id);
                    }
                }
            }


            foreach (var cardToRemove in cardsToRemove)
            {
                CardData cardData = cardToRemove.gameObject.GetComponent<CardData>();

                Debug.Log(cardData.Card);

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


                    Destroy(cardToRemove.gameObject);
                }
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
                if (card.Type == PlayCardType.Offense || card.Type == PlayCardType.SpecialOffense)
                {
                    GameObject newCard = Instantiate(supportCardPreview, offenseCardList);

                    ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

                    CardPreview cardPreview = newCard.GetComponent<CardPreview>();

                    CardData cardData = newCard.GetComponent<CardData>();

                    cardPreview.LoadCardData((ulong)card.ID);

                    clickableCard.enabled = false;

                    cardData.Card = card;
                }
                else
                {
                    GameObject newCard = Instantiate(supportCardPreview, supportCardList);
                    ClickableCard clickableCard = newCard.GetComponent<ClickableCard>();

                    CardData cardData = newCard.GetComponent<CardData>();


                    CardPreview cardPreview = newCard.GetComponent<CardPreview>();

                    cardPreview.LoadCardData((ulong)card.ID);

                    clickableCard.enabled = false;

                    cardData.Card = card;
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
                if (card.Key == (ulong)ownedCardData.Card.ID)
                {
                    ownedCard.gameObject.SetActive(false);
                }
            }
        }
    }


    public void ClearCards()
    {
        ClearOffensiveCards();
        ClearSupportCards();
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
}