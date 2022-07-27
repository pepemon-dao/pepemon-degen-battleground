using System;
using UnityEngine;

class PepemonBattle
{
    private const string AbiPath = "abi/PepemonBattle.abi.json";
    private static readonly string abi = "";

    static PepemonBattle()
    {
        abi = Resources.Load<TextAsset>(AbiPath).text;
    }

    // mapping (uint => uint) public battleIdRNGSeed;
}
