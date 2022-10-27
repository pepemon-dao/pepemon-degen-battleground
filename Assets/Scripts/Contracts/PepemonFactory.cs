using Contracts.PepemonFactory.abi.ContractDefinition;
using Nethereum.Unity.Rpc;
using System;
using System.Threading.Tasks;
using UnityEngine;

class PepemonFactory
{
    /// <summary>
    /// PepemonFactory address (Support cards, Battle cards)
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonFactoryAddress;

    /// <summary>
    /// Gets the specified card's metadata
    /// </summary>
    /// <returns></returns>
    public static async Task<CardMetadata> GetCardMetadata(ulong tokenId)
    {
        var request = new QueryUnityRequest<UriFunction, UriOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        await request.Query(
            new UriFunction { Id = tokenId },
            Address);

        try
        {
            return JsonUtility.FromJson<CardMetadata>(request.Result.ReturnValue1);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to load metadata for card token '{tokenId}': {e.Message}", e);
        }
    }

    [Serializable]
    public struct CardMetadata
    {
        public string external_url;
        public string image;
        public string name;
        public string description;
        public CardAttribute[] attributes;
    }

    [Serializable]
    public struct CardAttribute
    {
        public string trait_type;
        public string value;
    }
}
