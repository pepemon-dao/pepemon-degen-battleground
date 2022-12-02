using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Nethereum.Unity.Rpc;

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
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
                    new GetBattleCardInDeckFunction { DeckId = deckId },
                    Address);
        return (ulong)response.ReturnValue1;
    }

    public static async Task<Dictionary<ulong, int>> GetAllSupportCards(ulong deckId)
    {
        var request = new QueryUnityRequest<GetAllSupportCardsInDeckFunction, GetAllSupportCardsInDeckOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new GetAllSupportCardsInDeckFunction { DeckId = deckId },
            Address);

        var result = new Dictionary<ulong, int>();
        foreach (var card in response.SupportCards)
        {
            result[card] = result.ContainsKey(card) ? result[card] + 1 : 1;
        }
        return result;
    }

    public static async Task<List<ulong>> GetPlayerDecks(string address)
    {
        var getDeckCountRequest = new QueryUnityRequest<GetDeckCountFunction, GetDeckCountOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var getDeckCountResponse = await getDeckCountRequest.QueryAsync(
            new GetDeckCountFunction { Player = address },
            Address);

        var deckCount = getDeckCountResponse.DeckCount;

        List<ulong> deckIds = new List<ulong>();
        for (ulong i = 0; i < deckCount; i++)
        {
            var playerToDecksRequest = new QueryUnityRequest<PlayerToDecksFunction, PlayerToDecksOutputDTO>(
                Web3Controller.instance.GetUnityRpcRequestClientFactory(),
                Web3Controller.instance.SelectedAccountAddress);

            var playerToDecksResponse = await playerToDecksRequest.QueryAsync(
                new PlayerToDecksFunction()
                {
                    Player = address,
                    DeckIdx = i
                },
                Address);

            deckIds.Add((ulong)playerToDecksResponse.DeckId);
        }

        return deckIds;
    }

    public static async Task<ulong> GetMaxSupportCards()
    {
        var request = new QueryUnityRequest<MaxSupportCardsFunction, MaxSupportCardsOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new MaxSupportCardsFunction(), Address);
        return (ulong)response.ReturnValue1;
    }

    public static async Task<bool> GetApprovalState()
    {
        var request = new QueryUnityRequest<IsApprovedForAllFunction, IsApprovedForAllOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Web3Controller.instance.SelectedAccountAddress);
        var response = await request.QueryAsync(
            new IsApprovedForAllFunction()
            {
                Owner = Web3Controller.instance.SelectedAccountAddress,
                Operator = Address
            },
            Address);
        return response.ReturnValue1;
    }

    // reference implementation for Write operations: https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/Assets/MetamaskController.cs

    public static async Task<string> CreateDeck()
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        // note: deck is created using the sender's address
        return await request.SignAndSendTransactionAsync(new CreateDeckFunction(), Address);
    }

    public static async Task<string> SetBattleCard(ulong deckId, ulong battleCardId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        return await request.SignAndSendTransactionAsync(
            new AddBattleCardToDeckFunction()
            {
                DeckId = deckId,
                BattleCardId = battleCardId,
            },
            Address);
    }

    public static async Task<string> RemoveBattleCard(ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        return await request.SignAndSendTransactionAsync(new RemoveBattleCardFromDeckFunction() { DeckId = deckId }, Address);
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <returns>Transaction result</returns>
    public static async Task<string> SetApprovalState(bool approved)
    {
        var approvalRequest = Web3Controller.instance.GetContractTransactionUnityRequest();
        return await approvalRequest.SignAndSendTransactionAsync(
            new Contracts.PepemonFactory.abi.ContractDefinition.SetApprovalForAllFunction()
            {
                Operator = Address,
                Approved = approved
            },
            Web3Controller.instance.GetChainConfig().pepemonFactoryAddress);
    }

    /// <summary>
    /// Adds support card to deck. Requires prior Approval
    /// </summary>
    public static async Task<string> AddSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if max support cards will be reached
        // TODO: Check if the player has the cards
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        return await request.SignAndSendTransactionAsync(
            new AddSupportCardsToDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);
    }

    /// <summary>
    /// Removes support card from deck. Requires prior Approval
    /// </summary>
    public static async Task<string> RemoveSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if the deck has requested cards before removing
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        return await request.SignAndSendTransactionAsync(
            new RemoveSupportCardsFromDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);
    }
}
