using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

class WebGLWeb3Provider : MonoBehaviour, IWeb3Provider
{
    private const int TimeoutDelayMs = 5 * 60 * 1000;

    public int ChainId { private set; get; } = -1;
    public string Account { private set; get; } = "";
    public bool IsConnected => ChainId != -1 && !string.IsNullOrWhiteSpace(Account);

    private int nextRequestId = 1;
    private Dictionary<int, TaskCompletionSource<string>> pendingOperations = new Dictionary<int, TaskCompletionSource<string>>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        JSLib.SetEventListenerGameObject(gameObject.name);
    }

    public async Task<string> ConnectToWallet(Web3ConnectArgs args)
    {
        return await SendRequest(
            new Web3Request<Web3ConnectArgs>
            {
                requestId = nextRequestId++,
                action = "Connect",
                args = args
            },
            TimeoutDelayMs
        );
    }

    public async Task<T> CallContract<T>(Web3CallContractArgs args)
    {
        return JsonConvert.DeserializeObject<T>(await SendRequest(
            new Web3Request<Web3CallContractArgs>
            {
                requestId = nextRequestId++,
                action = "CallContract",
                args = args
            },
            TimeoutDelayMs
        ));
    }

    public async Task<string> SendContract(Web3SendContractArgs args)
    {
        return await SendRequest(
            new Web3Request<Web3SendContractArgs>
            {
                requestId = nextRequestId++,
                action = "SendContract",
                args = args
            },
            TimeoutDelayMs
        );
    }

    public async Task<string> SendTransaction(Web3SendTransactionArgs args)
    {
        return await SendRequest(
            new Web3Request<Web3SendTransactionArgs>
            {
                requestId = nextRequestId++,
                action = "SendTransaction",
                args = args
            },
            TimeoutDelayMs
        );
    }

    public async Task<string> SignMessage(Web3SignMessageArgs args)
    {
        return await SendRequest(
            new Web3Request<Web3SignMessageArgs>
            {
                requestId = nextRequestId++,
                action = "SignMessage",
                args = args
            },
            TimeoutDelayMs
        );
    }

    public async Task<List<Web3Event>> GetPastEvents(Web3GetPastEventsArgs args)
    {
        return JsonConvert.DeserializeObject<List<Web3Event>>(await SendRequest(
            new Web3Request<Web3GetPastEventsArgs>
            {
                requestId = nextRequestId++,
                action = "GetPastEvents",
                args = args
            },
            TimeoutDelayMs
        ));
    }

    public async Task<int> GetLatestBlockNumber()
    {
        return Convert.ToInt32(await SendRequest(
            new Web3Request<string>
            {
                requestId = nextRequestId++,
                action = "GetLatestBlockNumber"
            },
            TimeoutDelayMs
        ));
    }

    public void OnConnect(int chainId)
    {
        Debug.Log($"Provider event: OnConnect({chainId})");
    }

    public void OnDisconnect(string errorMessage)
    {
        Debug.Log($"Provider event: OnDisconnect(\"{errorMessage}\")");
    }

    public void OnAccountChanged(string account)
    {
        Debug.Log($"Provider event: OnAccountChanged(\"{account}\")");
    }

    public void OnChainChanged(int chainId)
    {
        Debug.Log($"Provider event: OnChainChanged({chainId})");
    }

    public void OnResponse(string json)
    {
        Web3Response res = JsonConvert.DeserializeObject<Web3Response>(json);
        if (pendingOperations.TryGetValue(res.requestId, out TaskCompletionSource<string> tcs))
        {
            tcs.SetResult(res.value);
        }
        else
        {
            throw new InvalidOperationException("Received response for non-existent operation: " + res.value);
        }
    }

    public void OnError(string json)
    {
        Web3Error res = JsonConvert.DeserializeObject<Web3Error>(json);
        if (pendingOperations.TryGetValue(res.requestId, out TaskCompletionSource<string> tcs))
        {
            tcs.SetException(new Web3Exception(res.message, res.code));
        }
        else
        {
            throw new InvalidOperationException("Received error message for non-existent operation: " + res.message);
        }
    }

    private async Task<string> SendRequest<T>(Web3Request<T> request, int timeoutMs)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        pendingOperations.Add(request.requestId, tcs);
        try
        {
            JSLib.SendRequest(JsonConvert.SerializeObject(request));
            if (await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task)
            {
                return await tcs.Task;
            }
            else
            {
                throw new TimeoutException("Web3 operation timed out");
            }
        }
        finally
        {
            pendingOperations.Remove(request.requestId);
        }
    }

    private static class JSLib
    {
        [DllImport("__Internal")]
        internal static extern void SetEventListenerGameObject(string gameObjectName);

        [DllImport("__Internal")]
        internal static extern void SendRequest(string requestJson);
    }
}
