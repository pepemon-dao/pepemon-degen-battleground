using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Pepemon.Store;
using Thirdweb;
using UnityEngine;

/// <summary>
/// Booster pack store contract.
///
/// Packs mint onto the same PepemonFactory the game already reads, so cards bought here appear
/// in the deck editor with no extra step. Nothing about decks, battles or the matchmaker changes.
///
/// The ABI lives here rather than in Web3Settings because it is a property of the contract
/// source, not of a particular deployment - unlike the address, which is per chain. That also
/// keeps a large JSON blob out of scene YAML, where a hand-editing slip stays invisible until a
/// build runs.
/// </summary>
public static class PepemonBoosterPack
{
    /// <summary>
    /// Pack gas is data-dependent: the roll decides which cards move, and which storage slots
    /// are already warm decides what that costs. The seed also changes between the wallet's gas
    /// estimation and execution, so an estimate can be made against a cheaper roll than the one
    /// that actually runs. Measured cost settles around 1.3-1.5M with a first-purchase spike
    /// near 2.9M, so this is roughly double the worst observed case. Without the explicit limit,
    /// purchases fail intermittently and the player pays for nothing.
    /// </summary>
    public const long GasLimit = 6000000;

    private const string Abi =
        @"[{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""buyer"",""type"":""address""},{""indexed"":true,""internalType"":""uint8"",""name"":""tier"",""type"":""uint8""},{""indexed"":false,""internalType"":""uint256[]"",""name"":""cardIds"",""type"":""uint256[]""}],""name"":""PackOpened"",""type"":""event""},{""inputs"":[{""internalType"":""uint8"",""name"":""tierId"",""type"":""uint8""}],""name"":""mintPack"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""},{""inputs"":[{""internalType"":""uint8"",""name"":""tierId"",""type"":""uint8""}],""name"":""packSize"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint8"",""name"":""tierId"",""type"":""uint8""}],""name"":""tierEnabled"",""outputs"":[{""internalType"":""bool"",""name"":"""",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint8"",""name"":""tierId"",""type"":""uint8""}],""name"":""tierPrice"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""}]";

    /// <summary>One tier as shown in the store: shop copy merged with live contract values.</summary>
    public class Tier
    {
        public byte Id;
        public string Name;
        public string Tagline;
        public string Contents;
        public BigInteger PriceWei;
        public int CardCount;

        public string PriceEth => EthDisplay.FormatEth(PriceWei);
    }

    public static string Address
    {
        get
        {
            var controller = Web3Controller.instance;
            if (controller == null) return null;
            return controller.GetChainConfig().pepemonBoosterPackAddress;
        }
    }

    /// <summary>
    /// False until a booster contract has been deployed and its address configured for the
    /// current chain. The store shows an explanatory state rather than throwing.
    /// </summary>
    public static bool IsConfigured
    {
        get
        {
            try
            {
                var address = Address;
                return !string.IsNullOrWhiteSpace(address) &&
                       address != "0x0000000000000000000000000000000000000000";
            }
            catch (Exception)
            {
                // GetChainConfig throws for a chain id with no settings entry.
                return false;
            }
        }
    }

    private static Contract Contract => ThirdwebManager.Instance.SDK.GetContract(Address, Abi);

    /// <summary>
    /// Reads each tier's live price and size. Tiers the contract reports as disabled are left
    /// out, so switching one off on chain removes it from the store with no client change.
    /// </summary>
    public static async Task<List<Tier>> GetTiers()
    {
        var tiers = new List<Tier>();

        foreach (var info in PackCatalogue.Tiers)
        {
            try
            {
                if (!await Contract.Read<bool>("tierEnabled", info.Id)) continue;

                var price = await Contract.Read<BigInteger>("tierPrice", info.Id);
                var size = await Contract.Read<BigInteger>("packSize", info.Id);

                tiers.Add(new Tier
                {
                    Id = info.Id,
                    Name = info.Name,
                    Tagline = info.Tagline,
                    Contents = info.Contents,
                    PriceWei = price,
                    CardCount = (int)size,
                });
            }
            catch (Exception ex)
            {
                // One unreadable tier must not take down the whole store.
                Debug.LogError($"[store] Could not read tier {info.Id}: {ex.Message}");
            }
        }

        return tiers;
    }

    /// <summary>
    /// Buys and opens one pack. The price must match the contract exactly - it rejects both
    /// under- and overpayment - so it is always the value read from chain, never an assumption.
    /// </summary>
    public static async Task<TransactionResult> MintPack(byte tierId, BigInteger priceWei)
    {
        var overrides = new TransactionRequest
        {
            value = priceWei.ToString(CultureInfo.InvariantCulture),
            gasLimit = GasLimit.ToString(CultureInfo.InvariantCulture),
        };

        return await Contract.Write("mintPack", overrides, tierId);
    }
}
