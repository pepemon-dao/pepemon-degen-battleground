using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static EVM;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

#if UNITY_EDITOR
    public int defaultChainId = 31337;
#else
    public int defaultChainId = 1;
    public string _selectedAccountAddress = "";
#endif
    public int currentChainId = 1;
    public Web3Settings settings;
    public IWeb3Provider provider; // TODO: remove
    public UnityEvent onWalletConnected;
    public bool _isMetamaskInitialised = false;

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
    }

    public void ConnectWallet()
    {
        Debug.Log("Trying to connect");
#if !DEBUG
        MetamaskConnectButton_Clicked();
#else
        onWalletConnected?.Invoke();
#endif
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

        await PepemonCardDeck.SetApprovalState(true);

        ulong addCardId = 6;
        Debug.Log("Adding card " + addCardId);
        string tx = await PepemonCardDeck.AddSupportCards(testDeck, new SupportCardRequest { Amount = 1, SupportCardId = addCardId });
        Debug.Log("Done? : " + tx);

        supportCardIDs = await PepemonCardDeck.GetAllSupportCards(testDeck);
        supportCardsInfo = supportCardIDs.Select(i => i.ToString()).Aggregate("", (acc, i) => acc + ", " + i);
        Debug.Log("Deck [" + testDeck + "] has cards: " + supportCardsInfo);

        Debug.Log("Removing card " + addCardId);
        tx = await PepemonCardDeck.RemoveSupportCards(testDeck, new SupportCardRequest { Amount = 1, SupportCardId = addCardId });
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

    /// <summary>
    /// Used with QueryUnityRequest to query contract functions (READ operations)
    /// </summary>
    public IUnityRpcRequestClientFactory GetUnityRpcRequestClientFactory()
    {
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable()) 
        {
            return new MetamaskRequestRpcClientFactory(_selectedAccountAddress, null, 1000);
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
            return null;
        }
#endif
        //_selectedAccountAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
        return new UnityWebRequestRpcClientFactory("http://localhost:8545");
    }

    /// <summary>
    /// Used to invoke contract functions (WRITE operations)
    /// </summary>
    public IContractTransactionUnityRequest GetContractTransactionUnityRequest()
    {
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            return new MetamaskTransactionUnityRequest(_selectedAccountAddress, GetUnityRpcRequestClientFactory());
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
            return null;
        }
#else
        //_selectedAccountAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
        return new TransactionSignedUnityRequest(
            url: "http://localhost:8545",
            privateKey: "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80",
            chainId: 31337);
#endif
    }

    // connect wallet in WebGL
    private void MetamaskConnectButton_Clicked()
    {
        //_lblError.visible = false;
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            MetamaskInterop.EnableEthereum(gameObject.name, nameof(EthereumEnabled), nameof(DisplayError));
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
        }
#endif
    }

    // callback from js
    public void EthereumEnabled(string addressSelected)
    {
#if !DEBUG
        if (!_isMetamaskInitialised)
        {
            MetamaskInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            _isMetamaskInitialised = true;
        }
        onWalletConnected?.Invoke();
        NewAccountSelected(addressSelected);
#endif
    }

    // callback from js
    public void NewAccountSelected(string accountAddress)
    {
        //_selectedAccountAddress = accountAddress;
        //_lblAccountSelected.text = accountAddress;
        //_lblAccountSelected.visible = true;
    }

    // callback from js
    public void ChainChanged(string chainId)
    {
        print(chainId);
        currentChainId = (int) new HexBigInteger(chainId).Value;
        try
        {
            //simple workaround to show suported configured chains
            print(currentChainId.ToString());
            StartCoroutine(GetBlockNumber());
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
        }
    }

    private IEnumerator GetBlockNumber()
    {
        var blockNumberRequest = new EthBlockNumberUnityRequest(GetUnityRpcRequestClientFactory());
        yield return blockNumberRequest.SendRequest();
        print(blockNumberRequest.Result.Value);
    }

    public void DisplayError(string errorMessage)
    {
        //_lblError.text = errorMessage;
        //_lblError.visible = true;
        Debug.LogError(errorMessage);
    }
}
