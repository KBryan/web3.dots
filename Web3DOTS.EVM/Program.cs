﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Web3Dots.RPC.Contracts;
using Web3Dots.RPC.Providers;
using Web3Dots.RPC.Signers;
using Web3Dots.RPC.Utils.Services;

namespace Web3Dots
{
    public class Program : BaseSigner
    {
         private readonly Key _signingKey;
         private static string PrivateKey = "ADD_PRIVATE_KEY";
         private const string MintingContractAddress = "0xaf0E6743bc86ebb35eBE9531d82FB326D466c4b5";
         private const string MintingNftContractAddress = "0xAAB00c2BED1016c39aa03860A2321Eb42803CC21";
         private const string MintingContractAbi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"approved\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"getApproved\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"ownerOf\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"}],\"name\":\"safeMint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"tokenURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
         private static string TokenContractAbi =
                "[{\"inputs\":[{\"internalType\":\"string\",\"name\":\"name_\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"symbol_\",\"type\":\"string\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"subtractedValue\",\"type\":\"uint256\"}],\"name\":\"decreaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"addedValue\",\"type\":\"uint256\"}],\"name\":\"increaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
         private const string ProviderUrl = "ADD_PROVIDER_URL";
        public static async Task Main(string[] args)
        {
            await GetRpcData();
            //await TransferEther();
            // await Mint();
            CreateEthWallet();
        }
        
        public BaseProvider BaseProvider { get; }

        private static async Task GetRpcData()
        {
            var ethereumService = new EthereumService( ProviderUrl);
            var accountBalance = await ethereumService._provider.GetBalance("0x525b19d1cA89c3620b4A12B7D36970E410c8C5f5");
            var blockNumber = await ethereumService._provider.GetBlockNumber();
            var getBlock = await ethereumService._provider.GetBlock();
            var network = await ethereumService._provider.GetNetwork();
            Console.WriteLine($"Network name: {network.Name}");
            Console.WriteLine($"Network chain id: {network.ChainId}");
            Console.WriteLine("Account Balance: " + accountBalance);
            Console.WriteLine("Block Number: " + blockNumber);
            Console.WriteLine("Block Info: " + JsonConvert.SerializeObject(getBlock, Formatting.Indented));
        }

        private static async Task TransferEther()
        {
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl,new HexBigInteger(5));
            var txHash = await ethereumService.TransferEther("0x525b19d1cA89c3620b4A12B7D36970E410c8C5f5", 0.000001m);
            Console.WriteLine($"Hash: {txHash}");
        }
        
        public static async Task Mint()
        {
            // smart contract method to call
            string method = "safeMint";
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(5));
            // connects to user's wallet to send a transaction
            try
            {
                var contract = new Contract(MintingContractAbi, MintingContractAddress,ethereumService.GetProvider());
                Console.WriteLine("Account: " + ethereumService.GetAddress(PrivateKey));
                var calldata = contract.Calldata(method, new object[]
                {
                    ethereumService.GetAddress(PrivateKey),
                });

                TransactionInput txInput = new TransactionInput
                {
                    To = MintingNftContractAddress,
                    From = ethereumService.GetAddress(PrivateKey),
                    Value = new HexBigInteger(0), // Convert the Ether amount to Wei
                    Data = calldata,
                    GasPrice = new HexBigInteger(100000),
                    Gas = new HexBigInteger(100000),
                };

                var txHash = await ethereumService.CreateSignAndSendTransactionAsync(txInput);
                Console.WriteLine($"Transaction Hash: {txHash}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        

        private static void CreateEthWallet()
        {
            // Generate a random seed using the RNGCryptoServiceProvider
            byte[] randomBytes = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            // Convert the random bytes to a mnemonic seed phrase
            string seedPhrase = new Mnemonic(Wordlist.English, randomBytes).ToString();

            Console.WriteLine("Seed phrase: " + seedPhrase);

            // Convert the seed phrase to a 64-byte seed using PBKDF2
            byte[] salt = Encoding.UTF8.GetBytes("mnemonic");
            using (var pbkdf2 = new Rfc2898DeriveBytes(seedPhrase, salt, 2048))
            {
                byte[] seed = pbkdf2.GetBytes(32);

                // Keep generating a private key until a valid one is obtained
                EthECKey privateKey;
                do
                {
                    privateKey = new EthECKey(seed, true);
                    seed = Increment(seed);
                } while (!IsValidPrivateKey(privateKey.GetPrivateKeyAsBytes()));

                // Print the private key
                Console.WriteLine("Private key: " + privateKey.GetPrivateKeyAsBytes().ToHex());

                // Generate the Ethereum public address from the private key
                string address = privateKey.GetPublicAddress().ToLower();
                
                // Print the Ethereum public address
                Console.WriteLine("Public address: " + address);
            }
        }
        
        public override Task<string> SignMessage(byte[] message)
        {
            var hash = new Sha3Keccack().CalculateHash(message);
            return Task.FromResult(_signingKey.Sign(new uint256(hash)).ToCompact().ToHex());
        }

        public override Task<string> SignMessage(string message)
        {
            var hash = new Sha3Keccack().CalculateHash(message);
            return Task.FromResult(_signingKey.Sign(new uint256(hash)).ToCompact().ToHex());
        }
        
        static bool IsValidPrivateKey(byte[] privateKeyBytes)
        {
            // Convert the private key bytes to a BigInteger
            // BigInteger privateKey = new HexBigInteger(1, privateKeyBytes);

            // Check if the private key is within the range of valid private keys for the secp256k1 curve
            // BigInteger n = new BigInteger("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", 16);
            return true; //privateKey.CompareTo(BigInteger.One) >= 0 && privateKey.CompareTo(n) < 0;
        }

        static byte[] Increment(byte[] bytes)
        {
            // Increment the least significant byte by 1
            byte[] result = new byte[bytes.Length];
            Array.Copy(bytes, result, bytes.Length);
            for (int i = result.Length - 1; i >= 0; i--)
            {
                if (result[i] < 255)
                {
                    result[i]++;
                    break;
                }
                else
                {
                    result[i] = 0;
                }
            }
            return result;
        }
        public Program(IProvider provider, BaseProvider baseProvider, Key signingKey) : base(provider)
        {
            BaseProvider = baseProvider;
            _signingKey = signingKey;
        }
    }
}
