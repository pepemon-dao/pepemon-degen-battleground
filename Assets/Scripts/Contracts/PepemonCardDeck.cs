using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Contracts.Standards.ERC721;
using Nethereum.Unity.Rpc;
using Sirenix.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;

// The ABI definitions were generated using this command:
// Nethereum.Generator.Console generate from-abi --abiPath C:\Projects\pepemon-battle-degen\Assets\Resources\abi\PepemonCardDeck.abi.json --outputPath C:\Projects\pepemon-battle-degen\Assets\Scripts\Contracts\ --namespace Contracts
// Note: The generated code requires some small changes to work, like renaming the class pepemonCardDeck.abiDeployment to pepemonCardDeckAbiDeployment
// as well as removing the .abiService.cs C# code, which is not used because relies on Web3 namespace, which is incompatible with WebGL.

public class PepemonCardDeck
{
    /// <summary>
    /// PepemonCardDeck Contract Address
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

    // reference implementation for Read operations: https://github.com/Nethereum/Nethereum/blob/master/src/Nethereum.Unity/Contracts/Standards/ERC721/NFTsOfUserUnityRequest.cs
    public static async Task<ulong> GetBattleCard(ulong deckId)
    {
        var request = new QueryUnityRequest<GetBattleCardInDeckFunction, GetBattleCardInDeckOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        await request.Query(
            new GetBattleCardInDeckFunction { DeckId = deckId },
            Address);

        return (ulong)request.Result.ReturnValue1;
    }

    public static async Task<List<ulong>> GetAllSupportCards(ulong deckId)
    {
        var request = new QueryUnityRequest<GetAllSupportCardsInDeckFunction, GetAllSupportCardsInDeckOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        await request.Query(
            new GetAllSupportCardsInDeckFunction { DeckId = deckId },
            Address);

        return request.Result.SupportCards;
    }

    public static async Task<List<ulong>> GetPlayerDecks(string address)
    {
        var getDeckCountRequest = new QueryUnityRequest<GetDeckCountFunction, GetDeckCountOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Address);

        await getDeckCountRequest.Query(
            new GetDeckCountFunction { Player = address },
            Address);

        var deckCount = getDeckCountRequest.Result.DeckCount;

        List<ulong> deckIds = new List<ulong>();
        for (ulong i = 0; i < deckCount; i++)
        {
            var playerToDecksRequest = new QueryUnityRequest<PlayerToDecksFunction, PlayerToDecksOutputDTO>(
                Web3Controller.instance.GetUnityRpcRequestClientFactory(),
                Address);

            await playerToDecksRequest.Query(
                new PlayerToDecksFunction()
                {
                    Player = address,
                    DeckIdx = i
                },
                Address);

            deckIds.Add((ulong)playerToDecksRequest.Result.DeckId);
        }

        return deckIds;
    }

    public static async Task<ulong> GetMaxSupportCards()
    {
        var request = new QueryUnityRequest<MaxSupportCardsFunction, MaxSupportCardsOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Address);

        // TODO: check request.Exception
        await request.Query(new MaxSupportCardsFunction(), Address);

        return (ulong)request.Result.ReturnValue1;
    }

    // reference implementation for Write operations: https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/Assets/MetamaskController.cs

    public static async Task<string> CreateDeck()
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        // note: deck is created using the sender's address
        await request.SignAndSendTransaction(new CreateDeckFunction(), Address);
        return request.Result;
    }

    public static async Task<string> SetBattleCard(ulong deckId, ulong battleCardId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransaction(
            new AddBattleCardToDeckFunction()
            {
                DeckId = deckId,
                BattleCardId = battleCardId,
            },
            Address);
        return request.Result;
    }

    public static async Task<string> RemoveBattleCard(ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransaction(
            new RemoveBattleCardFromDeckFunction()
            {
                DeckId = deckId
            },
            Address);
        return request.Result;
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <returns>Transaction result</returns>
    public static async Task<string> SetApprovalState(bool approved)
    {
        var approvalRequest = Web3Controller.instance.GetContractTransactionUnityRequest();
        await approvalRequest.SignAndSendTransaction(
            new Contracts.PepemonFactory.abi.ContractDefinition.SetApprovalForAllFunction()
            {
                Operator = Address,
                Approved = approved
            },
            Web3Controller.instance.GetChainConfig().pepemonFactoryAddress);
        return approvalRequest.Result;
    }

    /// <summary>
    /// Adds support card to deck. Requires prior Approval
    /// </summary>
    public static async Task<string> AddSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if max support cards will be reached
        // TODO: Check if the player has the cards
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransaction(
            new AddSupportCardsToDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);

        return request.Result;
    }

    /// <summary>
    /// Removes support card from deck. Requires prior Approval
    /// </summary>
    public static async Task<string> RemoveSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if the deck has requested cards before removing
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransaction(
            new RemoveSupportCardsFromDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);
        return request.Result;
    }
}
