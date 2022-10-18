using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PepemonCardDeck
{
    private const string AbiPath = "abi/PepemonCardDeck.abi";
    private static readonly string abi = Resources.Load<TextAsset>(AbiPath).text;
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;

    public static async Task<ulong> GetBattleCard(ulong deckId)
    {
        return await Web3Controller.instance.provider.CallContract<ulong>(
            new Web3CallContractArgs
            {
                abi = abi,
                contract = Address,
                method = "getBattleCardInDeck",
                parameters = new object[] { deckId }
            }
        );
    }

    public static async Task<List<ulong>> GetAllSupportCards(ulong deckId)
    {
        return await Web3Controller.instance.provider.CallContract<List<ulong>>(
            new Web3CallContractArgs
            {
                abi = abi,
                contract = Address,
                method = "getAllSupportCardsInDeck",
                parameters = new object[] { deckId }
            }
        );
    }

    public static async Task<List<ulong>> GetPlayerDecks(string address)
    {
        ulong deckCount = await Web3Controller.instance.provider.CallContract<ulong>(
            new Web3CallContractArgs
            {
                abi = abi,
                contract = Address,
                method = "getDeckCount",
                parameters = new object[] { address }
            }
        );

        List<ulong> deckIds = new List<ulong>();
        for (ulong i = 0; i < deckCount; i++)
        {
            ulong deckId = await Web3Controller.instance.provider.CallContract<ulong>(
                new Web3CallContractArgs
                {
                    abi = abi,
                    contract = Address,
                    method = "playerToDecks",
                    parameters = new object[] { address, i }
                }
            );
            deckIds.Add(deckId);
        }

        return deckIds;
    }

    public static async Task<ulong> GetMaxSupportCards()
    {
        return await Web3Controller.instance.provider.CallContract<ulong>(
            new Web3CallContractArgs
            {
                abi = abi,
                contract = Address,
                method = "MAX_SUPPORT_CARDS",
                parameters = new object[] { }
            }
        );
    }

    public static async Task<string> CreateDeck()
    {
        return await Web3Controller.instance.provider.SendContract(
            new Web3SendContractArgs
            {
                abi = abi,
                contract = Address,
                method = "createDeck",
                parameters = new object[] { },
                value = "0",
                gasLimit = "500000",
                gasPrice = "10000"
            }
        );
    }

    public static async Task<string> SetBattleCard(ulong deckId, ulong battleCardId)
    {
        return await Web3Controller.instance.provider.SendContract(
            new Web3SendContractArgs
            {
                abi = abi,
                contract = Address,
                method = "addBattleCardToDeck",
                parameters = new object[] { deckId, battleCardId },
                value = "0",
                gasLimit = "500000",
                gasPrice = "10000"
            }
        );
    }

    public static async Task<string> RemoveBattleCard(ulong deckId)
    {
        return await Web3Controller.instance.provider.SendContract(
            new Web3SendContractArgs
            {
                abi = abi,
                contract = Address,
                method = "removeBattleCardFromDeck",
                parameters = new object[] { deckId },
                value = "0",
                gasLimit = "500000",
                gasPrice = "10000"
            }
        );
    }

    public static async Task<string> AddSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if max support cards will be reached
        // TODO: Check if the player has the cards
        return await Web3Controller.instance.provider.SendContract(
            new Web3SendContractArgs
            {
                abi = abi,
                contract = Address,
                method = "addSupportCardsToDeck",
                parameters = new object[] { deckId, requests },
                value = "0",
                gasLimit = "500000",
                gasPrice = "10000"
            }
        );
    }

    public static async Task<string> RemoveSupportCards(ulong deckId, params SupportCardRequest[] requests)
    {
        // TODO: Check if the deck has requested cards before removing
        return await Web3Controller.instance.provider.SendContract(
            new Web3SendContractArgs
            {
                abi = abi,
                contract = Address,
                method = "removeSupportCardsFromDeck",
                parameters = new object[] { deckId, requests },
                value = "0",
                gasLimit = "500000",
                gasPrice = "10000"
            }
        );
    }

    [Serializable]
    public struct SupportCardRequest
    {
        public ulong supportCardId;
        public ulong amount;
    }
}
