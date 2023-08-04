using System.Threading.Tasks;
using Web3Dots.RPC.Providers;
using Web3Dots.RPC.Transactions;

namespace Web3Dots.RPC.Signers
{
    public interface ISigner
    {
        public Task<string> GetAddress();
        public Task<string> SignMessage(byte[] message);
        public Task<string> SignMessage(string message);
        public Task<string> SignTransaction(TransactionRequest transaction);
        public Task<TransactionResponse> SendTransaction(TransactionRequest transaction);
        public ISigner Connect(IProvider provider);
        public IProvider Provider { get; }
    }
}