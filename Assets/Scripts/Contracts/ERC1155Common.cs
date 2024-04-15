using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
//using Nethereum.Unity.Rpc;

public abstract class ERC1155Common
{
    /// <summary>
    /// Tells whether or not a contract/wallet can transfer NFTs
    /// </summary>
    /// <param name="operatorAddress">Address of the contract/wallet</param>
    /// <returns>result of IsApprovedForAll</returns>
    protected static async Task<bool> GetApproval(string contractAddress, string operatorAddress)
    {
        /*var request = new QueryUnityRequest<IsApprovedForAllFunction, IsApprovedForAllOutputDTO>(
           Web3Controller.instance.GetUnityRpcRequestClientFactory(),
           Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(
            new IsApprovedForAllFunction()
            {
                Account = Web3Controller.instance.SelectedAccountAddress,
                Operator = operatorAddress
            },
            contractAddress);*/
        await Task.Delay(1);
        return true;//response.ReturnValue1;
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <param name="operatorAddress">Contract/wallet which will be given/revoked permission to transfer the NFTs</param>
    /// <returns>Transaction hash</returns>
    protected static async Task SetApproval(string contractAddress, bool approved, string operatorAddress)
    {
        /*var approvalRequest = Web3Controller.instance.GetContractTransactionUnityRequest();
        await approvalRequest.SendTransactionAndWaitForReceiptAsync(
            new SetApprovalForAllFunction()
            {
                Operator = operatorAddress,
                Approved = approved
            },
            contractAddress);*/

        await Task.Delay(1);
    }
}

