using System;

[Serializable]
public struct Web3Settings
{
    public Web3ChainConfig[] chains;

    [Serializable]
    public struct Web3ChainConfig
    {
        public int chainId;
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
