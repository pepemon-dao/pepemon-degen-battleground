using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pepemon.BattleRules;
using Thirdweb;
using UnityEngine;

namespace Pepemon.Onboarding
{
    /// <summary>What the claim actually managed to do. Every field is independently meaningful.</summary>
    public class StarterPackClaimResult
    {
        public bool CardsMinted;
        public bool DeckCreated;
        public ulong DeckId;
        public bool BattleCardSet;
        public int SupportCardsAdded;
        public string Error;

        /// <summary>
        /// True when the player can take this deck straight into a battle. Anything less and
        /// they are dropped into the deck builder, which is the drop-off we are trying to avoid.
        /// </summary>
        public bool DeckIsPlayable => DeckCreated && BattleCardSet && SupportCardsAdded > 0;

        /// <summary>The irreversible part succeeded, so the claim must not be retried blindly.</summary>
        public bool AssetsGranted => CardsMinted && DeckCreated;
    }

    /// <summary>
    /// Mints the starter pack and assembles it into a ready-to-play deck.
    ///
    /// Minting alone is not a reward: it leaves the player holding loose cards and an empty
    /// deck, facing a four-transaction deck builder before they can play again. This runs the
    /// whole sequence so the player lands with something playable.
    ///
    /// The card ids granted by mintCards() are not knowable from the ABI, and the local
    /// ScriptableObject ids are not guaranteed to match on-chain token ids. So rather than
    /// assuming, this reads back actual balances and assembles from what the player provably
    /// owns. If nothing matches, the cards and deck are still granted and the player is told
    /// to finish in the deck builder - degraded, but never a lie.
    /// </summary>
    public static class StarterPackClaim
    {
        private const int DeckPropagationTimeoutMs = 20000;
        private const int DeckPropagationPollMs = 1000;
        private const int DefaultMaxSupportCards = 60;

        public static async Task<StarterPackClaimResult> Run(
            ulong pepemonId,
            IReadOnlyList<ulong> desiredSupportCardIds,
            Action<string> onStatus)
        {
            var result = new StarterPackClaimResult();
            void Status(string m) => onStatus?.Invoke(m);

            string address;
            try
            {
                address = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            }
            catch (Exception e)
            {
                result.Error = $"could not read wallet address: {e.Message}";
                return result;
            }

            if (string.IsNullOrEmpty(address))
            {
                result.Error = "no wallet address";
                return result;
            }

            ulong deckCountBefore;
            try
            {
                deckCountBefore = await PepemonCardDeck.GetDeckCount(address);
            }
            catch (Exception e)
            {
                // Not fatal on its own, but without a baseline the new deck cannot be identified.
                Debug.LogWarning($"[claim] could not read deck count: {e.Message}");
                deckCountBefore = 0;
            }

            // ---- 1. Mint the cards -------------------------------------------------------
            try
            {
                Status("Minting your cards...");
                await PepemonCardDeck.MintCards();
                result.CardsMinted = true;
            }
            catch (Exception e)
            {
                result.Error = $"mintCards failed: {e.Message}";
                return result;
            }

            // ---- 2. Create the deck that will hold them ----------------------------------
            try
            {
                Status("Creating your deck...");
                await PepemonCardDeck.CreateDeck();
                result.DeckCreated = true;
            }
            catch (Exception e)
            {
                result.Error = $"createDeck failed: {e.Message}";
                return result;
            }

            // ---- 3. Find the new deck id -------------------------------------------------
            // Polled rather than delayed by a fixed sleep: propagation time is not predictable
            // and a too-short wait silently assembles nothing.
            try
            {
                Status("Finding your new deck...");
                result.DeckId = await WaitForNewDeck(address, deckCountBefore);
            }
            catch (Exception e)
            {
                result.Error = $"could not resolve new deck: {e.Message}";
                return result;
            }

            if (result.DeckId == 0)
            {
                result.Error = "new deck did not appear in time";
                return result;
            }

            // ---- 4. Let the deck contract move the player's cards -------------------------
            try
            {
                var deckAddress = Web3Controller.instance.GetChainConfig().pepemonCardDeckAddress;
                if (!await PepemonFactory.GetApprovalState(deckAddress))
                {
                    Status("Approving card transfers...");
                    await PepemonFactory.SetApprovalState(true, deckAddress);
                }
            }
            catch (Exception e)
            {
                result.Error = $"approval failed: {e.Message}";
                return result;
            }

            // ---- 5. Assemble from what the player provably owns ---------------------------
            var candidateIds = new List<ulong> { pepemonId };
            foreach (var id in desiredSupportCardIds)
            {
                if (!candidateIds.Contains(id)) candidateIds.Add(id);
            }

            Dictionary<ulong, int> owned;
            try
            {
                Status("Checking your new cards...");
                owned = await PepemonFactory.GetOwnedCards(address, candidateIds);
            }
            catch (Exception e)
            {
                result.Error = $"could not read owned cards: {e.Message}";
                return result;
            }

            if (owned == null || owned.Count == 0)
            {
                result.Error = "minted cards did not match the starter list";
                return result;
            }

            if (owned.ContainsKey(pepemonId))
            {
                try
                {
                    Status("Adding your Pepemon...");
                    await PepemonCardDeck.SetBattleCard(result.DeckId, pepemonId);
                    result.BattleCardSet = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[claim] setBattleCard failed: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[claim] player does not own Pepemon {pepemonId}; skipping battle card.");
            }

            var requests = BuildSupportCardRequests(desiredSupportCardIds, owned, await ReadMaxSupportCards());
            if (requests.Count > 0)
            {
                try
                {
                    Status("Building your deck...");
                    await PepemonCardDeck.AddSupportCards(result.DeckId, requests.ToArray());

                    foreach (var r in requests) result.SupportCardsAdded += (int)r.Amount;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[claim] addSupportCards failed: {e.Message}");
                }
            }

            return result;
        }

        private static async Task<int> ReadMaxSupportCards()
        {
            try
            {
                var max = await PepemonCardDeck.GetMaxSupportCards();
                return max > 0 ? max : DefaultMaxSupportCards;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[claim] could not read MAX_SUPPORT_CARDS, using {DefaultMaxSupportCards}: {e.Message}");
                return DefaultMaxSupportCards;
            }
        }

        /// <summary>
        /// Turns the desired starter list into transfer requests. Selection rules live in
        /// <see cref="StarterDeckAssembly"/> so they can be tested; this only adapts the result
        /// to the contract's request type.
        /// </summary>
        internal static List<PepemonCardDeck.SupportCardRequest> BuildSupportCardRequests(
            IReadOnlyList<ulong> desiredIds,
            IReadOnlyDictionary<ulong, int> owned,
            int maxSupportCards)
        {
            var selection = StarterDeckAssembly.SelectSupportCards(
                desiredIds,
                owned,
                maxSupportCards > 0 ? maxSupportCards : DefaultMaxSupportCards);

            var requests = new List<PepemonCardDeck.SupportCardRequest>(selection.Count);
            foreach (var kvp in selection)
            {
                requests.Add(new PepemonCardDeck.SupportCardRequest
                {
                    SupportCardId = kvp.Key,
                    Amount = kvp.Value
                });
            }

            return requests;
        }

        private static async Task<ulong> WaitForNewDeck(string address, ulong deckCountBefore)
        {
            var waited = 0;
            while (waited < DeckPropagationTimeoutMs)
            {
                // Realtime, not scaled game time: this waits on the chain, not on the game, and
                // a stray timeScale of 0 would otherwise hang the poll forever.
                await UniTask.Delay(DeckPropagationPollMs, DelayType.Realtime);
                waited += DeckPropagationPollMs;

                ulong count;
                try
                {
                    count = await PepemonCardDeck.GetDeckCount(address);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[claim] deck count poll failed: {e.Message}");
                    continue;
                }

                if (count <= deckCountBefore) continue;

                // The newest deck is the last entry in the player's list.
                return await PepemonCardDeck.GetPlayerDeckAt(address, count - 1);
            }

            return 0;
        }
    }
}
