using Contracts.PepemonFactory.abi.ContractDefinition;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            Web3Controller.instance.GetReadOnlyRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        UriOutputDTO response;
        try
        {
            response = await request.QueryAsync(
                        new UriFunction { Id = tokenId },
                        Address);
        }
        catch (Exception e)
        {
            // Usually tokenId was not found
            Debug.LogException(e);
            return null;
        }

        // example of ReturnValue1:
        // data:application/json;base64\r\n\r\neyJwb29sIjogeyJuYW1lIjogInJvb3QiLCJwb2ludHMiOiAxfSwiZXh0ZXJuYWxfdXJsIjogImh0dHBzOi8vcGVwZW1vbi53b3JsZC8iLCJpbWFnZSI6ICJodHRwczovL2JhZnliZWljNmJkbnRoanA0djU0c3JtN3JvbHp0ZGRqaDRzb2dxajN1Y3V6eXVha3J1dHNqdjY3b21tLmlwZnMuZHdlYi5saW5rL2JmYWZueWNhcmQucG5nIiwibmFtZSI6ICJGYWZueSIsImRlc2NyaXB0aW9uIjogIkZhZm55IChCYXR0bGUgdmVyLikiLCJhdHRyaWJ1dGVzIjpbeyJ0cmFpdF90eXBlIjoiU2V0IiwidmFsdWUiOiJQZXBlbW9uIEJhdHRsZSJ9LHsidHJhaXRfdHlwZSI6IkxldmVsIiwidmFsdWUiOjF9LHsidHJhaXRfdHlwZSI6IkVsZW1lbnQiLCJ2YWx1ZSI6IkZpcmUifSx7InRyYWl0X3R5cGUiOiJXZWFrbmVzcyIsInZhbHVlIjoiV2F0ZXIifSx7InRyYWl0X3R5cGUiOiJSZXNpc3RhbmNlIiwidmFsdWUiOiJHcmFzcyJ9LHsidHJhaXRfdHlwZSI6IkhQIiwidmFsdWUiOjQwMH0seyJ0cmFpdF90eXBlIjoiU3BlZWQiLCJ2YWx1ZSI6NX0seyJ0cmFpdF90eXBlIjoiSW50ZWxsaWdlbmNlIiwidmFsdWUiOjZ9LHsidHJhaXRfdHlwZSI6IkRlZmVuc2UiLCJ2YWx1ZSI6MTJ9LHsidHJhaXRfdHlwZSI6IkF0dGFjayIsInZhbHVlIjo1fSx7InRyYWl0X3R5cGUiOiJTcGVjaWFsIEF0dGFjayIsInZhbHVlIjoyMH0seyJ0cmFpdF90eXBlIjoiU3BlY2lhbCBEZWZlbnNlIiwidmFsdWUiOjEyfV19

        var regexGroups = Regex.Match(response.ReturnValue1, "^data:application/json;base64[^\\w]+(.+)").Groups;
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
    public static async Task<Dictionary<ulong, int>> GetOwnedCards(string address, List<ulong> tokenIds)
    {
        var request = new QueryUnityRequest<BalanceOfBatchFunction, BalanceOfBatchOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        // using NFTsOfUserUnityRequest fails because PepemonFactory uses ERC1155 but NFTsOfUserUnityRequest is for ERC721
        var response = await request.QueryAsync(
            new BalanceOfBatchFunction
            {
                Ids = tokenIds,
                Owners = Enumerable.Repeat(address, tokenIds.Count).ToList()
            },
            Address);

        var ownedCards = new Dictionary<ulong, int>();
        for (int i = 0; i < (response.ReturnValue1?.Count ?? 0); i++)
            if (response.ReturnValue1[i] > 0)
                ownedCards[tokenIds[i]] = (int)response.ReturnValue1[i];

        return ownedCards;
    }

    public static async Task<BigInteger> GetTokenSupply(ulong tokenId)
    {
        var request = new QueryUnityRequest<TotalSupplyFunction, TotalSupplyOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new TotalSupplyFunction { Id = tokenId }, Address);
        return response.ReturnValue1;
    }

    public static async Task<ulong> FindMaxTokenId(ulong parallelBatchSize = 5)
    {
        ulong batch = 0;
        ulong maxTokenId = 0;
        ulong batchTokenMaxId = 0;

        do
        {
            batchTokenMaxId = 0;
            List<Task<ulong>> tasks = new List<Task<ulong>>();
            Debug.Log($"Checking supply of tokens {batch * parallelBatchSize}...{(batch + 1) * parallelBatchSize - 1}");
            for (ulong i = 0; i < parallelBatchSize; i++)
            {
                ulong tokenId = batch * parallelBatchSize + i;
                tasks.Add(GetTokenSupply(tokenId).ContinueWith(supply => supply.Result > 0 ? tokenId : 0));
            }
            await Task.WhenAll(tasks);

            batchTokenMaxId = tasks.Select(t => t.Result).Max();
            maxTokenId = Math.Max(maxTokenId, batchTokenMaxId);
            batch++;
        } while (batchTokenMaxId > 0);

        return maxTokenId;
    }

    /// <summary>
    /// Tells whether or not a contract/wallet can transfer NFTs
    /// </summary>
    /// <param name="operatorAddress">Address of the contract/wallet</param>
    /// <returns>result of IsApprovedForAll</returns>
    public static async Task<bool> GetApprovalState(string operatorAddress)
    {
        var request = new QueryUnityRequest<IsApprovedForAllFunction, IsApprovedForAllOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new IsApprovedForAllFunction()
            {
                Owner = Web3Controller.instance.SelectedAccountAddress,
                Operator = operatorAddress
            },
            Address);

        return response.IsOperator;
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <param name="operatorAddress">Contract/wallet which will be given/revoked permission to transfer the NFTs</param>
    /// <returns>Transaction hash</returns>
    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        var approvalRequest = Web3Controller.instance.GetContractTransactionUnityRequest();
        await approvalRequest.SendTransactionAndWaitForReceiptAsync(
            new SetApprovalForAllFunction()
            {
                Operator = operatorAddress,
                Approved = approved
            },
            Address);
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
