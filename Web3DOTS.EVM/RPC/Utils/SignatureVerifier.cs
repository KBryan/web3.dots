using System.Diagnostics;
using System.Text;
using Nethereum.Signer;
using Nethereum.Util;

namespace Web3Dots.RPC.Utils
{
    public class SignatureVerifier
    {
        public string VerifySignature(string signatureString, string originalMessage)
        {
            string msg = "\x19" + "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;
            byte[] msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            EthECDSASignature signature = MessageSigner.ExtractEcdsaSignature(signatureString);
            EthECKey key = EthECKey.RecoverFromSignature(signature, msgHash);

            bool isValid = key.Verify(msgHash, signature);

            // return signed tx response from wallet
            if (isValid)
            {
                return key.GetPublicAddress();
            }
            return "Address not equal to signer...";
        }
    }
}