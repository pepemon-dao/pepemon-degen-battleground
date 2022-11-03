using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    [BoxGroup("Deck Components"), SerializeField] private Text _deckDisplayName;
    [BoxGroup("Deck Components"), SerializeField] private Image _deckFrameImage;
    [BoxGroup("Deck Components"), SerializeField] private Image _fade;
    [BoxGroup("Deck Components"), SerializeField] private Text _pencil;

    public async Task LoadDeckInfo(ulong deckId)
    {
        Debug.Log("LoadDeckInfo of deckId " + deckId);
        var battleCard = await PepemonCardDeck.GetBattleCard(deckId);

        var metadata = PepemonFactoryCardCache.GetMetadata(deckId);

        _deckDisplayName.text = metadata?.name ?? "New Deck";
    }
}
