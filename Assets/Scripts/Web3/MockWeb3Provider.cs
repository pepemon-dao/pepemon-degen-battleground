using System.Collections.Generic;
using System.Threading.Tasks;

class MockWeb3Provider : IWeb3Provider
{
    public Task<T> CallContract<T>(Web3CallContractArgs args)
    {
        return Task.FromResult<T>(default);
    }

    public Task<string> ConnectToWallet(Web3ConnectArgs args)
    {
        return Task.FromResult("0x0000000000000000000000000000000000000000");
    }

    public Task<int> GetLatestBlockNumber()
    {
        return Task.FromResult(0);
    }

    public Task<List<Web3Event>> GetPastEvents(Web3GetPastEventsArgs args)
    {
        return Task.FromResult(new List<Web3Event> { });
    }

    public Task<string> SendContract(Web3SendContractArgs args)
    {
        return Task.FromResult("0x0000000000000000000000000000000000000000000000000000000000000000");
    }

    public Task<string> SendTransaction(Web3SendTransactionArgs args)
    {
        return Task.FromResult("0x0000000000000000000000000000000000000000000000000000000000000000");
    }

    public Task<string> SignMessage(Web3SignMessageArgs args)
    {
        return Task.FromResult("0x0000000000000000000000000000000000000000000000000000000000000000");
    }
}
