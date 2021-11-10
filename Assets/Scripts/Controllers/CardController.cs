using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

// Handles the instance of a card within a session
public class CardController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _cardDisplayName;

    Card _card;

    public void PouplateCard(Card card)
    {
        _card = card;
        _cardDisplayName.text = _card.DisplayName;
    }
}