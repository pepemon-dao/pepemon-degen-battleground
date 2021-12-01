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

    private Transform _targetPostion;        //the target transform in the layout group this card will lerp to

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

    public void SetTargetTransform(Transform _target)
    {
        _targetPostion = _target;
    }

    void Update()
    {
        //lerp to target position on UI
        transform.position = Vector3.Lerp(transform.position, _targetPostion.position, 5 * Time.deltaTime);
    }
}