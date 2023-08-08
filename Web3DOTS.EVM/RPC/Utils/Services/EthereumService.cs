using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Web3Dots.RPC.Providers;
using Web3Dots.RPC.Utils.Interfaces;

namespace Web3Dots.RPC.Utils.Services
{
    /// <summary>
    /// Provides Ethereum-related functionality.
    /// </summary>
    public class EthereumService : IEthereumService
    {
        public readonly Nethereum.Web3.Web3 _web3;
        private Account _account;
        public JsonRpcProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthereumService"/> class.
        /// </summary>
        /// <param name="privateKey">The private key associated with the Ethereum account.</param>
        /// <param name="url">The URL of the Ethereum node.</param>
        /// <param name="chainId"></param>
        public EthereumService(string privateKey, string url, HexBigInteger chainId)
        {
            _account = new Account(privateKey,chainId);
            _provider= new JsonRpcProvider(url);
            _web3 = new Nethereum.Web3.Web3(_account, url);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EthereumService"/> class.
        /// </summary>
        /// <param name="url">The URL of the Ethereum node.</param>
        public EthereumService(string url)
        {
            _provider= new JsonRpcProvider(url);
        }

        /// <summary>
        /// Creates and signs a transaction asynchronously.
        /// </summary>
        /// <param name="txInput">The transaction input parameters.</param>
        /// <returns>The signed transaction data as a string.</returns>
        public async Task<string> SignAndSendTransactionAsync(TransactionInput txInput)
        {
            var signedTransaction = await _web3.Eth.TransactionManager.SignTransactionAsync(txInput);
            var txHash = await SendTransactionAsync(signedTransaction);
            return txHash;
        }

        /// <summary>
        /// Sends a signed transaction asynchronously.
        /// </summary>
        /// <param name="signedTransactionData">The signed transaction data.</param>
        /// <returns>The transaction hash as a string.</returns>
        public async Task<string> SendTransactionAsync(string signedTransactionData)
        {
            var result = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransactionData);
            return result;
        }

        public async Task<string> TransferEther(string to, decimal amount)
        {
            
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(_account.Address);
            
            var tx = await _web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(to, amount,2, 75000,75000);
            return tx.TransactionHash;
        }

        /// <summary>
        /// Gets the Ethereum address associated with the provided private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <returns>The Ethereum address as a string.</returns>
        public string GetAddress(string privateKey) => new EthECKey(privateKey).GetPublicAddress();

        public JsonRpcProvider GetProvider()
        {
            return _provider;
        }
    }
}
