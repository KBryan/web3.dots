using System;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.ABI;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json;
using Web3Dots.RPC.Providers;
using Web3Dots.RPC.Signers;
using Web3Dots.RPC.Utils.Services;
using Contract = Web3Dots.RPC.Contracts.Contract;

namespace Web3Dots
{
    using Nethereum.ABI.FunctionEncoding.Attributes;
    using System.Numerics;


    public class HashInputsParams
    {
        public string Recipient { get; set; }
        public BigInteger TokenId { get; set; }
        public BigInteger Units { get; set; }
        public BigInteger Salt { get; set; }
        public string NftContract { get; set; }
        public string PaymentToken { get; set; }
        public BigInteger PaymentAmount { get; set; }
        public BigInteger ExpiryToken { get; set; }
    }

    public class HashService
    {
        public byte[] GetHash(HashInputsParams input)
        {
            var abiEncoded = ABIEncode(input);
            var hash = new Sha3Keccack().CalculateHash(abiEncoded);

            // This is what is missing off your offchain hash.
            string msg = "\x19" + "Ethereum Signed Message:\n" + hash.Length + hash;

            // This hash is what is needs to be past into contract. Otherwise the prefix is useless. 
            byte[] msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            return msgHash;
        }

        private byte[] ABIEncode(HashInputsParams input)
        {
            // Create an ABI encoder
            var encoder = new ABIEncode();

            // ABI encode the input properties
            return encoder.GetABIEncoded(
                new ABIValue("address", input.Recipient),
                new ABIValue("uint256", input.TokenId),
                new ABIValue("uint256", input.Units),
                new ABIValue("uint256", input.Salt),
                new ABIValue("address", input.NftContract),
                new ABIValue("address", input.PaymentToken),
                new ABIValue("uint256", input.PaymentAmount),
                new ABIValue("uint256", input.ExpiryToken)
            );
        }
    }
    /*
    // [0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4,0,1,100,0x7D0FAa703CD188a630b516a69Ceb2c87D9896DdA,0x0000000000000000000000000000000000000000,0,1691906390]
    * // SPDX-License-Identifier: MIT
    pragma solidity ^0.8.0;

    contract Verify {

        /// @dev - HashInputsParams is a struct that contains the params for generating a hash
        /// @param recipient - Address of the receiver of the NFT
        /// @param tokenId - ID of the NFT
        /// @param units - Amount of NFTs to mint
        /// @param salt - Salt of the message to be signed
        /// @param nftContract - Address of the NFT contract to Mint from
        /// @param paymentToken - Address of the token to be used for payment
        /// @param paymentAmount - Amount of the token to be used for payment
        /// @param expiryToken - Expiry token timestamp. ie create a timestamp.now on creation of the hash
        struct HashInputsParams {
            address recipient;
            uint256 tokenId; // nft token id to be minted
            uint256 units; // units to be minted
            uint256 salt;
            address nftContract; // nft contract address
            address paymentToken; // token to be used for payment if payment is required
            uint256 paymentAmount; // amount of token to be used for payment if payment is required
            uint256 expiryToken;
        }

        function VerifyMessage(bytes32 _hashedMessage, uint8 _v, bytes32 _r, bytes32 _s) public pure returns (address) {
            // bytes memory prefix = "\x19Ethereum Signed Message:\n32"; // This is not needed as its should already be added. 
            // bytes32 prefixedHashMessage = keccak256(abi.encodePacked(prefix, _hashedMessage)); // same
            address signer = ecrecover(_hashedMessage, _v, _r, _s);
            return signer;
        }

         /// @dev - Returns the hash of the message
        /// @param input - hashInputs struct
        
        
        /// This hash is missing the prefix.  
        function getHash(HashInputsParams memory input) public pure returns (bytes32) {
            bytes32 hash = keccak256(
                abi.encode(
                    input.recipient,
                    input.tokenId,
                    input.units,
                    input.salt,
                    input.nftContract,
                    input.paymentToken,
                    input.paymentAmount,
                    input.expiryToken
                )
            );
            return hash.toEthSignedMessageHash(); // This will add the prefix and hash the message
        }

    }
    */

    public class EthereumSignature
    {
        public string R { get; set; }
        public string S { get; set; }
        public int V { get; set; }

        public static EthereumSignature SplitSignature(string signature)
        {
            if (string.IsNullOrWhiteSpace(signature) || signature.Length != 132 || !signature.StartsWith("0x"))
            {
                throw new ArgumentException("Invalid Ethereum signature format", nameof(signature));
            }

            var r = signature.Substring(2, 64);
            var s = signature.Substring(66, 64);
            var v = int.Parse(signature.Substring(130, 2), System.Globalization.NumberStyles.HexNumber);

            return new EthereumSignature { R = r, S = s, V = v };
        }
    }


    public class Program : BaseSigner
    {
        private readonly Key _signingKey;
        private static string PrivateKey = "ADD_PRIVATE_KEY";
        private const string MintingContractAddress = "0xaf0E6743bc86ebb35eBE9531d82FB326D466c4b5";
        private const string PlaceablesContractAddress = "0x7D0FAa703CD188a630b516a69Ceb2c87D9896DdA";
        private const string MintingNftContractAddress = "ADD_MINTING_CONTRACT_ADDRESS";
        private const string AutographMinterContractAddress = "0x6b8D486fD16f94811bC41f5129f1Ec076A76D385";
        private const string MintingContractAbi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"approved\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"getApproved\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"ownerOf\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"}],\"name\":\"safeMint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"tokenURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        private const string ProviderUrl = "ADD_NODE_URL";
        private const string AutoGraphMinter = "[{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_core\",\"type\":\"address\"},{\"internalType\":\"address[]\",\"name\":\"_nftContracts\",\"type\":\"address[]\"},{\"internalType\":\"uint128\",\"name\":\"_replenishRatePerSecond\",\"type\":\"uint128\"},{\"internalType\":\"uint128\",\"name\":\"_bufferCap\",\"type\":\"uint128\"},{\"internalType\":\"address\",\"name\":\"_paymentRecipient\",\"type\":\"address\"},{\"internalType\":\"uint8\",\"name\":\"_expiryTokenHoursValid\",\"type\":\"uint8\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"oldBufferCap\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newBufferCap\",\"type\":\"uint256\"}],\"name\":\"BufferCapUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amountReplenished\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"bufferRemaining\",\"type\":\"uint256\"}],\"name\":\"BufferReplenished\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"amountUsed\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"bufferRemaining\",\"type\":\"uint256\"}],\"name\":\"BufferUsed\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"oldCore\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newCore\",\"type\":\"address\"}],\"name\":\"CoreUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"tokenIds\",\"type\":\"uint256[]\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"units\",\"type\":\"uint256[]\"}],\"name\":\"ERC1155BatchMinted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"}],\"name\":\"ERC1155Minted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Paused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"paymentRecipient\",\"type\":\"address\"}],\"name\":\"PaymentRecipientUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"oldReplenishRatePerSecond\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newRateReplenishPerSecond\",\"type\":\"uint256\"}],\"name\":\"ReplenishRatePerSecondUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Unpaused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"WhitelistAddressAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"WhitelistAddressRemoved\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"}],\"name\":\"WhitelistedContractAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"}],\"name\":\"WhitelistedContractRemoved\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContractAddress\",\"type\":\"address\"}],\"name\":\"addWhitelistedContract\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"whitelistAddresses\",\"type\":\"address[]\"}],\"name\":\"addWhitelistedContracts\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"buffer\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"bufferCap\",\"outputs\":[{\"internalType\":\"uint128\",\"name\":\"\",\"type\":\"uint128\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"bufferRemaining\",\"outputs\":[{\"internalType\":\"uint224\",\"name\":\"\",\"type\":\"uint224\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"core\",\"outputs\":[{\"internalType\":\"contract Core\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"target\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"}],\"internalType\":\"struct CoreRef.Call[]\",\"name\":\"calls\",\"type\":\"tuple[]\"}],\"name\":\"emergencyAction\",\"outputs\":[{\"internalType\":\"bytes[]\",\"name\":\"returnData\",\"type\":\"bytes[]\"}],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"}],\"name\":\"expiredHashes\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"used\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"expiryTokenHoursValid\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.HashInputsParams\",\"name\":\"input\",\"type\":\"tuple\"}],\"name\":\"getHash\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getWhitelistedAddresses\",\"outputs\":[{\"internalType\":\"address[]\",\"name\":\"\",\"type\":\"address[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_address\",\"type\":\"address\"}],\"name\":\"isWhitelistedAddress\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"lastBufferUsedTime\",\"outputs\":[{\"internalType\":\"uint32\",\"name\":\"\",\"type\":\"uint32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchForFree\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchWithEthAsFee\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintBatchParams[]\",\"name\":\"inputs\",\"type\":\"tuple[]\"}],\"name\":\"mintBatchWithPaymentTokenAsFee\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"name\":\"mintForFree\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintWithEthAsFeeParams\",\"name\":\"params\",\"type\":\"tuple\"}],\"name\":\"mintWithEthAsFee\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"units\",\"type\":\"uint256\"},{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"salt\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"},{\"internalType\":\"address\",\"name\":\"nftContract\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"paymentToken\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"paymentAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiryToken\",\"type\":\"uint256\"}],\"internalType\":\"struct ERC1155AutoGraphMinter.MintWithPaymentTokenAsFeeParams\",\"name\":\"params\",\"type\":\"tuple\"}],\"name\":\"mintWithPaymentTokenAsFee\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"pause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paused\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paymentRecipient\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"hash\",\"type\":\"bytes32\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"name\":\"recoverSigner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"nftContractAddress\",\"type\":\"address\"}],\"name\":\"removeWhitelistedContract\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"whitelistAddresses\",\"type\":\"address[]\"}],\"name\":\"removeWhitelistedContracts\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"replenishRatePerSecond\",\"outputs\":[{\"internalType\":\"uint128\",\"name\":\"\",\"type\":\"uint128\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint128\",\"name\":\"newBufferCap\",\"type\":\"uint128\"}],\"name\":\"setBufferCap\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newCore\",\"type\":\"address\"}],\"name\":\"setCore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint128\",\"name\":\"newRateLimitPerSecond\",\"type\":\"uint128\"}],\"name\":\"setReplenishRatePerSecond\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unpause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint8\",\"name\":\"_expiryTokenHoursValid\",\"type\":\"uint8\"}],\"name\":\"updateExpiryTokenHoursValid\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_paymentRecipient\",\"type\":\"address\"}],\"name\":\"updatePaymentRecipient\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

        public static async Task Main(string[] args)
        {
            //await GetRpcData();
            //await TransferEther();
            //await Mint();
            //await Mint2();
            //await MintAutoGraph();
            GetHash().Wait();
        }
        
        public partial class GetHashFunction : GetHashFunctionBase { }

        [Function("getHash", "bytes32")]
        public class GetHashFunctionBase : FunctionMessage
        {
            [Parameter("tuple", "input", 1)]
            public virtual HashInputsParams Input { get; set; }
        }

        public static async Task GetHash()
        {
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(421613));

            long expiryToken = ConvertToUnixTimestamp(DateTime.UtcNow) - 1000;
            Console.WriteLine("Expiry Token: " + expiryToken);
            // example tuple for smart contract
            var inputParams = new HashInputsParams
            {
                Recipient = ethereumService.GetAddress(PrivateKey),
                TokenId = new BigInteger(0),
                Units = new BigInteger(1),
                Salt = new BigInteger(100),
                NftContract = PlaceablesContractAddress,
                PaymentToken = "0x0000000000000000000000000000000000000000",
                PaymentAmount = new BigInteger(0),
                ExpiryToken = new BigInteger(expiryToken)
            };
            GetHashFunctionBase getHash = new GetHashFunctionBase
            {
                Input = inputParams,
                Gas = ethereumService._provider.GetGasPrice().Result,
                GasPrice = new HexBigInteger(100000000)
            };
                
            var contractHandler = ethereumService._web3.Eth.GetContractHandler(AutographMinterContractAddress);
            var txHash = await contractHandler.SendRequestAsync(getHash);
            Console.WriteLine("Transaction Hash: " + txHash);
            Console.WriteLine("Expiry Token: " + expiryToken);
        }

        // standard way of minting 
        public static async Task MintAutoGraph()
        {

            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(421613));
            try
            {
                var contract = new Contract(AutoGraphMinter, AutographMinterContractAddress, ethereumService.GetProvider());
                long expiryToken = ConvertToUnixTimestamp(DateTime.UtcNow) - 1000;
                Console.WriteLine("Expiry Token: " + expiryToken);
                // example tuple for smart contract
                var inputParams = new HashInputsParams
                {
                    Recipient = ethereumService.GetAddress(PrivateKey),
                    TokenId = new BigInteger(0),
                    Units = new BigInteger(1),
                    Salt = new BigInteger(100),
                    NftContract = PlaceablesContractAddress,
                    PaymentToken = "0x0000000000000000000000000000000000000000",
                    PaymentAmount = new BigInteger(0),
                    ExpiryToken = new BigInteger(expiryToken)
                };
                
                var hashService = new HashService();
                byte[] hash = hashService.GetHash(inputParams);
                Console.WriteLine("Input Params Hash: " + hash.ToHex());
                var signer1 = new EthereumMessageSigner();
                var signature1 = signer1.Sign(hash, PrivateKey);

                var result = EthereumSignature.SplitSignature(signature1);

                Console.WriteLine($"r: {"0x" + result.R}");
                Console.WriteLine($"s: {"0x" + result.S}");
                Console.WriteLine($"v: {result.V}");
                Console.WriteLine("Signature: " + signature1);

                string recoveredAddress = signer1.EcRecover(hash, signature1);
                Console.WriteLine("Recovered Address: " + recoveredAddress);
                bool isSameSigner = recoveredAddress.Equals(ethereumService.GetAddress(PrivateKey), StringComparison.OrdinalIgnoreCase);
                Console.WriteLine("Is Same Signer: " + isSameSigner);

                //Console.WriteLine("Hash Length: " + hashHex.Length);
                //Console.WriteLine($"Hash: {hashHex}");
                
                GetHashFunctionBase getHash = new GetHashFunctionBase
                {
                    Input = inputParams,
                    Gas = ethereumService._provider.GetGasPrice().Result,
                    GasPrice = new HexBigInteger(100000000)
                };
                
                var contractHandler = ethereumService._web3.Eth.GetContractHandler(AutographMinterContractAddress);
                var txHash = await contractHandler.SendRequestAsync(getHash);
                Console.WriteLine("Transaction Hash: " + txHash);
                Console.WriteLine("Expiry Token: " + expiryToken);
                
                //var key = new EthECKey(PrivateKey);
                //EthECDSASignature signature = key.Sign(txHash.HexToByteArray());
                //Console.WriteLine("Signature R: " + signature.R.ToHex());
                //Console.WriteLine("Signature S: " + signature.S.ToHex());

                /*var signer = new EthereumMessageSigner();
                string recoveredAddressGetHash = signer.EcRecover(ConvertorForRLPEncodingExtensions.ToBytesForRLPEncoding(txHash), signature);
                Console.WriteLine($"Signer's Address: {recoveredAddressGetHash}");
                Console.WriteLine("Message Hash: " + txHash);*/
                // smart contract method to call
                //string method = "mintForFree";

                /*var _calldata = contract.Calldata(method, new object[]
                 {
                    ethereumService.GetAddress(PrivateKey),
                    0,
                    1,
                    hash, // This hash wont work. my modified hashService.GetHash(inputParams) -> hash is what you want for the offchain hash method.  
                    123,
                    signature.To64ByteArray(),
                    PlaceablesContractAddress,
                    //0x0000000000000000000000000000000000000000,
                    //0,
                    expiryToken
                 });
                Console.WriteLine("CallData: : " + _calldata);
                Console.WriteLine("Account: : " + ethereumService.GetAddress(PrivateKey));
                Console.WriteLine("Autograph: : " + AutographMinterContractAddress);
                Console.WriteLine("Gas: : " + ethereumService._provider.GetGasPrice().Result);

                TransactionInput txInput = new TransactionInput
                {
                    To = AutographMinterContractAddress,
                    From = ethereumService.GetAddress(PrivateKey),
                    Value = new HexBigInteger(0),
                    Data = _calldata,
                    GasPrice = ethereumService._provider.GetGasPrice().Result,
                    Gas = new HexBigInteger(100000),
                };

                Console.WriteLine("Transaction Input: " + JsonConvert.SerializeObject(txInput, Formatting.Indented));
                //var txHash = await ethereumService.SignAndSendTransactionAsync(txInput);
                //Console.WriteLine($"Transaction Hash: {txHash}");*/
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        // 0x9b263e70f75c9fee1d8b23bdf5d66f80ab60e6dcd08e4687458af015b30ae490
        public BaseProvider BaseProvider { get; }

        private static async Task GetRpcData()
        {
            var ethereumService = new EthereumService(ProviderUrl);
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
            var ethereumService = new EthereumService(PrivateKey, ProviderUrl, new HexBigInteger(421613));
            var txHash = await ethereumService.TransferEther("0x525b19d1cA89c3620b4A12B7D36970E410c8C5f5", 0.000001m);
            Console.WriteLine($"Hash: {txHash}");
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - origin;
            return (long)diff.TotalSeconds;
        }
        public static byte[] StringToBytes32(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (input.Length > 32)
            {
                throw new ArgumentException("String too long for bytes32");
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] result = new byte[32];
            Buffer.BlockCopy(inputBytes, 0, result, 0, inputBytes.Length);
            return result;
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
                var contract = new Contract(MintingContractAbi, MintingContractAddress, ethereumService.GetProvider());
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
        

        [Function("safeMint")]
        
        public class SafeMintFunctionBase2 : FunctionMessage
        {
            [Parameter("address", "to", 1)]
            public virtual string To { get; set; }
 
        }
        


        public static async Task Mint2()
        {
            long expiryToken = ConvertToUnixTimestamp(DateTime.UtcNow) - 1000;
            Console.WriteLine("Expiry Token: " + expiryToken);
            var inputParams = new HashInputsParams
            {
                Recipient = "0x525b19d1cA89c3620b4A12B7D36970E410c8C5f5",
                TokenId = new BigInteger(0),
                Units = new BigInteger(1),
                Salt = new BigInteger(100),
                NftContract = PlaceablesContractAddress,
                PaymentToken = "0x0000000000000000000000000000000000000000",
                PaymentAmount = new BigInteger(0),
                ExpiryToken = new BigInteger(expiryToken)
            };
            var ethereumService = new EthereumService(PrivateKey, "https://arbitrum-goerli.infura.io/v3/19c93a677dff44fab7e35b4b459e4d01",new HexBigInteger(421613) );
            var getHash = new GetHashFunctionBase()
            {
                Input = inputParams,
            };
            //safeMint.FromAddress = "0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4";
            getHash.Gas = ethereumService._provider.GetGasPrice().Result;
            getHash.GasPrice = new HexBigInteger(100000000);
            var contractHandler = ethereumService._web3.Eth.GetContractHandler(AutographMinterContractAddress);
            var txHash = await contractHandler.SendRequestAsync(getHash);
            Console.WriteLine("Transaction Hash: " + txHash);
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

        public Program(IProvider provider, BaseProvider baseProvider, Key signingKey) : base(provider)
        {
            BaseProvider = baseProvider;
            _signingKey = signingKey;
        }
    }
}
