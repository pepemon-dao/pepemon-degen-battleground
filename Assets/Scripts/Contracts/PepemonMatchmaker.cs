using Contracts.PepemonMatchmaker.abi.ContractDefinition;
using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class PepemonMatchmaker
{
    public enum PepemonLeagues
    {
        Rice = 0,
        PepeKarp,
        Chad
    }

    /// <summary>
    /// PepemonMatchmaker address
    /// </summary>
    private static string[] Addresses => Web3Controller.instance.GetChainConfig().pepemonMatchmakerAddresses;

    public static async Task EnterBattle(PepemonLeagues league, ulong deckId)
    {
        var request = Web3Controller.instance.GetContractTransactionUnityRequest();
        await request.SignAndSendTransactionAsync(
            new EnterFunction()
            {
                DeckId = deckId,
            },
            Addresses[(int)league]);
    }
}
