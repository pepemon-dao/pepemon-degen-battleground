using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Contracts.Services;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.Blocks;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RPCWeb3Provider : IWeb3Provider
{
    public int ChainId { get; set; }
    public string Network { get; set; }
    public string RpcUrl { get; set; }
    public string PrivateKey { get; set; }
    public string Account => Web3PrivateKey.Address(PrivateKey);

    public async Task<T> CallContract<T>(Web3CallContractArgs args)
    {
        RpcClient rpcClient = new RpcClient(new Uri(RpcUrl));
        EthApiContractService contractService = new EthApiContractService(rpcClient);
        Contract contract = new Contract(contractService, args.abi, args.contract);
        Function function = contract.GetFunction(args.method);
        return await function.CallAsync<T>(args.parameters);
    }

    public Task<string> ConnectToWallet(Web3ConnectArgs args)
    {
        if (args.chainId != ChainId)
        {
            throw new ArgumentException("Requested chain ID does not match RPC settings: Expected " + ChainId + " but got " + args.chainId);
        }
        return Task.FromResult(Account);
    }

    public async Task<int> GetLatestBlockNumber()
    {
        RpcClient rpcClient = new RpcClient(new Uri(RpcUrl));
        HexBigInteger latest = await new EthBlockNumber(rpcClient).SendRequestAsync();
        return (int)latest.Value;
    }

    public async Task<List<Web3Event>> GetPastEvents(Web3GetPastEventsArgs args)
    {
        RpcClient rpcClient = new RpcClient(new Uri(RpcUrl));
        EthApiContractService contractService = new EthApiContractService(rpcClient);
        Contract contract = new Contract(contractService, args.abi, args.contract);
        Event ev = contract.GetEvent(args.eventName);

        object[][] topicFilters = new object[3][] { null, null, null, };
        foreach (var filter in args.filters)
        {
            int indexedParameterIndex = 0;
            for (int i = 0; i < ev.EventABI.InputParameters.Length; i++)
            {
                var abiParam = ev.EventABI.InputParameters[i];
                if (abiParam.Indexed)
                {
                    if (abiParam.Name == filter.param)
                    {
                        if (topicFilters[indexedParameterIndex] == null)
                        {
                            topicFilters[indexedParameterIndex] = new object[] { };
                        }
                        topicFilters[indexedParameterIndex] = topicFilters[indexedParameterIndex].Append(filter.value).ToArray();
                    }
                    indexedParameterIndex++;
                }
            }
        }

        NewFilterInput filterInput = ev.CreateFilterInput(
            topicFilters[0],
            topicFilters[1],
            topicFilters[2],
            new BlockParameter(Convert.ToUInt64(args.fromBlock)),
            new BlockParameter(Convert.ToUInt64(args.toBlock)));
        List<EventLog<List<ParameterOutput>>> eventLogs = await ev.GetAllChangesDefaultAsync(filterInput);

        return eventLogs.Select(e => new Web3Event
        {
            address = e.Log.Address,
            transactionHash = e.Log.TransactionHash,
            blockNumber = ((int)e.Log.BlockNumber.Value),
            topics = e.Log.Topics.Select(o => o.ToString()).ToList(),
            parameters = e.Event.Select(p => new Web3EventParameter
            {
                param = p.Parameter.Name,
                value = p.Result.ToString(),
            }).ToList()
        }).ToList();
    }

    public async Task<string> SendContract(Web3SendContractArgs args)
    {
        RpcClient rpcClient = new RpcClient(new Uri(RpcUrl));
        EthApiContractService contractService = new EthApiContractService(rpcClient);
        Contract contract = new Contract(contractService, args.abi, args.contract);
        Function function = contract.GetFunction(args.method);

        string txHash = await function.SendTransactionAsync(
            Account,
            new HexBigInteger(Convert.ToInt32(args.gasLimit)),
            new HexBigInteger(Convert.ToInt32(args.gasPrice)),
            new HexBigInteger(Convert.ToInt32(args.value)),
            functionInput: args.parameters);
        return txHash;
    }

    public async Task<string> SendTransaction(Web3SendTransactionArgs args)
    {
        string data = "";
        string tx = await EVM.CreateTransaction(ChainId.ToString(), Network, Account, args.to, args.value, data, args.gasPrice, args.gasLimit, RpcUrl);
        string signature = Web3PrivateKey.SignTransaction(PrivateKey, tx, ChainId.ToString());
        string txHash = await EVM.BroadcastTransaction(ChainId.ToString(), Network, Account, args.to, args.value, data, signature, args.gasPrice, args.gasLimit, RpcUrl);
        return txHash;
    }

    public Task<string> SignMessage(Web3SignMessageArgs args)
    {
        return Task.FromResult(Web3PrivateKey.Sign(PrivateKey, args.message));
    }
}
