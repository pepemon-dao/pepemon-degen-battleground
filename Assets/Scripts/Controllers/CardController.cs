using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

// Handles the instance of a card within a session
public class CardController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _cardDisplayName;

    [BoxGroup("Card Components"), SerializeField] private Image cardFrameImage;
    [BoxGroup("Card Components"), SerializeField] private Image cardBackgroundImage;
    [BoxGroup("Card Components"), SerializeField] private Image gemImage;

    [BoxGroup("Card Backdrops"), SerializeField] private Sprite defenceCardFrame;
    [BoxGroup("Card Backdrops"), SerializeField] private Sprite specialDefenceCardFrame;
    [BoxGroup("Card Backdrops"), SerializeField] private Sprite attackCardFrame;
    [BoxGroup("Card Backdrops"), SerializeField] private Sprite specialAttackCardFrame;

    [BoxGroup("Card Gems"), SerializeField] private Sprite commonGem;
    [BoxGroup("Card Gems"), SerializeField] private Sprite rareGem;
    [BoxGroup("Card Gems"), SerializeField] private Sprite epicGem;

    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite atkBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite defBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite intBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite r_atkBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite r_defBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite spclBG;
    [BoxGroup("Card Backgrounds"), SerializeField] private Sprite speedBG;

    [ReadOnly] public Card HostedCard;

    private Transform _targetPostion;        //the target transform in the layout group this card will lerp to
    private Vector3 _startingTargetPosition = Vector3.zero;  //the position to return to after being highlighted

    public void PouplateCard(Card card)
    {
        HostedCard = card;
        _cardDisplayName.text = HostedCard.DisplayName;

        switch (card.Type)
        {
            case PlayCardType.Defense:
                cardFrameImage.sprite = defenceCardFrame;
                break;
            case PlayCardType.SpecialDefense:
                cardFrameImage.sprite = specialDefenceCardFrame;
                break;
            case PlayCardType.Offense:
                cardFrameImage.sprite = attackCardFrame;
                break;
            case PlayCardType.SpecialOffense:
                cardFrameImage.sprite = specialAttackCardFrame;
                break;
        }

        switch (card.Rarity)
        {
            case CardRarity.Common:
                gemImage.sprite = commonGem;
                break;
            case CardRarity.Rare:
                gemImage.sprite = rareGem;
                break;
            case CardRarity.Epic:
                gemImage.sprite = epicGem;
                break;
        }
    }

    public void SetTargetTransform(Transform _target)
    {
        _targetPostion = _target;

    }

    /// <summary>
    /// The cards move foward when they are attacking
    /// </summary>
    public void SetAttackingTransform(int attackIndex)
    {

        if (attackIndex == 1)
        {
            _startingTargetPosition = _targetPostion.position;

            _targetPostion.position = new Vector3(_targetPostion.position.x, _targetPostion.position.y - 5f, _targetPostion.position.z - 15);
            transform.SetAsLastSibling(); //make sure this card is in front of the bottom cards.
        }
        else if (attackIndex == 2)
        {
            _startingTargetPosition = _targetPostion.position;

            _targetPostion.position = new Vector3(_targetPostion.position.x, _targetPostion.position.y + 5f, _targetPostion.position.z - 15);
            transform.SetAsLastSibling(); //make sure this card is in front of the bottom cards.
        }

    }

    /// <summary>
    /// return back to the board after being highlighted when attacking/defending
    /// </summary>
    public void ReturnToBaseTransform()
    {
        if (_startingTargetPosition != Vector3.zero)
            _targetPostion.position = _startingTargetPosition;

    }

    void Update()
    {
        //lerp to target position on UI

        transform.position = Vector3.Lerp(transform.position, _targetPostion.position, 5 * Time.deltaTime);
    }
}