using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

    public Web3Settings settings;
    public IWeb3Provider provider;
    public UnityEvent onWalletConnected;
    public int CurrentChainId { get; private set; } = 0;
    public string SelectedAccountAddress { get; private set; }

#if !DEBUG
    private bool _isMetamaskInitialised = false;
#endif

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
        OpenMetamaskConnectDialog();
#else
        Account debugAccount = new Account(settings.debugPrivateKey);
        ChainChanged(string.Format("0x{0:X}", settings.debugChainId));
        NewAccountSelected(debugAccount.Address);
        onWalletConnected?.Invoke();
#endif
    }

    public Web3Settings.Web3ChainConfig GetChainConfig()
    {
        return settings.GetChainConfig(CurrentChainId);
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
#else
        return new UnityWebRequestRpcClientFactory(settings.debugRpcUrl);
#endif
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
        return new TransactionSignedUnityRequest(
            url: settings.debugRpcUrl,
            privateKey: settings.debugPrivateKey,
            chainId: settings.debugChainId);
#endif
    }

    // connect wallet in WebGL
    private void OpenMetamaskConnectDialog()
    {
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
        SelectedAccountAddress = accountAddress;
        Debug.Log($"Account changed to {SelectedAccountAddress}");
    }

    // callback from js
    public void ChainChanged(string chainId)
    {
        CurrentChainId = (int)new HexBigInteger(chainId).Value;
        Debug.Log($"Changed chain to {CurrentChainId} (hex: {chainId})");
        try
        {
            //simple workaround to show suported configured chains
            print(CurrentChainId.ToString());
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
        Debug.LogError(errorMessage);
    }
}
