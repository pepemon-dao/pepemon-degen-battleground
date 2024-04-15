using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
//using Nethereum.Unity.Rpc;

// The ABI definitions were generated using this command:
// Nethereum.Generator.Console generate from-abi --abiPath C:\Projects\pepemon-battle-degen\Assets\Resources\abi\PepemonCardDeck.abi.json --outputPath C:\Projects\pepemon-battle-degen\Assets\Scripts\Contracts\ --namespace Contracts
// Note: The generated code requires some small changes to work, like renaming the class pepemonCardDeck.abiDeployment to pepemonCardDeckAbiDeployment
// as well as removing the .abiService.cs C# code, which is not used because relies on Web3 namespace, which is incompatible with WebGL.

public class PepemonCardDeck : ERC1155Common
{
    /// <summary>
    /// PepemonCardDeck Contract Address
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

    // reference implementation for Read operations: https://github.com/Nethereum/Nethereum/blob/master/src/Nethereum.Unity/Contracts/Standards/ERC721/NFTsOfUserUnityRequest.cs
    public static async Task<ulong> GetBattleCard(ulong deckId)
    {
        /* var request = new QueryUnityRequest<GetBattleCardInDeckFunction, GetBattleCardInDeckOutputDTO>(
             Web3Controller.instance.GetUnityRpcRequestClientFactory(),
             Web3Controller.instance.SelectedAccountAddress);

         var response = await request.QueryAsync(
                     new GetBattleCardInDeckFunction { DeckId = deckId },
                     Address);*/

        await Task.Delay(1);
        return (ulong)0;//response.ReturnValue1;
    }

    public static async Task<IDictionary<ulong, int>> GetAllSupportCards(ulong deckId)
    {
        /*var request = new QueryUnityRequest<GetAllSupportCardsInDeckFunction, GetAllSupportCardsInDeckOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new GetAllSupportCardsInDeckFunction { DeckId = deckId },
            Address);

        var result = new OrderedDictionary<ulong, int>();
        foreach (var card in response.SupportCards)
        {
            result[card] = result.ContainsKey(card) ? result[card] + 1 : 1;
        }*/

        await Task.Delay(1);
        return new Dictionary<ulong, int>();//result;
    }

    public static async Task<List<ulong>> GetPlayerDecks(string address)
    {
        /*var getDeckCountRequest = new QueryUnityRequest<GetDeckCountFunction, GetDeckCountOutputDTO>(
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
        }*/

        await Task.Delay(1);
        return new List<ulong>(); //deckIds;
    }

    public static async Task<ulong> GetMaxSupportCards()
    {

        await Task.Delay(1);
        /*var request = new QueryUnityRequest<MaxSupportCardsFunction, MaxSupportCardsOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new MaxSupportCardsFunction(), Address);*/
        return (ulong)0;//response.ReturnValue1;
    }

    public static async Task<bool> GetApprovalState(string operatorAddress)
    {
        return await GetApproval(Address, operatorAddress);
    }


    // reference implementation for Write operations: https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/Assets/MetamaskController.cs
    
    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        await SetApproval(Address, approved, operatorAddress);
    }

    public static async Task MintCards()
    {
        //var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        //await request.SendTransactionAndWaitForReceiptAsync(new MintCardsFunction(), Address);

        await Task.Delay(1);
    }

    public static async Task CreateDeck()
    {
        //var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        // note: deck is created using the sender's address
        //await request.SendTransactionAndWaitForReceiptAsync(new CreateDeckFunction(), Address);

        await Task.Delay(1);
    }

    public static async Task SetBattleCard(ulong deckId, ulong battleCardId)
    {
        /*var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SendTransactionAndWaitForReceiptAsync(
            new AddBattleCardToDeckFunction()
            {
                DeckId = deckId,
                BattleCardId = battleCardId,
            },
            Address);
        */

        await Task.Delay(1);
    }

    public static async Task RemoveBattleCard(ulong deckId)
    {
        //var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        //await request.SendTransactionAndWaitForReceiptAsync(new RemoveBattleCardFromDeckFunction() { DeckId = deckId }, Address);

        await Task.Delay(1);
    }

    /// <summary>
    /// Adds support card to deck. Requires prior Approval
    /// </summary>
    public static async Task AddSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if max support cards will be reached
        // TODO: Check if the player has the cards
        /*var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SendTransactionAndWaitForReceiptAsync(
            new AddSupportCardsToDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);*/

        await Task.Delay(1);
    }

    /// <summary>
    /// Removes support card from deck. Requires prior Approval
    /// </summary>
    public static async Task RemoveSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if the deck has requested cards before removing
        /*var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SendTransactionAndWaitForReceiptAsync(
            new RemoveSupportCardsFromDeckFunction()
            {
                DeckId = deckId,
                SupportCards = new List<SupportCardRequest>(requests),
            },
            Address);*/

        await Task.Delay(1);
    }
}
