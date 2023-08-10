﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RLP;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using Web3Dots.RPC.Bytes;
using Web3Dots.RPC.Providers;
using Web3Dots.RPC.Signers;
using Web3Dots.RPC.Utils.Services;
using Contract = Web3Dots.RPC.Contracts.Contract;

namespace Web3Dots
{
    public class Program : BaseSigner
    {
         private readonly Key _signingKey;
         private static string PrivateKey = "ADD_PRIVATE_KEY";
         private const string MintingContractAddress = "0xaf0E6743bc86ebb35eBE9531d82FB326D466c4b5";
         private const string MintingNftContractAddress = "0x7D0FAa703CD188a630b516a69Ceb2c87D9896DdA";
         private const string AutographMinterContract = "0x6b8D486fD16f94811bC41f5129f1Ec076A76D385";
         private const string MintingContractAbi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"approved\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"getApproved\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"ownerOf\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"}],\"name\":\"safeMint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"tokenURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
         private const string ProviderUrl = "ADD_NODE_URL";
         private const string AutoGraphMinter = "[{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_core\",\"type\":\"address\"},{\"internalType\":\"address[]\",\"name\":\"_nftContracts\",\"type\":\"address[]\"},{\"internalType\":\"uint128\",\"name\":\"_replenishRatePerSecond\",\"type\":\"uint128\"},{\"internalType\":\"uint128\",\"name\":\"_bufferCap\",\"type\":\"uint128\"},{\"internalType\":\"address\",\"name\":\"_paymentRecipient\",\"type\":\"address\"},{\"internalType\":\"uint8\",\"name\":\"_expiryTokenHoursValid\",\"type\":\"uint8\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"oldBufferCap\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newBufferCap\",\"type\":\"uint256\"}],\"name\":\"BufferCapUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amountReplenished\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"bufferRemaining\",\"type\":\"uint256\"}],\"name\":\"BufferReplenished\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amountUsed\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"bufferRemaining\",\"type\":\"uint256\"}],\"name\":\"BufferUsed\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"oldCore\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newCore\",\"type\":\"address\"}],\"name\":\"CoreUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"tokenIds\",\"type\":\"uint256[]\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"units\",\"type\":\"uint256[]\"}],\"name\":\"ERC1155BatchMinted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"}],\"name\":\"ERC1155Minted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Paused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"paymentRecipient\",\"type\":\"address\"}],\"name\":\"PaymentRecipientUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"oldReplenishRatePerSecond\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newRateReplenishPerSecond\",\"type\":\"uint256\"}],\"name\":\"ReplenishRatePerSecondUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Unpaused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"WhitelistAddressAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"WhitelistAddressRemoved\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"}],\"name\":\"WhitelistedContractAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"}],\"name\":\"WhitelistedContractRemoved\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContractAddress\",\"type\":\"address\"}],\"name\":\"addWhitelistedContract\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"whitelistAddresses\",\"type\":\"address[]\"}],\"name\":\"addWhitelistedContracts\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"buffer\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"bufferCap\",\"outputs\":[{\"internalType\":\"uint128\",\"name\":\"\",\"type\":\"uint128\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"bufferRemaining\",\"outputs\":[{\"internalType\":\"uint224\",\"name\":\"\",\"type\":\"uint224\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"core\",\"outputs\":[{\"internalType\":\"contract Core\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"target\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"}],\"internalType\":\"struct CoreRef.Call[]\",\"name\":\"calls\",\"type\":\"tuple[]\"}],\"name\":\"emergencyAction\",\"outputs\":[{\"internalType\":\"bytes[]\",\"name\":\"returnData\",\"type\":\"bytes[]\"}],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"}],\"name\":\"expiredHashes\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"used\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"expiryTokenHoursValid\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.HashInputsParams\",\"name\":\"input\",\"type\":\"tuple\"}],\"name\":\"getHash\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getWhitelistedAddresses\",\"outputs\":[{\"internalType\":\"address[]\",\"name\":\"\",\"type\":\"address[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"isWhitelistedAddress\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"lastBufferUsedTime\",\"outputs\":[{\"internalType\":\"uint32\",\"name\":\"\",\"type\":\"uint32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchForFree\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchWithEthAsFee\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchWithPaymentTokenAsFee\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"name\":\"mintForFree\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintWithEthAsFeeParams\",\"name\":\"params\",\"type\":\"tuple\"}],\"name\":\"mintWithEthAsFee\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintWithPaymentTokenAsFeeParams\",\"name\":\"params\",\"type\":\"tuple\"}],\"name\":\"mintWithPaymentTokenAsFee\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"pause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paused\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paymentRecipient\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"name\":\"recoverSigner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContractAddress\",\"type\":\"address\"}],\"name\":\"removeWhitelistedContract\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"whitelistAddresses\",\"type\":\"address[]\"}],\"name\":\"removeWhitelistedContracts\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"replenishRatePerSecond\",\"outputs\":[{\"internalType\":\"uint128\",\"name\":\"\",\"type\":\"uint128\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint128\",\"name\":\"newBufferCap\",\"type\":\"uint128\"}],\"name\":\"setBufferCap\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newCore\",\"type\":\"address\"}],\"name\":\"setCore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint128\",\"name\":\"newRateLimitPerSecond\",\"type\":\"uint128\"}],\"name\":\"setReplenishRatePerSecond\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unpause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint8\",\"name\":\"_expiryTokenHoursValid\",\"type\":\"uint8\"}],\"name\":\"updateExpiryTokenHoursValid\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_paymentRecipient\",\"type\":\"address\"}],\"name\":\"updatePaymentRecipient\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        public static async Task Main(string[] args)
        {
            //await GetRpcData();
            //await TransferEther();
            //await Mint();
            //await Mint2();
            await MintAutoGraph();
            //CreateEthWallet();
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
        
        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - origin;
            return (long)diff.TotalSeconds;
        }
        
        // standard way of minting 
        public static async Task MintAutoGraph()
        {
            // smart contract method to call
            string method = "mintForFree";
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(421613));

            try
            {
                var contract = new Contract(AutoGraphMinter, AutographMinterContract,ethereumService.GetProvider());
                long expiryToken = ConvertToUnixTimestamp(DateTime.UtcNow) - 1000;
                var calldata = contract.Calldata(method, new object[]
                {
                    ethereumService.GetAddress(PrivateKey),
                    0,
                    1,
                    getSalt(),
                    MintingNftContractAddress,
                    expiryToken.ToBytesForRLPEncoding()
                });

                TransactionInput txInput = new TransactionInput
                {
                    To = AutographMinterContract,
                    From = ethereumService.GetAddress(PrivateKey),
                    Value = new HexBigInteger(0),
                    Data = calldata,
                    GasPrice = ethereumService._provider.GetGasPrice().Result,
                    Gas = new HexBigInteger(75000),
                };

                var txHash = await ethereumService.SignAndSendTransactionAsync(txInput);
                Console.WriteLine($"Transaction Hash: {txHash}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static byte[] getSalt()
        {
            using (var random = new RNGCryptoServiceProvider())
            {
                // Define the length of the salt array (32 bytes for bytes32)
                const int length = 32;

                // Initialize the salt array
                byte[] salt = new byte[length];

                // Generate random bytes
                random.GetNonZeroBytes(salt);

                // Return the salt
                return salt;
            }
        }

        
        
        // standard way of minting 
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
                    Value = new HexBigInteger(0),
                    Data = calldata,
                    GasPrice = new HexBigInteger(100000),
                    Gas = new HexBigInteger(100000),
                };

                var txHash = await ethereumService.SignAndSendTransactionAsync(txInput);
                Console.WriteLine($"Transaction Hash: {txHash}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public static async Task Mint2()
        {
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(5));
            var safeMint = new SafeMintFunction()
            {
                To = "0x525b19d1cA89c3620b4A12B7D36970E410c8C5f5",
            };
            safeMint.To = MintingNftContractAddress;
            safeMint.FromAddress = ethereumService.GetAddress(PrivateKey);
            safeMint.Gas = new HexBigInteger(100000);
            safeMint.GasPrice = new HexBigInteger(100000);
            var contractHandler = ethereumService._web3.Eth.GetContractHandler(MintingContractAddress);
            var txHash = await contractHandler.SendRequestAsync(safeMint);
            Console.WriteLine("Transaction Hash: " + txHash);
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
            BigInteger privateKey = new BigInteger(1, privateKeyBytes);

            // Check if the private key is within the range of valid private keys for the secp256k1 curve
            BigInteger n = new BigInteger("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", 16);
            return privateKey.CompareTo(BigInteger.One) >= 0 && privateKey.CompareTo(n) < 0;
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
