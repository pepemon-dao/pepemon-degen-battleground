using System;

[Serializable]
public class Web3Settings
{
    public Web3ChainConfig[] chains;
    public int defaultChainId = 31337;
    public string debugRpcUrl = "http://localhost:8545";
    public string debugPrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
    public string readOnlyRpcUrl = "";

    [Serializable]
    public struct Web3ChainConfig
    {
        public int chainId;
        public string chainName;
        public string chainCurrencyName;
        public string chainCurrencySymbol;
        public uint chainCurrencyDecimals;
        public string rpcUrl;
        public string blockExplorerUrl;
        public string pepemonBattleAddress;
        public string pepemonBattleAbi;
        public string pepemonFactoryAddress;
        public string pepemonFactoryAbi;
        public string pepemonCardDeckAddress;
        public string pepemonCardDeckAbi;
        public long pepemonGasLimit;
        public string pepemonMatchmakerAbi;
        public string[] pepemonMatchmakerAddresses;

        /// <summary>
        /// Booster pack store. Address only: the ABI is a property of the contract source
        /// rather than of a deployment, so it lives in PepemonBoosterPack.cs instead of being
        /// pasted into scene YAML.
        ///
        /// Empty until a booster contract is deployed for this chain. The store screen checks
        /// PepemonBoosterPack.IsConfigured and shows an explanatory state rather than failing.
        /// </summary>
        public string pepemonBoosterPackAddress;
    }

    public Web3ChainConfig GetChainConfig(int chainId)
    {
        for(int i = 0; i < chains.Length; i++)
        {
            if (chains[i].chainId == chainId)
            {
                return chains[i];
            }
        }
        throw new ArgumentException($"No chain settings found for chain '{chainId}'");
    }
}
