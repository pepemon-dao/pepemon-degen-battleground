using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Contracts.PepemonCardDeck.abi.ContractDefinition;
using Thirdweb;
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
    private static string Abi => "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"approved\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"previousOwner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"OwnershipTransferred\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"MAX_SUPPORT_CARDS\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"MIN_SUPPORT_CARDS\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"battleCardId\",\"type\":\"uint256\"}],\"name\":\"addBattleCardToDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"supportCardId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"internalType\":\"struct PepemonCardDeck.SupportCardRequest[]\",\"name\":\"supportCards\",\"type\":\"tuple[]\"}],\"name\":\"addSupportCardsToDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"battleCardAddress\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"createDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"decks\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"battleCardId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"supportCardCount\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"}],\"name\":\"getAllSupportCardsInDeck\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"getApproved\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"}],\"name\":\"getBattleCardInDeck\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"}],\"name\":\"getCardTypesInDeck\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_cardTypeId\",\"type\":\"uint256\"}],\"name\":\"getCountOfCardTypeInDeck\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getDeckCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"deckId\",\"type\":\"uint256\"}],\"name\":\"getSupportCardCountInDeck\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"mintCards\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"},{\"internalType\":\"bytes\",\"name\":\"\",\"type\":\"bytes\"}],\"name\":\"onERC1155BatchReceived\",\"outputs\":[{\"internalType\":\"bytes4\",\"name\":\"\",\"type\":\"bytes4\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"\",\"type\":\"bytes\"}],\"name\":\"onERC1155Received\",\"outputs\":[{\"internalType\":\"bytes4\",\"name\":\"\",\"type\":\"bytes4\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"ownerOf\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"playerToDecks\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"}],\"name\":\"removeBattleCardFromDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"supportCardId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"internalType\":\"struct PepemonCardDeck.SupportCardRequest[]\",\"name\":\"_supportCards\",\"type\":\"tuple[]\"}],\"name\":\"removeSupportCardsFromDeck\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_battleCardAddress\",\"type\":\"address\"}],\"name\":\"setBattleCardAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_maxSupportCards\",\"type\":\"uint256\"}],\"name\":\"setMaxSupportCards\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_minSupportCards\",\"type\":\"uint256\"}],\"name\":\"setMinSupportCards\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"minCardId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxCardId\",\"type\":\"uint256\"}],\"name\":\"setMintingCards\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_supportCardAddress\",\"type\":\"address\"}],\"name\":\"setSupportCardAddress\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_deckId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_seed\",\"type\":\"uint256\"}],\"name\":\"shuffleDeck\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"supportCardAddress\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"tokenURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
    private static Contract contract => ThirdwebManager.Instance.SDK.GetContract(Address, Abi);

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
        return await contract.ERC1155.IsApprovedForAll(Address, operatorAddress);
    }


    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        await contract.ERC1155.SetApprovalForAll(operatorAddress, approved);
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
