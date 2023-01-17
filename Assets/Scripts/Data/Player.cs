using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Numerics;
using Nethereum.ABI;
using Nethereum.RLP;
using System;

[System.Serializable]
public class Player
{
    [BoxGroup("Player Loadout")] public Pepemon PlayerPepemon;
    [BoxGroup("Player Loadout")] public Deck PlayerDeck;

    [BoxGroup("Runtime")] public int CurrentHP;
    [BoxGroup("Runtime")] public Deck CurrentDeck;
    [BoxGroup("Runtime")] public Hand CurrentHand;
    [BoxGroup("Runtime")] public int StartingIndex;
    [BoxGroup("Runtime")] public int PlayedCardCount;
    [BoxGroup("Runtime")] public CurrentBattleCardStats CurrentPepemonStats = new(); // all stats of the player's battle cards currently


    public void Initialise(int index)
    {
        CurrentHP = PlayerPepemon.HealthPoints;
        StartingIndex = index;
    }

    public void SetPlayerDeck(Pepemon pepemon, IEnumerable<Card> supportCards)
    {
        PlayerPepemon = pepemon;
        PlayerDeck.ClearDeck();
        PlayerDeck.GetDeck().AddRange(supportCards);
    }

    public void GetAndShuffelDeck(BigInteger seed, BigInteger currentTurn, BigInteger battleRng)
    {
        // Get local copy of deck and shuffle
        CurrentDeck.ClearDeck();
        CurrentDeck.GetDeck().AddRange(PlayerDeck.GetDeck());

        // calculate random seed like in solidity
        var abiEncode = new ABIEncode();
        CurrentDeck.ShuffelDeck(
            abiEncode.GetSha3ABIEncodedPacked(
                new ABIValue("uint256", seed),
                new ABIValue("uint256", currentTurn),
                new ABIValue("uint256", battleRng)
             ).ToBigIntegerFromRLPDecoded());
    }

    // Reset player's hand infos to base stats
    public void ResetCurrentPepemonStats()
    {
        CurrentPepemonStats.spd = PlayerPepemon.Speed;
        CurrentPepemonStats.inte = PlayerPepemon.Intelligence;
        CurrentPepemonStats.atk = PlayerPepemon.Attack;
        CurrentPepemonStats.def = PlayerPepemon.Defense;
        CurrentPepemonStats.sAtk = PlayerPepemon.SAttack;
        CurrentPepemonStats.sDef = PlayerPepemon.SDeffense;
    }

    public void DrawNewHand()
    {
        // Clear previous hand
        CurrentHand.ClearHand();

        // Draw cards to hand
        // Note: Same logic of the contract PepemonBattle.sol
        for (int i = 0; i < CurrentPepemonStats.inte; i++)
        {
            CurrentHand.AddCardToHand(CurrentDeck.GetDeck()[(i + PlayedCardCount) % CurrentDeck.GetDeck().Count]);
        }

        PlayedCardCount += CurrentPepemonStats.inte;
    }


    // editor only func
    public void Reset()
    {
        CurrentHand.ClearHand();
        CurrentDeck.ClearDeck();
        CurrentHP = 0;
    }


#if UNITY_EDITOR
    [Button("Add All Cards")]
    public void AddAllCardsToDeck()
    {
        PlayerDeck.ClearDeck();
        // All all cards to deck 4x
        for (int i = 0; i < 5; i++)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Card).Name);  //FindAssets uses tags check documentation for more info
            Card[] a = new Card[guids.Length];
            for (int j = 0; j < guids.Length; j++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[j]);
                a[j] = UnityEditor.AssetDatabase.LoadAssetAtPath<Card>(path);
                PlayerDeck.GetDeck().Add(a[j]);
            }
        }
    }
#endif

    // Note: Same logic of the contract PepemonBattle.sol
    // This method calculates the battle card's stats after taking into consideration all the support cards currently being played
    public void CalcSupportCardsOnTable(Player opponent)
    {
        var toBeRemoved = new List<Card>();

        //Loop through every support card currently played
        foreach (var card in CurrentHand.GetCardsInHand)
        {
            //Get the effect of that support card
            var effect = card.effectMany;

            //If there is at least 1 turn left
            if (effect.numTurns >= 1)
            {
                var pepemonToBeAffected = opponent.CurrentPepemonStats;

                //If the effect is for me
                if (effect.effectFor == EffectFor.Me)
                {
                    pepemonToBeAffected = CurrentPepemonStats;
                }

                // Change card's stats using that support card
                // Currently effectTo of EffectMany can be ATTACK, DEFENSE, SPEED and INTELLIGENCE
                // Get the statistic changed and update it 
                // Intelligence can't go into the negatives
                if (effect.effectTo == EffectTo.Attack)
                {
                    pepemonToBeAffected.atk += effect.power;
                }
                else if (effect.effectTo == EffectTo.Defense)
                {
                    pepemonToBeAffected.def += effect.power;
                }
                else if (effect.effectTo == EffectTo.Speed)
                {
                    pepemonToBeAffected.spd += effect.power;
                }
                else if (effect.effectTo == EffectTo.Intelligence)
                {
                    pepemonToBeAffected.inte = Math.Max(pepemonToBeAffected.inte + effect.power, 0);
                }

                // Decrease effect numTurns by 1 since 1 turn has already passed
                effect.numTurns--;

                // Delete this one from tableSupportCardStat if all turns of the card have been exhausted
                if (effect.numTurns == 0)
                {
                    toBeRemoved.Add(card);
                }
            }
        }

        foreach (var card in toBeRemoved)
        {
            CurrentHand.RemoveCardFromHand(card);
        }
    }
}
