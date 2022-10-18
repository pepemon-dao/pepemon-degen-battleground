using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

    public int defaultChainId = 1;
    public int currentChainId = 1;
    public Web3Settings settings;
    public IWeb3Provider provider;
    public UnityEvent onWalletConnected;

    private void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        // Use the local Hardhat test network
        provider = new RPCWeb3Provider()
        {
            ChainId = 31337,
            PrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80",
            Network = "Hardhat",
            RpcUrl = "http://localhost:8545"
        };
#elif UNITY_WEBGL
        GameObject webglProvider = new GameObject("WebGL_Web3Provider");
        provider = webglProvider.AddComponent<WebGLWeb3Provider>();
#else
        provider = null;
#endif
    }

    public async void ConnectWallet()
    {
        Debug.Log("Trying to connect");
        string addr = await provider.ConnectToWallet(new Web3ConnectArgs { chainId = defaultChainId });
        Debug.Log("Got addr: " + addr);
        onWalletConnected?.Invoke();
    }

    public Web3Settings.Web3ChainConfig GetChainConfig()
    {
        return settings.GetChainConfig(currentChainId);
    }

    public async void RunTests()
    {
        Debug.Log("Creating decks");
        await PepemonCardDeck.CreateDeck();

        List<ulong> deckIds = await PepemonCardDeck.GetPlayerDecks("0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266");
        Debug.Log("Decks: " + JsonConvert.SerializeObject(deckIds));

        ulong testDeck = deckIds[0];
        List<ulong> supportCardIDs = await PepemonCardDeck.GetAllSupportCards(testDeck);
        string supportCardsInfo = supportCardIDs.Select(i => i.ToString()).Aggregate("", (acc, i) => acc + ", " + i);
        Debug.Log("Deck [" + testDeck + "] has cards: " + supportCardsInfo);


        ulong addCardId = 6;
        Debug.Log("Adding card " + addCardId);
        string tx = await PepemonCardDeck.AddSupportCards(testDeck, new PepemonCardDeck.SupportCardRequest { amount = 1, supportCardId = addCardId });
        Debug.Log("Done? : " + tx);

        supportCardIDs = await PepemonCardDeck.GetAllSupportCards(testDeck);
        supportCardsInfo = supportCardIDs.Select(i => i.ToString()).Aggregate("", (acc, i) => acc + ", " + i);
        Debug.Log("Deck [" + testDeck + "] has cards: " + supportCardsInfo);

        Debug.Log("Removing card " + addCardId);
        tx = await PepemonCardDeck.RemoveSupportCards(testDeck, new PepemonCardDeck.SupportCardRequest { amount = 1, supportCardId = addCardId });
        Debug.Log("Done? : " + tx);

        supportCardIDs = await PepemonCardDeck.GetAllSupportCards(testDeck);
        supportCardsInfo = supportCardIDs.Select(i => i.ToString()).Aggregate("", (acc, i) => acc + ", " + i);
        Debug.Log("Deck [" + testDeck + "] has cards: " + supportCardsInfo);



        Debug.Log("Getting battle card ID:");

        ulong currentBattleCardId = await PepemonCardDeck.GetBattleCard(testDeck);
        Debug.Log("Deck [" + testDeck + "] has battle card: " + currentBattleCardId);

        Debug.Log("Setting battle card");
        tx = await PepemonCardDeck.SetBattleCard(testDeck, addCardId);
        Debug.Log("Done? : " + tx);

        currentBattleCardId = await PepemonCardDeck.GetBattleCard(testDeck);
        Debug.Log("Deck [" + testDeck + "] has battle card: " + currentBattleCardId);

        Debug.Log("Removing battle card");
        tx = await PepemonCardDeck.RemoveBattleCard(testDeck);
        Debug.Log("Done? : " + tx);


        currentBattleCardId = await PepemonCardDeck.GetBattleCard(testDeck);
        Debug.Log("Deck [" + testDeck + "] has battle card: " + currentBattleCardId);

        Debug.Log("Setting battle card");
        tx = await PepemonCardDeck.SetBattleCard(testDeck, addCardId);
        Debug.Log("Done? : " + tx);

        currentBattleCardId = await PepemonCardDeck.GetBattleCard(testDeck);
        Debug.Log("Deck [" + testDeck + "] has battle card: " + currentBattleCardId);

        Debug.Log("Removing battle card");
        tx = await PepemonCardDeck.RemoveBattleCard(testDeck);
        Debug.Log("Done? : " + tx);

    }
}
