using System.Collections;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Web3Dots.RPC.Utils.Interfaces;

// creates a transaction for the wallet
namespace Web3Dots.RPC.Utils.Services
{
    public class SignatureService : IMessageSigner
    {
        /// <summary>
        /// Signs a transaction using the provided private key and transaction data.
        /// </summary>
        /// <param name="privateKey">The private key used for signing the transaction.</param>
        /// <param name="transaction">The transaction data to sign.</param>
        /// <returns>The signature string of the signed transaction.</returns>
        public string SignTransaction(string privateKey, string transaction, int _chainId)
        {
	        int chainId = _chainId;
            var ethEcKey = new EthECKey(privateKey);
            var byteArray = transaction.HexToByteArray();
            var chainIdBigInt = _chainId;

            return ShouldUseYParityV(chainId)
                ? EthECDSASignature.CreateStringSignature(ethEcKey.SignAndCalculateYParityV(byteArray))
                : EthECDSASignature.CreateStringSignature(ethEcKey.SignAndCalculateV(byteArray, chainIdBigInt));
        }

        /// <summary>
        /// Determines whether to use the YParityV signature calculation based on the provided chain ID.
        /// </summary>
        /// <param name="chainId">The chain ID to check.</param>
        /// <returns>True if YParityV signature calculation should be used; otherwise, false.</returns>
        private bool ShouldUseYParityV(int chainId)
        {
            int[] validChainIds = { 137, 80001, 1666600000, 1666700000, 25, 338, 250, 4002, 43114, 43113 };
            return ((IList)validChainIds).Contains(chainId);
        }

        /// <summary>
        /// Signs a message using the provided private key.
        /// </summary>
        /// <param name="privateKey">The private key used for signing the message.</param>
        /// <param name="message">The message to sign.</param>
        /// <returns>The signature string of the signed message.</returns>
        public string SignMessage(string privateKey, string message) => new EthereumMessageSigner().HashAndSign(message, privateKey);
    }
}