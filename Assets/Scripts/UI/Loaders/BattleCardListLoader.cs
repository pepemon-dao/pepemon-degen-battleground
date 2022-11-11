using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

public class BattleCardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _battleCardList;

    public void ReloadAllBattleCards(List<ulong> ownedCardIds)
    {
        // destroy before re-creating
        foreach (var battlecard in _battleCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        var battleCardAttribute = new CardAttribute { value = "Pepemon Battle", trait_type = "Set" };

        foreach(var cardId in ownedCardIds)
        {
            var metadata = (CardMetadata)PepemonFactoryCardCache.GetMetadata(cardId);

            // skip support cards
            if (!metadata.attributes.Contains(battleCardAttribute))
                continue;

            var battleCardInstance = Instantiate(_battleCardPrefab);
            battleCardInstance.transform.SetParent(_battleCardList.transform, false);

            battleCardInstance.GetComponent<CardPreview>().LoadCardData(cardId);
            // todo: handle selection event
        }
    }
}
