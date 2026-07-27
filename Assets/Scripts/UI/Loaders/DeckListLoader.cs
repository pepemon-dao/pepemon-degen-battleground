using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nethereum.Web3;
using Pepemon.Battle;
using Pepemon.UI;
using Sirenix.OdinInspector;
using Thirdweb;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeckListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] GameObject _deckPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _deckList;
    [TitleGroup("Component References"), SerializeField] GameObject _loadingMessage;
    [TitleGroup("Deck display options"), SerializeField] bool _deckEditMode;

    /// <summary>
    /// The loading label this list owns, so sibling controls on the same screen can reuse it
    /// instead of needing their own scene-wired reference.
    /// </summary>
    public GameObject LoadingMessage => _loadingMessage;

    /// <summary>
    /// Forces this list into selection or edit mode, overriding the scene setting.
    ///
    /// The battle deck picker's loader ships with _deckEditMode checked, so it rendered Edit
    /// buttons and never a Select button - decks appeared but none could be chosen, and every
    /// [deckpick] line read selectionMode=False. Callers that know which mode a screen needs
    /// now state it instead of depending on the inspector.
    /// </summary>
    public void SetEditMode(bool editMode) => _deckEditMode = editMode;

    [ReadOnly] public UnityEvent<ulong> onEditDeck;
    [ReadOnly] public UnityEvent<ulong, bool> onSelectDeck;
    private bool loadingInProgress = false;

    /// <summary>
    /// Removes all elements in _deckList and loads all decks using _deckPrefab.
    /// Each deck element invokes onItemSelected when clicked, the deckId is passed as a parameter of this event
    /// </summary>
    /// <param name="force">If true, bypasses the loadingInProgress guard to force a reload</param>
    public async void ReloadAllDecks(bool force = false)
    {
        // prevent re-reloading things over and over again with old async calls if
        // the user decides to go back and forth very quickly between screens
        if (loadingInProgress && !force) 
            return;

        loadingInProgress = true;

        _loadingMessage.SetActive(true);
        var loadingMessageLabel = _loadingMessage.GetComponent<TMPro.TMP_Text>();
        loadingMessageLabel.text = "Loading decks...";

        // destroy all existing deck instances before re-creating
        // Store children in array first to avoid modification during iteration
        var children = new Transform[_deckList.transform.childCount];
        for (int i = 0; i < _deckList.transform.childCount; i++)
        {
            children[i] = _deckList.transform.GetChild(i);
        }
        
        foreach (var child in children)
        {
            // Only keep objects that explicitly should not be destroyed
            if (child != null && !child.name.Contains("StarterDeck"))
            {
                Destroy(child.gameObject);
            }
        }
        
        // Everything from here runs inside try/finally. The guard and the loading label were
        // previously only cleared on the happy path, so a single throw - an RPC hiccup while
        // reading decks, for instance - left loadingInProgress stuck at true forever. Every
        // later ReloadAllDecks() then returned immediately at the guard, leaving the player
        // staring at "Loading decks..." with an empty list for the rest of the session. The
        // `force` flag exists to work around exactly this; it is no longer the only escape.
        try
        {

        string account = "";

        try
        {
            account = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        } catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
        // should not happen, but if it happens then it won't crash the game
        if (string.IsNullOrEmpty(account))
        {
            // A gated empty state must offer the action, not just describe it. This screen
            // previously told the player to connect a wallet and gave them nothing to press.
            loadingMessageLabel.text = "Connect your wallet to see your decks";
            PixelNotice.Instance.Show(
                "No wallet connected",
                "Connect your wallet to see and mint your decks.",
                "CONNECT WALLET",
                async () =>
                {
                    if (Web3Controller.instance == null) return;
                    await Web3Controller.instance.ConnectWallet();
                    if (Web3Controller.instance.IsConnected) ReloadAllDecks(force: true);
                });
            return;
        }

        // load all decks
        List<ulong> decks = new List<ulong>();
        try
        {
            decks = await PepemonCardDeck.GetPlayerDecks(account);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[decks] Unable to read decks: {ex.Message}");
            loadingMessageLabel.text = "Could not load your decks. Please retry.";
            return;
        }

        var loadingTasks = new List<UniTask>();
        if (MainMenuController.claimedStarterDeck)
        {
            ulong deckId = 1234;

            var deckInstance = Instantiate(_deckPrefab);

            // show or hide the edit mode
            deckInstance.GetComponent<DeckController>().DisplayDeckEditMode = _deckEditMode;
            deckInstance.GetComponent<DeckController>().onEditButtonClicked.AddListener(
                delegate {
                    onEditDeck?.Invoke(deckId);
                });
            deckInstance.GetComponent<DeckController>().onSelectButtonClicked.AddListener(
                delegate {
                    onSelectDeck?.Invoke(deckId, false);
                });

            await LoadAndAddDeck(deckInstance, deckId);
        }

        decks.ForEach((deckId) => {
            var deckInstance = Instantiate(_deckPrefab);

            // show or hide the edit mode
            deckInstance.GetComponent<DeckController>().DisplayDeckEditMode = _deckEditMode;
            deckInstance.GetComponent<DeckController>().onEditButtonClicked.AddListener(
                delegate {
                    onEditDeck?.Invoke(deckId);
                });
            deckInstance.GetComponent<DeckController>().onSelectButtonClicked.AddListener(
                delegate {
                    onSelectDeck?.Invoke(deckId, false);
                });

            // this should set each deck detail in parallel
            loadingTasks.Add(LoadAndAddDeck(deckInstance, deckId));
        });
        await UniTask.WhenAll(loadingTasks);

        // Tell the player when there is genuinely nothing to pick, rather than showing an
        // empty screen with no explanation.
        if (_deckList.transform.childCount == 0)
        {
            loadingMessageLabel.text = decks.Count == 0
                ? "No decks yet - mint one from the Deck screen"
                : "Your decks could not be loaded. Please retry.";
            return;
        }

        _loadingMessage.SetActive(false);

        }
        finally
        {
            loadingInProgress = false;
        }
    }

    private async UniTask LoadAndAddDeck(GameObject deckInstance, ulong deckId)
    {
        bool showDeck;
        try
        {
            showDeck = await deckInstance.GetComponent<DeckController>().LoadDeckInfo(deckId, !_deckEditMode);
        }
        catch (System.Exception ex)
        {
            // One bad deck must not take down the whole list.
            Debug.LogError($"[decks] Unable to load deck {deckId}: {ex.Message}");
            Destroy(deckInstance);
            return;
        }

        if (showDeck)
        {
            deckInstance.transform.SetParent(_deckList.transform, false);
            return;
        }

        // Was instantiated but never parented, so it used to leak at the scene root.
        Destroy(deckInstance);
    }
}
