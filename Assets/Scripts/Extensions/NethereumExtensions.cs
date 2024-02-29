using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System.Collections.Generic;
using System.Threading;
using System;

public static class NethereumExtensions
{
    private const float TX_POLLING_INTERVAL_S = 0.25f;
    private const int EVENT_POLLING_INTERVAL_MS = 2000;

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
    }

    public static async Task<List<EventLog<_IEventDTO>>> GetEventsAsync<_IEventDTO>(this NewFilterInput eventFilter)
        where _IEventDTO : IEventDTO, new()
    {
        var getLogsRequest = new EthGetLogsUnityRequest(Web3Controller.instance.GetUnityRpcRequestClientFactory());
        Debug.Log($"Getting events: {typeof(_IEventDTO).Name} " +
            $"with {eventFilter.Topics.Length} filter params " +
            $"from blocks {eventFilter.FromBlock.BlockNumber} - {eventFilter.ToBlock.BlockNumber}");

        List<EventLog<_IEventDTO>> eventLogs;
        await getLogsRequest.SendRequest(eventFilter);
        eventLogs = getLogsRequest.Result.DecodeAllEvents<_IEventDTO>();

        Debug.Log("Events received: " + eventLogs.Count);
        return eventLogs;
    }

    public static async Task<List<EventLog<_IEventDTO>>> WaitForEventAsync<_IEventDTO>(
        this NewFilterInput eventFilter,
        int millisecondsToWait = EVENT_POLLING_INTERVAL_MS,
        CancellationToken token = default
    )
    where _IEventDTO : IEventDTO, new()
    {
        EthGetLogsUnityRequest getLogsRequest;

        Debug.Log($"Waiting for event: {typeof(_IEventDTO).Name} " +
            $"with {eventFilter.Topics.Length} topics " +
            $"from block {eventFilter.FromBlock.BlockNumber}");

        List<EventLog<_IEventDTO>> eventLogs;
        do
        {
            getLogsRequest = new EthGetLogsUnityRequest(Web3Controller.instance.GetUnityRpcRequestClientFactory());
            await getLogsRequest.SendRequest(eventFilter);
            eventLogs = getLogsRequest.Result.DecodeAllEvents<_IEventDTO>();
            
            if (eventLogs.Count == 0)
            {
                try
                {
                    await UniTask.Delay(millisecondsToWait, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Stopped waiting event " + typeof(_IEventDTO).Name);
                    break;
                }
            }
        }
        while (eventLogs.Count == 0);

        Debug.Log("Events received: " + eventLogs.Count);

        return eventLogs;
    }
}
