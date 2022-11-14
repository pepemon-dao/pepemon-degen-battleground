using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static PepemonFactory;

public class SupportCardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _supportCardList;

    public void ReloadAllSupportCards(List<ulong> ownedCardIds)
    {
        // destroy before re-creating
        foreach (var battlecard in _supportCardList.GetComponentsInChildren<Button>())
        {
            Destroy(battlecard.gameObject);
        }

        var supportCardAttribute = new CardAttribute { value = "Pepemon Support", trait_type = "Set" };

        // add to the list
        foreach (var cardId in ownedCardIds)
        {
            var metadata = (CardMetadata)PepemonFactoryCardCache.GetMetadata(cardId);

            // skip battle cards
            if (!metadata.attributes.Contains(supportCardAttribute))
                continue;

            var supportCardInstance = Instantiate(_supportCardPrefab);
            supportCardInstance.transform.SetParent(_supportCardList.transform, false);

            supportCardInstance.GetComponent<CardPreview>().LoadCardData(cardId);
            // todo: handle selection event
        }
    }
}
