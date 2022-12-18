using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UnityEngine;
using UnityEngine.Events;
#if !UNITY_EDITOR
using Nethereum.Unity.Metamask;
using Nethereum.RPC.HostWallet;
using System.Collections.Generic;
#endif
using Nethereum.RPC.Eth.DTOs;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

    public Web3Settings settings;
    public UnityEvent onWalletConnected;
    public int CurrentChainId { get; private set; } = 0;
    public string SelectedAccountAddress { get; private set; }

#if !UNITY_EDITOR
    private bool _isMetamaskInitialised = false;
#endif

    private void Awake()
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
        CurrentChainId = settings.defaultChainId;
    }

    public void ConnectWallet()
    {
        Debug.Log("Trying to connect");
#if !UNITY_EDITOR
        OpenMetamaskConnectDialog();
#else
        Account debugAccount = new Account(settings.debugPrivateKey);
        NewAccountSelected(debugAccount.Address);
        ChainChanged(string.Format("0x{0:X}", settings.defaultChainId));
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
    /// Used with QueryUnityRequest to query contract functions (READ operations)
    /// </summary>
    public IUnityRpcRequestClientFactory GetReadOnlyRpcRequestClientFactory()
    {
        return new UnityWebRequestRpcClientFactory(settings.readOnlyRpcUrl);
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
            chainId: settings.defaultChainId);
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
    public async void EthereumEnabled(string addressSelected)
    {
#if !UNITY_EDITOR
        if (!_isMetamaskInitialised)
        {
            await SwitchChain(settings.defaultChainId);
            MetamaskInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            _isMetamaskInitialised = true;
        }
        onWalletConnected?.Invoke();
        NewAccountSelected(addressSelected);
#else
        await Task.CompletedTask;
#endif
    }

    // callback from js
    public void NewAccountSelected(string accountAddress)
    {
        SelectedAccountAddress = accountAddress;
        Debug.Log($"Account changed to {SelectedAccountAddress}");
    }

    /// <summary>
    /// Displays a popup in metamask asking the user to confirm changing the current network
    /// </summary>
    /// <param name="chainId">Network's chain id to switch to</param>
    /// <returns>Task that must be awaited for the popup to appear</returns>
    private async Task SwitchChain(int chainId)
    {
#if !UNITY_EDITOR
        var addRequest = new WalletAddEthereumChainUnityRequest(GetUnityRpcRequestClientFactory());

        var config = settings.GetChainConfig(chainId);
        var chainParams = new AddEthereumChainParameter
        {
            ChainId = new HexBigInteger(chainId),
            ChainName = config.chainName,
            RpcUrls = new List<string> { config.rpcUrl },
            NativeCurrency = new NativeCurrency
            {
                Name = config.chainCurrencyName,
                Symbol = config.chainCurrencySymbol,
                Decimals = config.chainCurrencyDecimals
            }
        };
        await addRequest.SendRequest(chainParams);
#else
        await Task.CompletedTask;
#endif
    }

    // callback from js
    public void ChainChanged(string chainId)
    {
        CurrentChainId = (int)new HexBigInteger(chainId).Value;
        Debug.Log($"Changed chain to {CurrentChainId} (hex: {chainId})");
    }

    public void DisplayError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }

    public static async Task<TransactionReceipt> GetTransactionReceipt(string transactionHash)
    {
        var request = new TransactionReceiptPollingRequest(instance.GetUnityRpcRequestClientFactory());
        await request.PollForReceipt(transactionHash, 0.25f);
        if (request.Result != null)
        {
            return request.Result;
        }
        throw request.Exception;
    }
}
