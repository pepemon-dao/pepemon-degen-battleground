using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;
using Nethereum.Contracts.QueryHandlers.MultiCall;

public static class NethereumExtensions
{
    private const float TX_POLLING_INTERVAL_S = 0.25f;

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
}
