using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

// Handles displaying game state
public class UIController : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _cardPrefab;
    [TitleGroup("Component References"), SerializeField] GameController _gameController;

    [BoxGroup("Sidebar")]
    [SerializeField, BoxGroup("Sidebar")] TextMeshProUGUI _roundCount;
    [SerializeField, BoxGroup("Sidebar/ID1")] TextMeshProUGUI _index1Name;
    [SerializeField, BoxGroup("Sidebar/ID2")] TextMeshProUGUI _index2Name;
    [SerializeField, BoxGroup("Sidebar/ID1")] TextMeshProUGUI _index1DeckCount;
    [SerializeField, BoxGroup("Sidebar/ID2")] TextMeshProUGUI _index2DeckCount;
    [SerializeField, BoxGroup("Sidebar/ID1")] TextMeshProUGUI _index1HP;
    [SerializeField, BoxGroup("Sidebar/ID2")] TextMeshProUGUI _index2HP;
    [SerializeField, BoxGroup("Sidebar/ID1")] Image _index1Icon;
    [SerializeField, BoxGroup("Sidebar/ID2")] Image _index2Icon;

    [SerializeField, BoxGroup("Board")] Transform _index1CardContainer;
    [SerializeField, BoxGroup("Board")] Transform _index2CardContainer;

    Player _player1;
    Player _player2;


    public void InitialiseGame(Player player1, Player player2)
    {
        _player1 = player1;
        _player2 = player2;

        _index1Name.text = player1.PlayerPepemon.DisplayName;
        _index2Name.text = player2.PlayerPepemon.DisplayName;

        _index1DeckCount.text = player1.PlayerDeck.GetDeck().Count + "cards";
        _index2DeckCount.text = player2.PlayerDeck.GetDeck().Count + "cards";

        _index1HP.text = player1.CurrentHP + "hp";
        _index2HP.text = player2.CurrentHP + "hp";

        _index1Icon.sprite = player1.PlayerPepemon.DisplayIcon;
        _index2Icon.sprite = player2.PlayerPepemon.DisplayIcon;
    }


    // part1 lay down all AD + DF cards
    // play the hand
    // Reverse role
    // next round

    public void DisplayHands()
    {
        // Cleanup old hand
        foreach (Transform child in _index1CardContainer) GameObject.Destroy(child.gameObject);
        foreach (Transform child in _index2CardContainer) GameObject.Destroy(child.gameObject);

        // Display new hand
        for (int i = 0; i < _player1.CurrentHand.GetCardsInHand.Count; i++)
        {
            GameObject go = Instantiate(_cardPrefab);
            go.transform.SetParent(_index1CardContainer);
            go.GetComponent<CardController>().PouplateCard(_player1.CurrentHand.GetCardsInHand[i]);
        }

        for (int i = 0; i < _player2.CurrentHand.GetCardsInHand.Count; i++)
        {
            GameObject go = Instantiate(_cardPrefab);
            go.transform.SetParent(_index2CardContainer);
            go.GetComponent<CardController>().PouplateCard(_player2.CurrentHand.GetCardsInHand[i]);
        }

        // Update deck count
        _index1DeckCount.text = _player1.CurrentDeck.GetDeck().Count + "cards";
        _index2DeckCount.text = _player2.CurrentDeck.GetDeck().Count + "cards";
        _roundCount.text = "R: " + _gameController.GetRoundNumber();
    }

    // disables cards based on attacking or defending
    public void FlipCards(int attackIndex)
    {
        if (attackIndex == 1) // p1
        {
            for (int i = 0; i < _index1CardContainer.childCount; i++)
            {
                if (_index1CardContainer.GetChild(i).GetComponent<CardController>().HostedCard.IsAttackingCard() == false)
                {
                    _index1CardContainer.GetChild(i).GetComponent<Image>().color = Color.gray;
                }
            }

            for (int i = 0; i < _index2CardContainer.childCount; i++)
            {
                if (_index2CardContainer.GetChild(i).GetComponent<CardController>().HostedCard.IsAttackingCard() == true)
                {
                    _index2CardContainer.GetChild(i).GetComponent<Image>().color = Color.gray;
                }
            }
        }
        else if (attackIndex == 2) // p2
        {
            for (int i = 0; i < _index2CardContainer.childCount; i++)
            {
                if (_index2CardContainer.GetChild(i).GetComponent<CardController>().HostedCard.IsAttackingCard() == false)
                {
                    _index2CardContainer.GetChild(i).GetComponent<Image>().color = Color.gray;
                }
            }

            for (int i = 0; i < _index1CardContainer.childCount; i++)
            {
                if (_index1CardContainer.GetChild(i).GetComponent<CardController>().HostedCard.IsAttackingCard() == true)
                {
                    _index1CardContainer.GetChild(i).GetComponent<Image>().color = Color.gray;
                }
            }
        }
        else if (attackIndex == 3) // reset cards
        {
            for (int i = 0; i < _index2CardContainer.childCount; i++)
            {
                _index2CardContainer.GetChild(i).GetComponent<Image>().color = Color.black;
            }

            for (int i = 0; i < _index1CardContainer.childCount; i++)
            {
                _index1CardContainer.GetChild(i).GetComponent<Image>().color = Color.black;
            }
        }
    }


    public void UpdateUI()
    {
        _index1HP.text = _player1.CurrentHP + "hp";
        _index2HP.text = _player2.CurrentHP + "hp";

        _index1DeckCount.text = _player1.CurrentDeck.GetDeck().Count + "cards";
        _index2DeckCount.text = _player2.CurrentDeck.GetDeck().Count + "cards";
        _roundCount.text = "R: " + _gameController.GetRoundNumber();
    }

    public void DisplayWinner(Player player)
    {
        Debug.Log("WINNER: " + player.PlayerPepemon.DisplayName);
    }
}
