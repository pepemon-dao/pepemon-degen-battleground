using System;
using System.Collections.Generic;

[Serializable]
public struct Web3Request<T>
{
    public int requestId;
    public string action;
    public T args;
}

[Serializable]
public struct Web3Response
{
    public int requestId;
    public string value;
}

[Serializable]
public struct Web3Error
{
    public int requestId;
    public int code;
    public string message;
}

[Serializable]
public struct Web3ConnectArgs
{
    public int chainId;
}

[Serializable]
public struct Web3SignMessageArgs
{
    public string message;
}

[Serializable]
public struct Web3SendTransactionArgs
{
    public string to;
    public string value;
    public string gasLimit;
    public string gasPrice;
}

[Serializable]
public struct Web3SendContractArgs
{
    public string method;
    public string abi;
    public string contract;
    public object[] parameters;
    public string value;
    public string gasLimit;
    public string gasPrice;
}

[Serializable]
public struct Web3CallContractArgs
{
    public string method;
    public string abi;
    public string contract;
    public object[] parameters;
}

[Serializable]
public struct Web3GetPastEventsArgs
{
    public string abi;
    public string contract;
    public string eventName;
    public string fromBlock;
    public string toBlock;
    public List<Web3EventParameter> filters;
}

[Serializable]
public struct Web3EventParameter
{
    public string param;
    public string value;
}

[Serializable]
public struct Web3GetPastEventsResponse
{
    public List<Web3Event> events;
}

[Serializable]
public struct Web3Event
{
    public int blockNumber;
    public string address;
    public string transactionHash;
    public List<string> topics;
    public List<Web3EventParameter> parameters;
}
