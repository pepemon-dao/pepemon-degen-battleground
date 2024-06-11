using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;

class PepemonFactory
{
    /// <summary>
    /// PepemonFactory address (Support cards, Battle cards)
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonFactoryAddress;
    private static string Abi => Web3Controller.instance.GetChainConfig().pepemonFactoryAbi;
    private static Contract contract => ThirdwebManager.Instance.SDK.GetContract(Address, Abi);

    /// <summary>
    /// Gets the specified card's metadata
    /// </summary>
    /// <returns></returns>
    public static async Task<CardMetadata?> GetCardMetadata(ulong tokenId)
    {
        try
        {
            var response = await contract.Read<string>("uri", tokenId);
            var regexGroups = Regex.Match(response, "^data:application/json;base64[^\\w]+(.+)").Groups;
            if (regexGroups.Count > 1)
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(regexGroups[1].Value));
                return JsonUtility.FromJson<CardMetadata>(decoded);
            }
        }
        catch (Exception e)
        {
            // Usually tokenId was not found
            Debug.LogException(e);
            return null;
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
        var response = await contract.Read<List<ulong>>("balanceOfBatch", Enumerable.Repeat(address, tokenIds.Count).ToList(), tokenIds);
        var ownedCards = new Dictionary<ulong, int>();
        for (int i = 0; i < (response?.Count ?? 0); i++)
            if (response[i] > 0)
                ownedCards[tokenIds[i]] = (int)response[i];

        return ownedCards;
    }

    public static async Task<BigInteger> GetTokenSupply(ulong tokenId)
    {
        return await contract.Read<BigInteger>("totalSupply", tokenId);
    }

    public static async Task<ulong> FindMaxTokenId(ulong parallelBatchSize = 5)
    {
        ulong batch = 0;
        ulong maxTokenId = 0;
        ulong batchTokenMaxId = 0;
        do
        {
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
        // cant use contract.ERC1155.IsApprovedForAll because it fails in WebGL
        var playerWalletAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        if (string.IsNullOrEmpty(playerWalletAddress))
        {
            Debug.LogWarning("Unable to call 'isApprovedForAll': Player wallet was not set");
            return false;
        }
        return await contract.Read<bool>("isApprovedForAll", playerWalletAddress, operatorAddress);
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <param name="operatorAddress">Contract/wallet which will be given/revoked permission to transfer the NFTs</param>
    /// <returns>Transaction hash</returns>
    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        // cant use contract.ERC1155.SetApprovalForAll because it fails in WebGL
        await contract.Write("setApprovalForAll", operatorAddress, approved);
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
