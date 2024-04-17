using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using Thirdweb;
using System.IO;

public class Web3Controller : MonoBehaviour
{
    public static Web3Controller instance;

    // old
    public Web3Settings settings;
    public UnityEvent onWalletConnected;
    public int CurrentChainId { get; private set; } = 0;
    public string SelectedAccountAddress { get; private set; }

    #region ThirdWeb
    private Wallet wallet 
    { 
        get => ThirdwebManager.Instance.SDK.Wallet;
    }

    [Header("Events")]
    public UnityEvent onStart;
    public UnityEvent<WalletConnection> onConnectionRequested;
    public UnityEvent<string> onConnected;
    public UnityEvent<Exception> onConnectionError;
    public UnityEvent onDisconnected;
    public UnityEvent onSwitchNetwork;
    private string _address;
    #endregion

#if UNITY_EDITOR
    #region Unity editor button for updating hardhat contracts addresses
    [Serializable]
    struct DeploymentJson
    {
        public string address;
    }

    [Button(), PropertyTooltip("Update hardhat's addresses (chainId 31337) from localhost deployment," +
        "this way there is no need to copy the contracts' addresses from the terminal")]
    private void UpdateHardhatAddresses()
    {
        // use hardhat local deployment assuming that the battle-contracts project is on ../
        var deployments = @"..\battle-contracts\deployments\localhost";
        
        // get hardhat config
        var chain = settings.chains.FirstOrDefault(x => x.chainId == 31337);
        var index = Array.IndexOf(settings.chains, chain);

        chain.pepemonBattleAddress = JsonUtility.FromJson<DeploymentJson>(
            System.IO.File.ReadAllText($@"{deployments}\PepemonBattle.json")).address;
        Debug.Log("Set pepemonBattleAddress = " + chain.pepemonBattleAddress);

        chain.pepemonCardDeckAddress = JsonUtility.FromJson<DeploymentJson>(
            System.IO.File.ReadAllText($@"{deployments}\PepemonCardDeck.json")).address;
        Debug.Log("Set pepemonCardDeckAddress = " + chain.pepemonCardDeckAddress);

        chain.pepemonMatchmakerAddresses[0] = JsonUtility.FromJson<DeploymentJson>(
            System.IO.File.ReadAllText($@"{deployments}\PepemonMatchmaker.json")).address;
        Debug.Log("Set pepemonMatchmakerAddresses[0] = " + chain.pepemonMatchmakerAddresses[0]);

        settings.chains[index] = chain;
    }
    #endregion
#endif

#if !UNITY_EDITOR
    private bool _isMetamaskInitialised = false;
#endif

    [HideInInspector] public bool IsConnected = false;
    [HideInInspector] public ulong StarterDeckID = 0;
    [HideInInspector] public int StarterPepemonID = 0;

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

    public async UniTask ConnectWallet()
    {
        Debug.Log("Trying to connect");

#if UNITY_EDITOR
        var provider = WalletProvider.LocalWallet;
        await WriteDebugLocalAccount();
#else
        var provider = WalletProvider.Metamask;
#endif
        var wc = new WalletConnection(provider, settings.defaultChainId);
        onConnectionRequested.Invoke(wc);
        _address = await ThirdwebManager.Instance.SDK.Wallet.Connect(wc);
        onWalletConnected?.Invoke();
    }

    private async UniTask WriteDebugLocalAccount()
    {
#if UNITY_EDITOR
        // Thirdweb provides no easy way of connecting a wallet using hardhat's private key,
        // The lines below create a file with the private key. If it didn't exist, Thirdweb would generate
        // a random wallet instead.
        var key = new Nethereum.Signer.EthECKey(settings.debugPrivateKey);
        var path = Utils.GetAccountPath();
        var deviceId = Utils.GetDeviceIdentifier();
        Debug.Log("Encrypting and writing debug key...");
        await UniTask.RunOnThreadPool(() => 
        {
            var content = Utils.EncryptAndGenerateKeyStore(key, deviceId);
            File.WriteAllText(path, content);
        });
        Debug.Log("Done");
#endif
    }

    public Web3Settings.Web3ChainConfig GetChainConfig()
    {
        return settings.GetChainConfig(CurrentChainId);
    }

    /// <summary>
    /// Used with QueryUnityRequest to query contract functions (READ operations)
    /// </summary>
    /*public IUnityRpcRequestClientFactory GetUnityRpcRequestClientFactory()
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
    }*/

    /// <summary>
    /// Used with QueryUnityRequest to query contract functions (READ operations)
    /// </summary>
    /*public IUnityRpcRequestClientFactory GetReadOnlyRpcRequestClientFactory()
    {
        return new UnityWebRequestRpcClientFactory(settings.readOnlyRpcUrl);
    }*/

    /// <summary>
    /// Used to invoke contract functions (WRITE operations)
    /// </summary>
    /*public IContractTransactionUnityRequest GetContractTransactionUnityRequest()
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
    }*/

    // connect wallet in WebGL
    private void OpenMetamaskConnectDialog()
    {
#if !UNITY_EDITOR
        /*if (MetamaskInterop.IsMetamaskAvailable())
        {
            MetamaskInterop.EnableEthereum(Web3Controller.instance.name, nameof(EthereumEnabled), nameof(DisplayError));
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
        }*/
#endif
    }

    // callback from js
    public async void EthereumEnabled(string addressSelected)
    {
#if !UNITY_EDITOR
        /*if (!_isMetamaskInitialised)
        {
            MetamaskInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            _isMetamaskInitialised = true;
        }
        onWalletConnected?.Invoke();
        NewAccountSelected(addressSelected);
        IsConnected = true;*/
        await Task.CompletedTask;
#else
        await Task.CompletedTask;
#endif
    }

    /// <summary>
    /// Displays a popup in metamask asking the user to confirm changing the current network
    /// </summary>
    /// <param name="chainId">Network's chain id to switch to</param>
    /// <returns>Task that must be awaited for the popup to appear</returns>
    private async Task SwitchChain(int chainId)
    {
#if !UNITY_EDITOR
        /*var addRequest = new WalletAddEthereumChainUnityRequest(GetUnityRpcRequestClientFactory());

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
        await addRequest.SendRequest(chainParams);*/
        
        await Task.CompletedTask;
#else
        await Task.CompletedTask;
#endif
    }

    // callback from js
    public async void ChainChanged(string chainId)
    {
        CurrentChainId = (int)new HexBigInteger(chainId).Value;
        Debug.Log($"Changed chain to {CurrentChainId} (hex: {chainId})");

        if (CurrentChainId != settings.defaultChainId)
        {
            Debug.Log($"Attempting to switch chain to {settings.defaultChainId}");
            await SwitchChain(settings.defaultChainId);
        }
    }

    public void DisplayError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }

    /*public static async Task<TransactionReceipt> GetTransactionReceipt(string transactionHash)
    {
        var request = new TransactionReceiptPollingRequest(instance.GetUnityRpcRequestClientFactory());
        await request.PollForReceipt(transactionHash, 0.25f);
        if (request.Result != null)
        {
            return request.Result;
        }
        throw request.Exception;
    }*/
}
