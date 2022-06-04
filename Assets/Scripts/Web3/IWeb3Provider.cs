using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWeb3Provider
{
    public Task<string> ConnectToWallet(Web3ConnectArgs args);
    public Task<T> CallContract<T>(Web3CallContractArgs args);
    public Task<string> SendContract(Web3SendContractArgs args);
    public Task<string> SendTransaction(Web3SendTransactionArgs args);
    public Task<string> SignMessage(Web3SignMessageArgs args);
    public Task<List<Web3Event>> GetPastEvents(Web3GetPastEventsArgs args);
    public Task<int> GetLatestBlockNumber();
}
