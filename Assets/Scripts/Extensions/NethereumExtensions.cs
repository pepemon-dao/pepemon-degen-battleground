using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public static class NethereumExtensions
{
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
}
