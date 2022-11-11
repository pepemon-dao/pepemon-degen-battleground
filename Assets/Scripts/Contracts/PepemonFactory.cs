using Contracts.PepemonFactory.abi.ContractDefinition;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public static async Task<CardMetadata?> GetCardMetadata(ulong tokenId)
    {
        var request = new QueryUnityRequest<UriFunction, UriOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        await request.Query(
            new UriFunction { Id = tokenId },
            Address);

        // happens when tokenId was not found
        if (request.Result == null)
            return null;

        // example of ReturnValue1:
        // data:application/json;base64\r\n\r\neyJwb29sIjogeyJuYW1lIjogInJvb3QiLCJwb2ludHMiOiAxfSwiZXh0ZXJuYWxfdXJsIjogImh0dHBzOi8vcGVwZW1vbi53b3JsZC8iLCJpbWFnZSI6ICJodHRwczovL2JhZnliZWljNmJkbnRoanA0djU0c3JtN3JvbHp0ZGRqaDRzb2dxajN1Y3V6eXVha3J1dHNqdjY3b21tLmlwZnMuZHdlYi5saW5rL2JmYWZueWNhcmQucG5nIiwibmFtZSI6ICJGYWZueSIsImRlc2NyaXB0aW9uIjogIkZhZm55IChCYXR0bGUgdmVyLikiLCJhdHRyaWJ1dGVzIjpbeyJ0cmFpdF90eXBlIjoiU2V0IiwidmFsdWUiOiJQZXBlbW9uIEJhdHRsZSJ9LHsidHJhaXRfdHlwZSI6IkxldmVsIiwidmFsdWUiOjF9LHsidHJhaXRfdHlwZSI6IkVsZW1lbnQiLCJ2YWx1ZSI6IkZpcmUifSx7InRyYWl0X3R5cGUiOiJXZWFrbmVzcyIsInZhbHVlIjoiV2F0ZXIifSx7InRyYWl0X3R5cGUiOiJSZXNpc3RhbmNlIiwidmFsdWUiOiJHcmFzcyJ9LHsidHJhaXRfdHlwZSI6IkhQIiwidmFsdWUiOjQwMH0seyJ0cmFpdF90eXBlIjoiU3BlZWQiLCJ2YWx1ZSI6NX0seyJ0cmFpdF90eXBlIjoiSW50ZWxsaWdlbmNlIiwidmFsdWUiOjZ9LHsidHJhaXRfdHlwZSI6IkRlZmVuc2UiLCJ2YWx1ZSI6MTJ9LHsidHJhaXRfdHlwZSI6IkF0dGFjayIsInZhbHVlIjo1fSx7InRyYWl0X3R5cGUiOiJTcGVjaWFsIEF0dGFjayIsInZhbHVlIjoyMH0seyJ0cmFpdF90eXBlIjoiU3BlY2lhbCBEZWZlbnNlIiwidmFsdWUiOjEyfV19

        var regexGroups = Regex.Match(request.Result.ReturnValue1, "^data:application/json;base64[^\\w]+(.+)").Groups;
        if (regexGroups.Count > 1)
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(regexGroups[1].Value));
            return JsonUtility.FromJson<CardMetadata>(decoded);
        }
        Debug.LogWarning("Unable to parse metadata of tokenId " + tokenId);
        return null;
    }

    /// <summary>
    /// Returns a list of owned cards by checking the owner of a list of card IDs.
    /// </summary>
    /// <param name="address">User address</param>
    /// <param name="tokenIds">Card IDs to be checked</param>
    /// <returns>List of IDs of owned cards</returns>
    public static async Task<List<ulong>> GetOwnedCards(string address, List<ulong> tokenIds)
    {
        var request = new QueryUnityRequest<BalanceOfBatchFunction, BalanceOfBatchOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        // using NFTsOfUserUnityRequest fails because PepemonFactory uses ERC1155 but NFTsOfUserUnityRequest is for ERC721
        await request.Query(
            new BalanceOfBatchFunction
            {
                Ids = tokenIds, 
                Owners = Enumerable.Repeat(address, tokenIds.Count).ToList()
            },
            Address);

        var ownedCards = new List<ulong>();
        for (int i = 0; i < (request.Result?.ReturnValue1?.Count ?? 0); i++)
            if (request.Result.ReturnValue1[i] > 0)
                ownedCards.Add(tokenIds[i]);

        return ownedCards;
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
