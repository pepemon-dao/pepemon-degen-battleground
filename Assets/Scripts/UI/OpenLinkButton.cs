using Pepemon.Store;
using Pepemon.UI;
using UnityEngine;

public class OpenLinkButton : MonoBehaviour
{
    [SerializeField] private string url = "https://pepemon.world";

    public void OpenLink()
    {
        // Store links open the in-game booster store rather than the web shop. The web shop's
        // packs mint onto a different cards contract from the one this game reads, so a player
        // who bought there would receive cards the game can never show them.
        //
        // Matched on the URL rather than rewired in the scene: every hard-to-find bug in this
        // project has been a scene reference, and a code path is at least visible in review.
        if (StoreLinks.IsStoreLink(url))
        {
            BoosterStoreScreen.Instance.Open();
            return;
        }

        Application.OpenURL(url);
    }
}
