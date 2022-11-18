using System.Collections;
using System.Collections.Generic;
using Nethereum.Web3;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// MonoBehavior for Screen_5_EditDeck
/// </summary>
public class ScreenEditDeck : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardsListLoader;
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardsListLoader;

    public async void LoadAllCards()
    {
        var account = FindObjectOfType<MainMenuController>().web3.SelectedAccountAddress;
        var ownedCardIds = await PepemonFactory.GetOwnedCards(account, PepemonFactoryCardCache.CardsIds);
        _battleCardsListLoader.GetComponent<BattleCardListLoader>().ReloadAllBattleCards(ownedCardIds);
        _supportCardsListLoader.GetComponent<SupportCardListLoader>().ReloadAllSupportCards(ownedCardIds);
    }
}
