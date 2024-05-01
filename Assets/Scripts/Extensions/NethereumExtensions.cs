using Nethereum.ABI.FunctionEncoding.Attributes;
//using Nethereum.Unity.Rpc;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System.Collections.Generic;
using System.Threading;
using System;
using Thirdweb;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

public static class NethereumExtensions
{
    private const float TX_POLLING_INTERVAL_S = 0.25f;
    private const int EVENT_POLLING_INTERVAL_MS = 2000;

    /*
    public static async Task<TResponse> QueryAsync<TFunctionMessage, TResponse>(this QueryUnityRequest<TFunctionMessage, TResponse> request,
                                                                                TFunctionMessage functionMessage,
                                                                                string contractAddress)
         where TFunctionMessage : FunctionMessage, new() where TResponse : IFunctionOutputDTO, new()
    {
        await request.Query(functionMessage, contractAddress);
        if (request.Result != null)
        {
            return request.Result;
        }

        throw request.Exception;
    }

    public static async Task<string> SignAndSendTransactionAsync<TContractFunction>(this IContractTransactionUnityRequest request, TContractFunction function, string contractAdress)
        where TContractFunction : FunctionMessage
    {
        await request.SignAndSendTransaction(function, contractAdress);
        if (request.Result != null)
        {

            return request.Result;
        }
        throw request.Exception;
    }

    public static async Task<TransactionReceipt> SendTransactionAndWaitForReceiptAsync<TContractFunction>(this IContractTransactionUnityRequest request, TContractFunction function, string contractAdress)
        where TContractFunction : FunctionMessage
    {
        await request.SignAndSendTransaction(function, contractAdress);
        if (request.Result == null)
        {
            throw request.Exception;
        }

        IUnityRpcRequestClientFactory rpcFactory = Web3Controller.instance.GetUnityRpcRequestClientFactory();
        TransactionReceiptPollingRequest poller = new(rpcFactory);

        Debug.Log("Waiting for tx receipt: " + request.Result);
        await poller.PollForReceipt(request.Result, TX_POLLING_INTERVAL_S);
        if (poller.Result == null)
        {
            throw poller.Exception;
        }

        Debug.Log("Tx receipt received: " + poller.Result);
        return poller.Result;
    }

    public static async Task<BlockParameter> RequestLatestBlockNumber<T>(this T blockParamInstance)
        where T : BlockParameter, new()
    {
        var getBlockNumberRequest = new EthBlockNumberUnityRequest(
            Web3Controller.instance.GetUnityRpcRequestClientFactory());

        await getBlockNumberRequest.SendRequest();
        if (getBlockNumberRequest.Result == null)
        {
            throw getBlockNumberRequest.Exception;
        }

        var blockNumber = getBlockNumberRequest.Result.Value;
        blockParamInstance.SetValue(blockNumber);
        return blockParamInstance;
    }*/

    public static async Task<List<EventLog<TEventDTO>>> GetEventsAsync<TEventDTO>(
        this NewFilterInput eventFilter, 
        Thirdweb.Contract contract
    )
        where TEventDTO : IEventDTO, new()
    {
        var web3 = Utils.GetWeb3();
        var transferEventHandler = web3.Eth.GetEvent<TEventDTO>(contract.Address);

        Debug.Log($"Getting events: {typeof(TEventDTO).Name} " +
            $"with {eventFilter.Topics.Length} filter params " +
            $"from blocks {eventFilter.FromBlock.BlockNumber} - {eventFilter.ToBlock.BlockNumber}");

        List<EventLog<TEventDTO>> eventLogs = await transferEventHandler.GetAllChangesAsync(eventFilter);

        Debug.Log("Events received: " + eventLogs.Count);
        return eventLogs;
    }

    public static async Task<List<EventLog<TEventDTO>>> WaitForEventAsync<TEventDTO>(
        this NewFilterInput eventFilter,
        Thirdweb.Contract contract,
        int millisecondsToWait = EVENT_POLLING_INTERVAL_MS,
        CancellationToken token = default
    )
    where TEventDTO : IEventDTO, new()
    {
        Debug.Log($"Waiting for event: {typeof(TEventDTO).Name} " +
            $"with {eventFilter.Topics.Length} topics " +
            $"from block {eventFilter.FromBlock.BlockNumber}");

        var web3 = Utils.GetWeb3();
        var transferEventHandler = web3.Eth.GetEvent<TEventDTO>(contract.Address);

        List<EventLog<TEventDTO>> eventLogs;
        do
        {
            eventLogs = await transferEventHandler.GetAllChangesAsync(eventFilter);
            
            if (eventLogs.Count == 0)
            {
                try
                {
                    await UniTask.Delay(millisecondsToWait, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Stopped waiting event " + typeof(TEventDTO).Name);
                    break;
                }
            }
        }
        while (eventLogs.Count == 0);

        Debug.Log("Events received: " + eventLogs.Count);

        return eventLogs;
    }
}
