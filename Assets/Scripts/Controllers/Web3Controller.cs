using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Nethereum.Unity.Metamask;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

    public Web3Settings settings;
    public IWeb3Provider provider;
    public UnityEvent onWalletConnected;
    public int CurrentChainId { get; private set; } = 0;
    public string SelectedAccountAddress { get; private set; }

#if !UNITY_EDITOR
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
#if !UNITY_EDITOR
        OpenMetamaskConnectDialog();
#else
        Account debugAccount = new Account(settings.debugPrivateKey);
        NewAccountSelected(debugAccount.Address);
        ChainChanged(string.Format("0x{0:X}", settings.debugChainId));
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
#if !UNITY_EDITOR
        if (MetamaskInterop.IsMetamaskAvailable()) 
        {
            return new MetamaskRequestRpcClientFactory(SelectedAccountAddress, null);
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
#if !UNITY_EDITOR
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            return new MetamaskTransactionUnityRequest(SelectedAccountAddress, GetUnityRpcRequestClientFactory());
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
#if !UNITY_EDITOR
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
#if !UNITY_EDITOR
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
    public async void ChainChanged(string chainId)
    {
        CurrentChainId = (int)new HexBigInteger(chainId).Value;
        Debug.Log($"Changed chain to {CurrentChainId} (hex: {chainId})");
        await new PepemonFactoryCardCache().PreloadAll();
    }

    public void DisplayError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}
