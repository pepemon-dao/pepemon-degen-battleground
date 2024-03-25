using Contracts.PepemonFactory.abi.ContractDefinition;
using Cysharp.Threading.Tasks;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

class PepemonFactory : ERC1155Common
{
    /// <summary>
    /// PepemonFactory address (Support cards, Battle cards)
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonFactoryAddress;

    /// <summary>
    /// Gets the stats of multiple battle cards
    /// </summary>
    /// <returns></returns>
    public static async UniTask<List<BattleCardStats>> BatchGetBattleCardStats(ulong minId, ulong maxId)
    {
        var request = new QueryUnityRequest<BatchGetBattleCardStatsFunction, BatchGetBattleCardStatsOutputDTO>(
            Web3Controller.instance.GetReadOnlyRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        BatchGetBattleCardStatsOutputDTO response;
        try
        {
            response = await request.QueryAsync(
                        new BatchGetBattleCardStatsFunction { MinId = minId, MaxId = maxId },
                        Address);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return null;
        }
        return response.ReturnValue1;
    }

    /// <summary>
    /// Gets the stats of multiple support cards
    /// </summary>
    /// <returns></returns>
    public static async UniTask<List<SupportCardStats>> BatchGetSupportCardStats(ulong minId, ulong maxId)
    {
        var request = new QueryUnityRequest<BatchGetSupportCardStatsFunction, BatchGetSupportCardStatsOutputDTO>(
            Web3Controller.instance.GetReadOnlyRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new BatchGetSupportCardStatsFunction { MinId = minId, MaxId = maxId },
            Address);

        return response.ReturnValue1;
    }

    public static async UniTask<ulong> GetLastCardId()
    {
        var request = new QueryUnityRequest<GetLastTokenIDFunction, GetLastTokenIDOutputDTO>(
            Web3Controller.instance.GetReadOnlyRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new GetLastTokenIDFunction(), Address);

        return (ulong)response.ReturnValue1;
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

    /// <summary>
    /// Tells whether or not a contract/wallet can transfer NFTs
    /// </summary>
    /// <param name="operatorAddress">Address of the contract/wallet</param>
    /// <returns>result of IsApprovedForAll</returns>
    public static async Task<bool> GetApprovalState(string operatorAddress)
    {
        return await GetApproval(Address, operatorAddress);
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <param name="operatorAddress">Contract/wallet which will be given/revoked permission to transfer the NFTs</param>
    /// <returns>Transaction hash</returns>
    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        await SetApproval(Address, approved, operatorAddress);
    }

    [Serializable]
    public struct CardMetadata
    {
        public string image;
        public string name;
        public string description;
        public bool isSupportCard;
    }
}
