using System;

[Serializable]
public class Web3Settings
{
    public Web3ChainConfig[] chains;
    public int defaultChainId = 31337;
    public string debugRpcUrl = "http://localhost:8545";
    public string debugPrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";

    [Serializable]
    public struct Web3ChainConfig
    {
        public int chainId;
        public string chainName;
        public string chainCurrencyName;
        public string chainCurrencySymbol;
        public uint chainCurrencyDecimals;
        public string rpcUrl;
        public string pepemonBattleAddress;
        public string pepemonFactoryAddress;
        public string pepemonCardDeckAddress;
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
