using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;

namespace Web3Dots.RPC.Utils.Interfaces
{
    public interface IEthereumService
    {
        string GetAddressW3A(string privateKey);

        Task<string> CreateAndSignTransactionAsync(TransactionInput txInput);

        Task<string> SendTransactionAsync(string signedTransactionData);

        Task<string> TransferEther(string to, decimal amount);
    }
}