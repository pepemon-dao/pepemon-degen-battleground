using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

// Handles the instance of a card within a session
public class CardController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _cardDisplayName;

    public Image colorCircle;

    [ReadOnly] public Card HostedCard;

    public void PouplateCard(Card card)
    {
        HostedCard = card;
        _cardDisplayName.text = HostedCard.DisplayName;

        switch (card.Type)
        {
            case PlayCardType.Defense:
            case PlayCardType.SpecialDefense:
                {
                    colorCircle.color = Color.green;
                    break;
                }

            case PlayCardType.Offense:
            case PlayCardType.SpecialOffense:
                {
                    colorCircle.color = Color.red;
                    break;
                }
        }
    }
}