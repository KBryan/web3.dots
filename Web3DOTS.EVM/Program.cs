using System;
using static System.Console;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Newtonsoft.Json;
using NBitcoin;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Rijndael256;
using Account = Nethereum.Web3.Accounts.Account;

namespace Verify
{
    public class VerifyConsole
    {
        // TODO: Specify wich network you are going to use.
        const string network = "ADD_RPC_URL"; 
        const string privateKey = "ADD_PRIVATE_KEY";

        const string workingDirectory = @"Wallets\"; // Path where you want to store the Wallets
        public static async Task Main(string[] args)
        {
            //await GetHashMessage();
            /*var input = new HashInputsParams()
            {
                Recipient = "0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4",
                TokenId = new BigInteger(1),
                Units = new BigInteger(1),
                Salt = new BigInteger(123),
                NftContract = "0x312A00D9183c155Bac1eE736441536D8c15429D7",
                PaymentToken = "0x0000000000000000000000000000000000000000",
                PaymentAmount = new BigInteger(0),
                ExpiryToken = new BigInteger(1692532467)
            };*/

            //await GetHashMessage();
            //var signer = new EthereumMessageSigner();

            //EthECDSASignature signedMessage =  signer.SignAndCalculateV("1723e1963d629b1275f8dfd530218f6b716306937a81c301cbeae8e234e7c3f7".HexToByteArray(),"a7b4eee5fee47ed331e6ccaad100730e9232b6521383f223308a430d5ce3f588");
            //Console.WriteLine("Signed Message: " + signedMessage.CreateStringSignature());

            //await MintForFree();
            CreateWalletAsync(args).GetAwaiter().GetResult();
            CreateWalletDialog();
        }
        
        static async Task CreateWalletAsync(string[] args)
        {
            //Initial params.
            string[] availableOperations =
            {
                "create", "load", "recover", "exit" // Allowed functionality
            };
            string input = string.Empty;
            bool isWalletReady = false;
            Wallet wallet = new Wallet(Wordlist.English, WordCount.Twelve);


            // TODO: Initialize the Web3 instance and create the Storage Directory

            Web3 web3 = new Web3(network);
            Directory.CreateDirectory(workingDirectory);

            while (!input.ToLower().Equals("exit"))
            {
                if (!isWalletReady) // User still doesn't have an wallet.
                {
                    do
                    {
                        input = ReceiveCommandCreateLoadOrRecover();

                    } while (!((IList)availableOperations).Contains(input));
                    switch (input)
                    {
                        // Create brand-new wallet. User will receive mnemonic phrase, public and private keys.
                        case "create":
                            wallet = CreateWalletDialog();
                            isWalletReady = true;
                            break;

                        // Load wallet from json file contains encrypted mnemonic phrase (words).
                        // This command will decrypt words and load wallet.
                        case "load":
                            wallet = LoadWalletDialog();
                            isWalletReady = true;
                            break;

                        /* Recover wallet from mnemonic phrase (words) which user must enter.
                         This is usefull if user has wallet, but has no json file for him
                         (for example if he uses this program for the first time).
                         Command will creates new Json file contains encrypted mnemonic phrase (words)
                         for this wallet.
                         After encrypt words program will load wallet.*/
                        case "recover":
                            wallet = RecoverWalletDialog();
                            isWalletReady = true;
                            break;

                        // Exit from the program.
                        case "exit":
                            return;
                    }
                }
                else // Already having loaded Wallet
                {
                    string[] walletAvailableOperations = {
                        "balance", "receive", "send", "exit" //Allowed functionality
                    };

                    string inputCommand = string.Empty;

                    while (!inputCommand.ToLower().Equals("exit"))
                    {
                        do
                        {
                            inputCommand = ReceiveCommandForEthersOperations();

                        } while (!((IList)walletAvailableOperations).Contains(inputCommand));
                        switch (inputCommand)
                        {
                            // Send transaction from address to address
                            case "send":
                                await SendTransactionDialog(wallet);
                                break;

                            // Shows the balances of addresses and total balance.
                            case "balance":
                                await GetWalletBallanceDialog(web3, wallet);
                                break;

                            // Shows the addresses in which you can receive coins.
                            case "receive":
                                Receive(wallet);
                                break;
                            case "exit":
                                return;
                        }
                    }
                }

            }
        }
        
        // Provided Dialogs.
        private static Wallet CreateWalletDialog()
        {
            try
            {
                string password;
                string passwordConfirmed;
                do
                {
                    Write("Enter password for encryption: ");
                    password = ReadLine();
                    Write("Confirm password: ");
                    passwordConfirmed = ReadLine();
                    if (password != passwordConfirmed)
                    {
                        WriteLine("Passwords did not match!");
                        WriteLine("Try again.");
                    }
                } while (password != passwordConfirmed);

                // Creating new Wallet with the provided password.
                Wallet wallet = CreateWallet(password, workingDirectory);
                return wallet;
            }
            catch (Exception)
            {
                WriteLine($"ERROR! Wallet in path {workingDirectory} can`t be created!");
                throw;
            }
        }
        private static Wallet LoadWalletDialog()
        {
            Write("Enter: Name of the file containing wallet: ");
            string nameOfWallet = ReadLine();
            Write("Enter: Password: ");
            string pass = ReadLine();
            try
            {
                // Loading the Wallet from an JSON file. Using the Password to decrypt it.
                Wallet wallet = LoadWalletFromJsonFile(nameOfWallet, workingDirectory, pass);
                return (wallet);

            }
            catch (Exception)
            {
                WriteLine($"ERROR! Wallet {nameOfWallet} in path {workingDirectory} can`t be loaded!");
                throw;
            }
        }
        private static Wallet RecoverWalletDialog()
        {
            try
            {
                Write("Enter: Mnemonic words with single space separator: ");
                string mnemonicPhrase = ReadLine();
                Write("Enter: password for encryption: ");
                string passForEncryptionInJsonFile = ReadLine();

                // Recovering the Wallet from Mnemonic Phrase
                Wallet wallet = RecoverFromMnemonicPhraseAndSaveToJson(
                    mnemonicPhrase, passForEncryptionInJsonFile, workingDirectory);
                return wallet;
            }
            catch (Exception)
            {
                WriteLine("ERROR! Wallet can`t be recovered! Check your mnemonic phrase.");
                throw;
            }
        }
        private static async Task GetWalletBallanceDialog(Web3 web3, Wallet wallet)
        {
            WriteLine("Balance:");
            try
            {
                // Getting the Balance and Displaying the Information.
                await Balance(web3, wallet);
            }
            catch (Exception)
            {
                WriteLine("Error occured! Check your wallet.");
            }
        }
        private static async Task SendTransactionDialog(Wallet wallet)
        {
            WriteLine("Enter: Address sending ethers.");
            string fromAddress = ReadLine();
            WriteLine("Enter: Address receiving ethers.");
            string toAddress = ReadLine();
            WriteLine("Enter: Amount of coins in ETH.");
            double amountOfCoins = 0d;
            try
            {
                amountOfCoins = double.Parse(ReadLine());
            }
            catch (Exception)
            {
                WriteLine("Unacceptable input for amount of coins.");
            }
            if (amountOfCoins > 0.0d)
            {
                WriteLine($"You will send {amountOfCoins} ETH from {fromAddress} to {toAddress}");
                WriteLine($"Are you sure? yes/no");
                string answer = ReadLine();
                if (answer != null && answer.ToLower() == "yes")
                {
                    // Send the Transaction.
                    await Send(wallet, fromAddress, toAddress, amountOfCoins);
                }
            }
            else
            {
                WriteLine("Amount of coins for transaction must be positive number!");
            }
        }
        private static string ReceiveCommandCreateLoadOrRecover()
        {
            WriteLine("Choose working wallet.");
            WriteLine("Choose [create] to Create new Wallet.");
            WriteLine("Choose [load] to load existing Wallet from file.");
            WriteLine("Choose [recover] to recover Wallet with Mnemonic Phrase.");
            Write("Enter operation [\"Create\", \"Load\", \"Recover\", \"Exit\"]: ");
            string input = ReadLine()?.ToLower().Trim();
            return input;
        }
        private static string ReceiveCommandForEthersOperations()
        {
            Write("Enter operation [\"Balance\", \"Receive\", \"Send\", \"Exit\"]: ");
            string inputCommand = ReadLine()?.ToLower().Trim();
            return inputCommand;
        }
        
        /// Asynchronously creates a new wallet with a specified password and saves it to a JSON file.
        /// </summary>
        /// <param name="password">The password used for encrypting the wallet.</param>
        /// <param name="pathfile">The directory path where the wallet JSON file will be saved.</param>
        /// <returns>A new Wallet instance.</returns>
        public static Wallet CreateWallet(string password, string pathfile)
        {
            // TODO: Create brand-new wallet and get all the Words that were used.

            var wallet = new Wallet(Wordlist.English, WordCount.Twelve);
            var words = string.Join(" ", wallet.Words);

            try
            {
                // TODO: Save the Wallet in the desired Directory.
                var fileName = SaveWalletToJsonFile(wallet, password, pathfile);
                WriteLine("Filename path: " + fileName);
            }
            catch (Exception e)
            {
                WriteLine($"ERROR! The file can`t be saved! {e}");
                throw;
            }

            WriteLine("New Wallet was created successfully!");
            WriteLine("Write down the following mnemonic words and keep them in the save place.");
            // TODO: Display the Words here.
            WriteLine(words);
            WriteLine("Seed: ");
            // TODO: Display the Seed here.
            WriteLine(wallet.Seed);
            WriteLine();
            // TODO: Implement and use PrintAddressesAndKeys to print all the Addresses and Keys.
            PrintAddressesAndKeys(wallet);

            return wallet;
        }
        private static void PrintAddressesAndKeys(Wallet wallet)
        {
            // TODO: Print all the Addresses and the coresponding Private Keys.
            WriteLine("Addresses: ");
            for (int i = 0; i < 1; i++)
            {
                WriteLine(wallet.GetAccount(i).Address);
            }

            WriteLine();
            WriteLine("Private Keys: ");
            for (int i = 0; i < 1; i++)
            {
                WriteLine(wallet.GetAccount(i).PrivateKey);
            }

            WriteLine();
        }
        private static string SaveWalletToJsonFile(Wallet wallet, string password, string pathfile)
        {
            //TODO: Encrypt and Save the Wallet to JSON.
            string words = string.Join(" ", wallet.Words);
            var encryptedWords = Rijndael.Encrypt(words, password, KeySize.Aes256);
            string date = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var walletJsonData = new { encryptedWords = encryptedWords, date = date };
            string json = JsonConvert.SerializeObject(walletJsonData);
            Random random = new Random();
            var fileName =
                "EthereumWallet_"
                + DateTime.Now.Year + "-"
                + DateTime.Now.Month + "-"
                + DateTime.Now.Day + "-"
                + DateTime.Now.Hour + "-"
                + DateTime.Now.Minute + "-"
                + DateTime.Now.Second + "-"
                + random.Next(0, 1000) + ".json";
            File.WriteAllText(Path.Combine(pathfile, fileName), json);
            WriteLine($"Wallet saved in file: {fileName}");
            return fileName;
        }

        static Wallet LoadWalletFromJsonFile(string nameOfWalletFile, string path, string pass)
        {
            //TODO: Implement the logic that is needed to Load and Wallet from JSON.
            string pathToFile = Path.Combine(path, nameOfWalletFile);
            string words = string.Empty;
            WriteLine($"Read from {pathToFile}");
            try
            {
                string line = File.ReadAllText(pathToFile);
                dynamic results = JsonConvert.DeserializeObject<dynamic>(line);
                string encryptedWords = results.encryptedWords;
                words = Rijndael.Decrypt(encryptedWords, pass, KeySize.Aes256);
                string dataAndTime = results.date;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR!" + e);
            }

            return Recover(words);
        }
        public static Wallet Recover(string words)
        {
            // TODO: Recover a Wallet from existing mnemonic phrase (words).
            Wallet wallet = new Wallet(words, null);
            WriteLine("Wallet was successfully recovered.");
            WriteLine("Words: " + string.Join(" ", wallet.Words));
            WriteLine("Seed: " + string.Join(" ", wallet.Seed));
            WriteLine();
            PrintAddressesAndKeys(wallet);
            return wallet;
        }
        
        /// <summary>
        /// Recovers a wallet from a given mnemonic phrase and saves it to a JSON file.
        /// </summary>
        /// <param name="words">The mnemonic phrase used to recover the wallet.</param>
        /// <param name="password">The password used for encrypting the wallet before saving to JSON.</param>
        /// <param name="pathfile">The directory path where the wallet JSON file will be saved.</param>
        /// <returns>The Wallet instance recovered from the mnemonic phrase.</returns>
        public static Wallet RecoverFromMnemonicPhraseAndSaveToJson(string words, string password, string pathfile)
        {
            // TODO: Recover from Mnemonic phrases and Save to JSON.
            Wallet wallet = Recover(words);
            string fileName = string.Empty;
            try
            {
                fileName = SaveWalletToJsonFile(wallet, password, pathfile);
            }
            catch (Exception)
            {
                WriteLine($"ERROR! The file {fileName} with recovered wallet can't be saved!");
                throw;
            }

            return wallet;
        }

        public static void Receive(Wallet wallet)
        {
            // TODO: Print all avaiable addresses in Wallet.
            if (wallet.GetAddresses().Any())
            {
                for (var i = 0; i < 20; i++)
                {
                    WriteLine(wallet.GetAccount(i).Address);
                }
                WriteLine();
            }
            else
            {
                WriteLine("No addresses found!");
            }
        }
        
        /// <summary>
        /// Sends a transaction from one address to another with a specified amount of Ether.
        /// </summary>
        /// <param name="wallet">The wallet from which the transaction will be sent.</param>
        /// <param name="fromAddress">The address sending Ether.</param>
        /// <param name="toAddress">The address receiving Ether.</param>
        /// <param name="amountOfCoins">The amount of Ether to send.</param>
        /// <returns>An asynchronous task that represents the operation.</returns>
        static async Task Send(Wallet wallet, string fromAddress, string toAddress, double amountOfCoins)
        {
            // TODO: Generate and Send a transaction.
            Account accountFrom = wallet.GetAccount(fromAddress);
            string privateKeyFrom = accountFrom.PrivateKey;
            if (privateKeyFrom == string.Empty)
            {
                WriteLine("Address sending coins is not from current wallet!");
                throw new Exception("Address sending coins is not from current wallet!");
            }

            var web3 = new Web3(accountFrom);
            var wei = Web3.Convert.ToWei(amountOfCoins);
            try
            {
                var transaction = await web3.TransactionManager.SendTransactionAsync(
                    accountFrom.Address,
                    toAddress,
                    new Nethereum.Hex.HexTypes.HexBigInteger(wei)
                );
                WriteLine("Transaction has been sent successfully!");
            }
            catch (Exception e)
            {
                WriteLine($"ERROR! The transaction can't be completed! {e}");
                throw e;
            }
        }
        
        /// <summary>
        /// Retrieves and displays the balance for each address in a wallet, as well as the total balance.
        /// </summary>
        /// <param name="web3">The Web3 instance used for Ethereum interactions.</param>
        /// <param name="wallet">The wallet for which balances will be retrieved.</param>
        /// <returns>An asynchronous task that represents the operation.</returns>
        static async Task Balance(Web3 web3, Wallet wallet)
        {
            // TODO: Print all addresses and their balance. Print the Total Balance of the Wallet as well.
            decimal totalBalance = 0.0m;
            for (int i = 0; i < 20; i++)
            {
                var balance = await web3.Eth.GetBalance.SendRequestAsync(wallet.GetAddresses(0).ToString());
                var etherAmount = Web3.Convert.FromWei(balance.Value);
                totalBalance += etherAmount;
                WriteLine(wallet.GetAccount(i).Address + " " + etherAmount + " ETH");
            }

            WriteLine($"Total balance: {totalBalance} ETH \n");
        }

        static long CalculateExpiryToken()
        {
            long currentUnixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return currentUnixTimestamp - 1000;
        }
    
        /// <summary>
        /// Retrieves a hash message from a smart contract based on specified input parameters.
        /// </summary>
        /// <returns>An asynchronous task that represents the operation.</returns>

        public static async Task GetHashMessage()
        {

            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            var web3 = new Web3(account, network);
            
            var contractHandler = web3.Eth.GetContractHandler("0x65428e4937dbFc829ddBc6d8611E21D11559231D");

            /** Function: getHash**/
            Console.WriteLine("Expiry Token: " + CalculateExpiryToken());
            var input = new HashInputsParams()
            {
                Recipient = "0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4",
                TokenId = new BigInteger(1),
                Units = new BigInteger(1),
                Salt = new BigInteger(123),
                NftContract = "0x312A00D9183c155Bac1eE736441536D8c15429D7",
                PaymentToken = "0x0000000000000000000000000000000000000000",
                PaymentAmount = new BigInteger(0),
                ExpiryToken = new BigInteger(CalculateExpiryToken())
            };

            var getHashFunction = new GetHashFunction
            {
                Input = input
            };
            var getHashFunctionReturn = await contractHandler.QueryAsync<GetHashFunction, byte[]>(getHashFunction);

            Console.WriteLine("Hash without 0x: " + String.Concat(getHashFunctionReturn.ToHex()));
        }
        /// <summary>
        /// Calls the 'mintForFree' function of a smart contract to mint a token without cost under certain conditions.
        /// </summary>
        /// <returns>An asynchronous task that represents the operation, including the transaction receipt.</returns>
        static async Task MintForFree()
        {
            var account = new Nethereum.Web3.Accounts.Account(privateKey,new BigInteger(421613));
            var web3 = new Web3(account, network);
            var signer = new EthereumMessageSigner();
            var contractHandler = web3.Eth.GetContractHandler("0x6b8D486fD16f94811bC41f5129f1Ec076A76D385");
            var expiryToken = CalculateExpiryToken();
            var input = new HashInputsParams()
            {
                Recipient = "0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4",
                TokenId = new BigInteger(1),
                Units = new BigInteger(1),
                Salt = new BigInteger(123),
                NftContract = "0x312A00D9183c155Bac1eE736441536D8c15429D7",
                PaymentToken = "0x0000000000000000000000000000000000000000",
                PaymentAmount = new BigInteger(0),
                ExpiryToken = new BigInteger(expiryToken)
            };

            var getHashFunction = new GetHashFunction
            {
                Input = input
            };
            var getHashFunctionReturn = await contractHandler.QueryAsync<GetHashFunction, byte[]>(getHashFunction);
            var hashData = "0x" + BitConverter.ToString(getHashFunctionReturn).Replace("-", "").ToLower();
            Console.WriteLine("Get Hash: " + hashData);
            EthECDSASignature signedMessage =  signer.SignAndCalculateV(getHashFunctionReturn,"ADD_PRIVATE_KEY");
            Console.WriteLine("SignAndCalculateV: " + signedMessage.CreateStringSignature().HexToByteArray());
            var mintForFreeFunction = new MintForFreeFunction
            {
                Recipient = "0x5Bf3DC356A5e41021AE208667a835DfB143Bf4b4",
                TokenId = new BigInteger(1),
                Units = new BigInteger(1),
                Hash =  hashData.HexToByteArray(),
                Salt = new BigInteger(123),
                Signature = SignatureExtensions.CreateStringSignature(signedMessage).HexToByteArray(),
                NftContract = "0x312A00D9183c155Bac1eE736441536D8c15429D7",
                ExpiryToken = new BigInteger(expiryToken),
                Gas = new BigInteger(100000000),
                GasPrice = new BigInteger(100000000)
            };
            
            var mintForFreeFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(mintForFreeFunction);
            Console.WriteLine("Transaction Hash: " + mintForFreeFunctionTxnReceipt.TransactionHash);
        }
    }
    
    public class EthereumMessageHashMessage
    {
        private const string MessagePrefix = "\x19Ethereum Signed Message:\n";

        public static string HashMessage(string message)
        {
            byte[] messageBytes;
            if (message.StartsWith("0x"))
            {
                messageBytes = message.HexToByteArray();
            }
            else
            {
                messageBytes = Encoding.UTF8.GetBytes(message);
            }
            var messageLength = Encoding.UTF8.GetBytes(messageBytes.Length.ToString());

            var combinedMessage = CombineBytes(Encoding.UTF8.GetBytes(MessagePrefix), messageLength, messageBytes);
            Sha3Keccack shaKec = new Sha3Keccack();
            return shaKec.CalculateHash(combinedMessage).ToHex();
        }

        private static byte[] CombineBytes(params byte[][] arrays)
        {
            
            var result = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }
    }
}

public partial class VerifyDeployment : VerifyDeploymentBase
{
    public VerifyDeployment() : base(BYTECODE) { }
    public VerifyDeployment(string byteCode) : base(byteCode) { }
}

public class VerifyDeploymentBase : ContractDeploymentMessage
{
    //public static string BYTECODE = "608060405234801561001057600080fd5b5061072c806100206000396000f3fe608060405234801561001057600080fd5b50600436106100365760003560e01c806304958b651461003b578063659934c11461006b575b600080fd5b61005560048036038101906100509190610403565b61009b565b604051610062919061044a565b60405180910390f35b610085600480360381019061008091906104ca565b610106565b6040516100929190610540565b60405180910390f35b600080826000015183602001518460400151856060015186608001518760a001518860c001518960e001516040516020016100dd98979695949392919061056a565b6040516020818303038152906040528051906020012090506100fe816101cf565b915050919050565b6000806040518060400160405280601c81526020017f19457468657265756d205369676e6564204d6573736167653a0a33320000000081525090506000818760405160200161015692919061067a565b60405160208183030381529060405280519060200120905060006001828888886040516000815260200160405260405161019394939291906106b1565b6020604051602081039080840390855afa1580156101b5573d6000803e3d6000fd5b505050602060405103519050809350505050949350505050565b60007f19457468657265756d205369676e6564204d6573736167653a0a33320000000060005281601c52603c6000209050919050565b6000604051905090565b600080fd5b600080fd5b6000601f19601f8301169050919050565b7f4e487b7100000000000000000000000000000000000000000000000000000000600052604160045260246000fd5b61026282610219565b810181811067ffffffffffffffff821117156102815761028061022a565b5b80604052505050565b6000610294610205565b90506102a08282610259565b919050565b600073ffffffffffffffffffffffffffffffffffffffff82169050919050565b60006102d0826102a5565b9050919050565b6102e0816102c5565b81146102eb57600080fd5b50565b6000813590506102fd816102d7565b92915050565b6000819050919050565b61031681610303565b811461032157600080fd5b50565b6000813590506103338161030d565b92915050565b600061010082840312156103505761034f610214565b5b61035b61010061028a565b9050600061036b848285016102ee565b600083015250602061037f84828501610324565b602083015250604061039384828501610324565b60408301525060606103a784828501610324565b60608301525060806103bb848285016102ee565b60808301525060a06103cf848285016102ee565b60a08301525060c06103e384828501610324565b60c08301525060e06103f784828501610324565b60e08301525092915050565b6000610100828403121561041a5761041961020f565b5b600061042884828501610339565b91505092915050565b6000819050919050565b61044481610431565b82525050565b600060208201905061045f600083018461043b565b92915050565b61046e81610431565b811461047957600080fd5b50565b60008135905061048b81610465565b92915050565b600060ff82169050919050565b6104a781610491565b81146104b257600080fd5b50565b6000813590506104c48161049e565b92915050565b600080600080608085870312156104e4576104e361020f565b5b60006104f28782880161047c565b9450506020610503878288016104b5565b93505060406105148782880161047c565b92505060606105258782880161047c565b91505092959194509250565b61053a816102c5565b82525050565b60006020820190506105556000830184610531565b92915050565b61056481610303565b82525050565b600061010082019050610580600083018b610531565b61058d602083018a61055b565b61059a604083018961055b565b6105a7606083018861055b565b6105b46080830187610531565b6105c160a0830186610531565b6105ce60c083018561055b565b6105db60e083018461055b565b9998505050505050505050565b600081519050919050565b600081905092915050565b60005b8381101561061c578082015181840152602081019050610601565b60008484015250505050565b6000610633826105e8565b61063d81856105f3565b935061064d8185602086016105fe565b80840191505092915050565b6000819050919050565b61067461066f82610431565b610659565b82525050565b60006106868285610628565b91506106928284610663565b6020820191508190509392505050565b6106ab81610491565b82525050565b60006080820190506106c6600083018761043b565b6106d360208301866106a2565b6106e0604083018561043b565b6106ed606083018461043b565b9594505050505056fea2646970667358221220e5f8569a6fd56ec07a7d711939c448b51939b76fe584fe25ec00671c99664d7864736f6c63430008120033";
    public static string BYTECODE = "6080604052600436106101e35760003560e01c80635fae05761161010257806397aba7f911610095578063dd6a7fc911610064578063dd6a7fc9146105bc578063edaafe20146105dc578063f2f4eb26146105f1578063f75d76941461061657600080fd5b806397aba7f91461053c578063b51bd0011461055c578063c51ebaf41461057c578063d0475bad1461059c57600080fd5b806380009630116100d157806380009630146104b55780638456cb59146104d5578063845cf31f146104ea57806393c611301461050a57600080fd5b80635fae0576146104405780636d028027146104605780636fd6ba13146104825780637df3927e1461049557600080fd5b80632b1eaf291161017a57806346704adb1161014957806346704adb146103bc57806354ce8f23146103dc5780635724a794146104085780635c975abb1461042857600080fd5b80632b1eaf291461033c5780633aa79672146103745780633c33077c146103875780633f4ba83a146103a757600080fd5b806310e0c357116101b657806310e0c3571461029c57806310e99845146102dc57806325daffef146102fc5780632a47aede1461031c57600080fd5b806304958b65146101e8578063078f20fb1461021b5780630ae7a8ed1461023d5780630ff24ee21461027c575b600080fd5b3480156101f457600080fd5b5061020861020336600461405c565b610656565b6040519081526020015b60405180910390f35b34801561022757600080fd5b5061023b610236366004614105565b610749565b005b34801561024957600080fd5b5060035461026490600160801b90046001600160801b031681565b6040516001600160801b039091168152602001610212565b34801561028857600080fd5b5061023b61029736600461412e565b610890565b3480156102a857600080fd5b506102cc6102b736600461414b565b60066020526000908152604090205460ff1681565b6040519015158152602001610212565b3480156102e857600080fd5b5061023b6102f73660046142ec565b610a02565b34801561030857600080fd5b5061023b610317366004614391565b610c46565b34801561032857600080fd5b5061023b6103373660046143d2565b610d86565b34801561034857600080fd5b5060055461035c906001600160a01b031681565b6040516001600160a01b039091168152602001610212565b61023b610382366004614466565b611067565b34801561039357600080fd5b5061023b6103a236600461412e565b61141d565b3480156103b357600080fd5b5061023b611586565b3480156103c857600080fd5b5061023b6103d736600461412e565b61175c565b3480156103e857600080fd5b506007546103f69060ff1681565b60405160ff9091168152602001610212565b34801561041457600080fd5b5061023b610423366004614534565b6118ce565b34801561043457600080fd5b5060025460ff166102cc565b34801561044c57600080fd5b506102cc61045b36600461412e565b611be5565b34801561046c57600080fd5b50610475611bf7565b6040516102129190614614565b61023b6104903660046142ec565b611c08565b6104a86104a3366004614391565b611e74565b60405161021291906146b1565b3480156104c157600080fd5b5061023b6104d036600461412e565b6120bc565b3480156104e157600080fd5b5061023b6121b9565b3480156104f657600080fd5b5061023b610505366004614713565b61238f565b34801561051657600080fd5b506004546105279063ffffffff1681565b60405163ffffffff9091168152602001610212565b34801561054857600080fd5b5061035c610557366004614736565b6124c0565b34801561056857600080fd5b50600354610264906001600160801b031681565b34801561058857600080fd5b5061023b610597366004614105565b6124cc565b3480156105a857600080fd5b5061023b6105b7366004614391565b61260d565b3480156105c857600080fd5b5061023b6105d736600461477c565b612747565b3480156105e857600080fd5b506102086129b6565b3480156105fd57600080fd5b5060025461035c9061010090046001600160a01b031681565b34801561062257600080fd5b5060045461063e9064010000000090046001600160e01b031681565b6040516001600160e01b039091168152602001610212565b600080826000015183602001518460400151856060015186608001518760a001518860c001518960e001516040516020016106df9897969594939291906001600160a01b039889168152602081019790975260408701959095526060860193909352908516608085015290931660a083015260c082019290925260e08101919091526101000190565b60408051601f1981840301815282825280516020918201207f19457468657265756d205369676e6564204d6573736167653a0a33320000000084830152603c80850182905283518086039091018152605c90940190925282519201919091209091505b9392505050565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d148549061079f90859033906004016147f0565b602060405180830381865afa1580156107bc573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906107e09190614807565b8061085d5750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061081c90849033906004016147f0565b602060405180830381865afa158015610839573d6000803e3d6000fd5b505050506040513d601f19601f8201168201806040525081019061085d9190614807565b6108825760405162461bcd60e51b815260040161087990614829565b60405180910390fd5b61088b83612b36565b505050565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d14854906108e690859033906004016147f0565b602060405180830381865afa158015610903573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906109279190614807565b806109a45750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061096390849033906004016147f0565b602060405180830381865afa158015610980573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906109a49190614807565b6109c05760405162461bcd60e51b815260040161087990614829565b6109c983612bb2565b6040516001600160a01b038416907fbf4ebb78df053fc7d1820e52893293ea77b46e98bdb1707c4fec4e9ed6661cc190600090a2505050565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa158015610a59573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190610a7d919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b158015610ac457600080fd5b505af1158015610ad8573d6000803e3d6000fd5b50505050610ae4612c66565b84610aee81611be5565b610b0a5760405162461bcd60e51b81526004016108799061486c565b600080610b1a8888600089612cae565b5060405163d81d0a1560e01b815291935091506001600160a01b0389169063d81d0a1590610b50908a9086908690600401614904565b600060405180830381600087803b158015610b6a57600080fd5b505af1158015610b7e573d6000803e3d6000fd5b50505050866001600160a01b0316886001600160a01b03167f0608e8249e0c6db2633b525da433eaf0569d763142ce2c311627af1184b4c99a8484604051610bc7929190614944565b60405180910390a35050506001600160a01b03811663094feeb4610bec600185614988565b6040516001600160e01b031960e084901b16815260ff9091166004820152602401600060405180830381600087803b158015610c2757600080fd5b505af1158015610c3b573d6000803e3d6000fd5b505050505050505050565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d1485490610c9c90859033906004016147f0565b602060405180830381865afa158015610cb9573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190610cdd9190614807565b80610d5a5750600254604051632474521560e21b81526101009091046001600160a01b0316906391d1485490610d1990849033906004016147f0565b602060405180830381865afa158015610d36573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190610d5a9190614807565b610d765760405162461bcd60e51b815260040161087990614829565b610d808484613042565b50505050565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa158015610ddd573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190610e01919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b158015610e4857600080fd5b505af1158015610e5c573d6000803e3d6000fd5b50505050610e68612c66565b83610e7281611be5565b610e8e5760405162461bcd60e51b81526004016108799061486c565b60006040518061010001604052808d6001600160a01b031681526020018c81526020018b8152602001898152602001876001600160a01b0316815260200160006001600160a01b031681526020016000815260200186815250905060006040518060c001604052808b8152602001610f0584610656565b81526020018981526020018c8152602001886001600160a01b03168152602001878152509050610f3481613084565b604051630ab714fb60e11b81526001600160a01b038e81166004830152602482018e9052604482018d905288169063156e29f690606401600060405180830381600087803b158015610f8557600080fd5b505af1158015610f99573d6000803e3d6000fd5b505050508b8d6001600160a01b0316886001600160a01b03167f3a306552631b0570dc1a987636b037a157d9055d6c15b904f990681212d7fd818e604051610fe391815260200190565b60405180910390a45050506001600160a01b03811663094feeb4611008600185614988565b6040516001600160e01b031960e084901b16815260ff9091166004820152602401600060405180830381600087803b15801561104357600080fd5b505af1158015611057573d6000803e3d6000fd5b5050505050505050505050505050565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa1580156110be573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906110e2919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b15801561112957600080fd5b505af115801561113d573d6000803e3d6000fd5b50505050611149612c66565b8260c0015161115781611be5565b6111735760405162461bcd60e51b81526004016108799061486c565b600060405180610100016040528086600001516001600160a01b031681526020018660200151815260200186604001518152602001866080015181526020018660c001516001600160a01b0316815260200160006001600160a01b031681526020018660e001518152602001866101000151815250905060006040518060c001604052808760600151815260200161120a84610656565b81526020018760a001518152602001876040015181526020018760c001516001600160a01b03168152602001876101000151815250905061124e8660e0015161330c565b61125781613084565b60055460e08701516040516000926001600160a01b031691908381818185875af1925050503d80600081146112a8576040519150601f19603f3d011682016040523d82523d6000602084013e6112ad565b606091505b50509050806112ce5760405162461bcd60e51b8152600401610879906149a1565b60c0870151875160208901516040808b01519051630ab714fb60e11b81526001600160a01b0393841660048201526024810192909252604482015291169063156e29f690606401600060405180830381600087803b15801561132f57600080fd5b505af1158015611343573d6000803e3d6000fd5b50505050866020015187600001516001600160a01b03168860c001516001600160a01b03167f3a306552631b0570dc1a987636b037a157d9055d6c15b904f990681212d7fd818a6040015160405161139d91815260200190565b60405180910390a450505050806001600160a01b031663094feeb46001846113c59190614988565b6040516001600160e01b031960e084901b16815260ff9091166004820152602401600060405180830381600087803b15801561140057600080fd5b505af1158015611414573d6000803e3d6000fd5b50505050505050565b600254604051632474521560e21b8152600080516020614bd28339815191529161010090046001600160a01b0316906391d148549061146290849033906004016147f0565b602060405180830381865afa15801561147f573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906114a39190614807565b6114bf5760405162461bcd60e51b8152600401610879906149ed565b6001600160a01b03821661153b5760405162461bcd60e51b815260206004820152603f60248201527f455243313135354175746f47726170684d696e7465723a207061796d656e745260448201527f6563697069656e74206d757374206e6f742062652061646472657373283029006064820152608401610879565b600580546001600160a01b0319166001600160a01b0384169081179091556040517f694f4a35894884360636162607be63896d92aff456c086ea693e15e2f209100590600090a25050565b600254604051632474521560e21b8152600080516020614bd283398151915291600080516020614bf2833981519152917f55435dd261a4b9b3364963f7738a7a662ad9c84396d64be3365284bb7f0a50419161010090046001600160a01b0316906391d14854906115fd90869033906004016147f0565b602060405180830381865afa15801561161a573d6000803e3d6000fd5b505050506040513d601f19601f8201168201806040525081019061163e9190614807565b806116bb5750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061167a90859033906004016147f0565b602060405180830381865afa158015611697573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906116bb9190614807565b806117385750600254604051632474521560e21b81526101009091046001600160a01b0316906391d14854906116f790849033906004016147f0565b602060405180830381865afa158015611714573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906117389190614807565b6117545760405162461bcd60e51b815260040161087990614829565b61088b6133a1565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d14854906117b290859033906004016147f0565b602060405180830381865afa1580156117cf573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906117f39190614807565b806118705750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061182f90849033906004016147f0565b602060405180830381865afa15801561184c573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906118709190614807565b61188c5760405162461bcd60e51b815260040161087990614829565b611895836133f3565b6040516001600160a01b038416907f56135cb92f507d24c4d85542876eb1dabb1db58e6f7ef6cbd0193d57f242818890600090a2505050565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa158015611925573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190611949919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b15801561199057600080fd5b505af11580156119a4573d6000803e3d6000fd5b505050506119b0612c66565b8260c001516119be81611be5565b6119da5760405162461bcd60e51b81526004016108799061486c565b600060405180610100016040528086600001516001600160a01b031681526020018660200151815260200186604001518152602001866080015181526020018660c001516001600160a01b031681526020018660e001516001600160a01b031681526020018661010001518152602001866101200151815250905060006040518060c0016040528087606001518152602001611a7584610656565b81526020018760a001518152602001876040015181526020018760c001516001600160a01b031681526020018761012001518152509050611abf8660e001518761010001516134a0565b611ac881613084565b60055461010087015160e0880151611af1926001600160a01b0391821692339290911690613540565b60c0860151865160208801516040808a01519051630ab714fb60e11b81526001600160a01b0393841660048201526024810192909252604482015291169063156e29f690606401600060405180830381600087803b158015611b5257600080fd5b505af1158015611b66573d6000803e3d6000fd5b50505050856020015186600001516001600160a01b03168760c001516001600160a01b03167f3a306552631b0570dc1a987636b037a157d9055d6c15b904f990681212d7fd818960400151604051611bc091815260200190565b60405180910390a45050506001600160a01b03811663094feeb46113c5600185614988565b6000611bf1818361359a565b92915050565b6060611c0360006135bc565b905090565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa158015611c5f573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190611c83919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b158015611cca57600080fd5b505af1158015611cde573d6000803e3d6000fd5b50505050611cea612c66565b84611cf481611be5565b611d105760405162461bcd60e51b81526004016108799061486c565b6000806000611d22898960008a612cae565b925092509250611d318161330c565b6005546040516000916001600160a01b03169083908381818185875af1925050503d8060008114611d7e576040519150601f19603f3d011682016040523d82523d6000602084013e611d83565b606091505b5050905080611da45760405162461bcd60e51b8152600401610879906149a1565b60405163d81d0a1560e01b81526001600160a01b038b169063d81d0a1590611dd4908c9088908890600401614904565b600060405180830381600087803b158015611dee57600080fd5b505af1158015611e02573d6000803e3d6000fd5b50505050886001600160a01b03168a6001600160a01b03167f0608e8249e0c6db2633b525da433eaf0569d763142ce2c311627af1184b4c99a8686604051611e4b929190614944565b60405180910390a35050505050806001600160a01b031663094feeb4600184610bec9190614988565b600254604051632474521560e21b8152606091600080516020614bd2833981519152916101009091046001600160a01b0316906391d1485490611ebd90849033906004016147f0565b602060405180830381865afa158015611eda573d6000803e3d6000fd5b505050506040513d601f19601f82011682018060405250810190611efe9190614807565b611f1a5760405162461bcd60e51b8152600401610879906149ed565b826001600160401b03811115611f3257611f32613f83565b604051908082528060200260200182016040528015611f6557816020015b6060815260200190600190039081611f505790505b50915060005b838110156120b4576000858583818110611f8757611f87614a24565b9050602002810190611f999190614a3a565b611fa790602081019061412e565b90506000868684818110611fbd57611fbd614a24565b9050602002810190611fcf9190614a3a565b602001359050366000888886818110611fea57611fea614a24565b9050602002810190611ffc9190614a3a565b61200a906040810190614a5a565b91509150600080856001600160a01b031685858560405161202c929190614aa0565b60006040518083038185875af1925050503d8060008114612069576040519150601f19603f3d011682016040523d82523d6000602084013e61206e565b606091505b50915091508161207d57600080fd5b8089888151811061209057612090614a24565b602002602001018190525050505050505080806120ac90614ab0565b915050611f6b565b505092915050565b600254604051632474521560e21b8152600080516020614bd28339815191529161010090046001600160a01b0316906391d148549061210190849033906004016147f0565b602060405180830381865afa15801561211e573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906121429190614807565b61215e5760405162461bcd60e51b8152600401610879906149ed565b600280546001600160a01b03848116610100818102610100600160a81b031985161790945560405193909204169182907f9209b7c8c06dcfd261686a663e7c55989337b18d59da5433c6f2835fb697092090600090a3505050565b600254604051632474521560e21b8152600080516020614bd283398151915291600080516020614bf2833981519152917f55435dd261a4b9b3364963f7738a7a662ad9c84396d64be3365284bb7f0a50419161010090046001600160a01b0316906391d148549061223090869033906004016147f0565b602060405180830381865afa15801561224d573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906122719190614807565b806122ee5750600254604051632474521560e21b81526101009091046001600160a01b0316906391d14854906122ad90859033906004016147f0565b602060405180830381865afa1580156122ca573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906122ee9190614807565b8061236b5750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061232a90849033906004016147f0565b602060405180830381865afa158015612347573d6000803e3d6000fd5b505050506040513d601f19601f8201168201806040525081019061236b9190614807565b6123875760405162461bcd60e51b815260040161087990614829565b61088b6135c9565b600254604051632474521560e21b8152600080516020614bd28339815191529161010090046001600160a01b0316906391d14854906123d490849033906004016147f0565b602060405180830381865afa1580156123f1573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906124159190614807565b6124315760405162461bcd60e51b8152600401610879906149ed565b61243e8260016018613606565b6124a95760405162461bcd60e51b815260206004820152603660248201527f455243313135354175746f47726170684d696e7465723a20486f757273206d756044820152751cdd0818994818995d1dd9595b880c48185b99080c8d60521b6064820152608401610879565b506007805460ff191660ff92909216919091179055565b6000610742838361362c565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d148549061252290859033906004016147f0565b602060405180830381865afa15801561253f573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906125639190614807565b806125e05750600254604051632474521560e21b81526101009091046001600160a01b0316906391d148549061259f90849033906004016147f0565b602060405180830381865afa1580156125bc573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906125e09190614807565b6125fc5760405162461bcd60e51b815260040161087990614829565b612604613650565b61088b83613698565b600254604051632474521560e21b8152600080516020614bf283398151915291600080516020614bd2833981519152916101009091046001600160a01b0316906391d148549061266390859033906004016147f0565b602060405180830381865afa158015612680573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906126a49190614807565b806127215750600254604051632474521560e21b81526101009091046001600160a01b0316906391d14854906126e090849033906004016147f0565b602060405180830381865afa1580156126fd573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906127219190614807565b61273d5760405162461bcd60e51b815260040161087990614829565b610d8084846136fb565b60016000600260019054906101000a90046001600160a01b03166001600160a01b031663f83d08ba6040518163ffffffff1660e01b8152600401602060405180830381865afa15801561279e573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906127c2919061484f565b604051639f3d4e6960e01b815260ff841660048201529091506001600160a01b03821690639f3d4e6990602401600060405180830381600087803b15801561280957600080fd5b505af115801561281d573d6000803e3d6000fd5b50505050612829612c66565b8561283381611be5565b61284f5760405162461bcd60e51b81526004016108799061486c565b60008060006128608a8a8a8a612cae565b92509250925061287088826134a0565b60055461288c906001600160a01b038a81169133911684613540565b60405163d81d0a1560e01b81526001600160a01b038b169063d81d0a15906128bc908c9087908790600401614904565b600060405180830381600087803b1580156128d657600080fd5b505af11580156128ea573d6000803e3d6000fd5b50505050886001600160a01b03168a6001600160a01b03167f0608e8249e0c6db2633b525da433eaf0569d763142ce2c311627af1184b4c99a8585604051612933929190614944565b60405180910390a350505050806001600160a01b031663094feeb460018461295b9190614988565b6040516001600160e01b031960e084901b16815260ff9091166004820152602401600060405180830381600087803b15801561299657600080fd5b505af11580156129aa573d6000803e3d6000fd5b50505050505050505050565b600454600090819063ffffffff166129cd42612a39565b6129d79190614ac9565b60035463ffffffff919091169150612a33906129fd9083906001600160801b0316614ae6565b600454612a1b919064010000000090046001600160e01b0316614afd565b600354600160801b90046001600160801b0316612b20565b91505090565b600063ffffffff821115612a9e5760405162461bcd60e51b815260206004820152602660248201527f53616665436173743a2076616c756520646f65736e27742066697420696e203360448201526532206269747360d01b6064820152608401610879565b5090565b6000610742836001600160a01b03841661373d565b60006001600160e01b03821115612a9e5760405162461bcd60e51b815260206004820152602760248201527f53616665436173743a2076616c756520646f65736e27742066697420696e20326044820152663234206269747360c81b6064820152608401610879565b6000818310612b2f5781610742565b5090919050565b612b3e613650565b600380546001600160801b03838116600160801b908102828416179093556040519290910416907f52d0e582769dcd1e242b38b9a795ef4699f2eca0f23b1d8f94368efb27bcd5ff90612ba690839085909182526001600160801b0316602082015260400190565b60405180910390a15050565b612bbd60008261378c565b612c2f5760405162461bcd60e51b815260206004820152603b60248201527f57686974656c6973746564416464726573733a204661696c656420746f20726560448201527f6d6f766520616464726573732066726f6d2077686974656c69737400000000006064820152608401610879565b6040516001600160a01b038216907f45dcd9ab8c61f0629f2904906111e617d542ed4af59ecb4af2586823382a408c90600090a250565b60025460ff1615612cac5760405162461bcd60e51b815260206004820152601060248201526f14185d5cd8589b194e881c185d5cd95960821b6044820152606401610879565b565b60608060008084516001600160401b03811115612ccd57612ccd613f83565b604051908082528060200260200182016040528015612cf6578160200160208202803683370190505b509050600085516001600160401b03811115612d1457612d14613f83565b604051908082528060200260200182016040528015612d3d578160200160208202803683370190505b5090506000612da560405180610100016040528060006001600160a01b0316815260200160008152602001600081526020016000815260200160006001600160a01b0316815260200160006001600160a01b0316815260200160008152602001600081525090565b6040805160c08101825260008082526020820181905260609282018390529181018290526080810182905260a08101829052905b895181101561302e57898181518110612df457612df4614a24565b602002602001015160000151868281518110612e1257612e12614a24565b602002602001018181525050898181518110612e3057612e30614a24565b602002602001015160200151858281518110612e4e57612e4e614a24565b6020026020010181815250506040518061010001604052808d6001600160a01b031681526020018b8381518110612e8757612e87614a24565b60200260200101516000015181526020018b8381518110612eaa57612eaa614a24565b60200260200101516020015181526020018b8381518110612ecd57612ecd614a24565b60200260200101516060015181526020018e6001600160a01b031681526020018c6001600160a01b031681526020018b8381518110612f0e57612f0e614a24565b602002602001015160a0015181526020018b8381518110612f3157612f31614a24565b602002602001015160c0015181525092506040518060c001604052808b8381518110612f5f57612f5f614a24565b6020026020010151604001518152602001612f7985610656565b81526020018b8381518110612f9057612f90614a24565b60200260200101516080015181526020018b8381518110612fb357612fb3614a24565b60200260200101516020015181526020018e6001600160a01b031681526020018b8381518110612fe557612fe5614a24565b602002602001015160c001518152509150612fff82613084565b89818151811061301157613011614a24565b602002602001015160a00151840193508080600101915050612dd9565b50939b929a50909850909650505050505050565b60005b8181101561088b5761307c83838381811061306257613062614a24565b9050602002016020810190613077919061412e565b6133f3565b600101613045565b6130918160a001516137a1565b6130f55760405162461bcd60e51b815260206004820152602f60248201527f455243313135354175746f47726170684d696e7465723a20457870697279207460448201526e1bdad95b881a5cc8195e1c1a5c9959608a1b6064820152608401610879565b60208101518151146131575760405162461bcd60e51b815260206004820152602560248201527f455243313135354175746f47726170684d696e7465723a2048617368206d69736044820152640dac2e8c6d60db1b6064820152608401610879565b805160009081526006602052604090205460ff16156131c45760405162461bcd60e51b8152602060048201526024808201527f455243313135354175746f47726170684d696e7465723a2048617368206578706044820152631a5c995960e21b6064820152608401610879565b600260019054906101000a90046001600160a01b03166001600160a01b03166391d148547f6866fa9a0b49cd2a1577ed68dd946395d8f3aa85a2066a650b5b63dc2e433f0461321b846000015185604001516124c0565b6040518363ffffffff1660e01b81526004016132389291906147f0565b602060405180830381865afa158015613255573d6000803e3d6000fd5b505050506040513d601f19601f820116820180604052508101906132799190614807565b6132e05760405162461bcd60e51b815260206004820152603260248201527f455243313135354175746f47726170684d696e7465723a204d697373696e67206044820152714d494e5445525f4e4f5441525920526f6c6560701b6064820152608401610879565b80516000908152600660205260409020805460ff191660011790556060810151613309906137e4565b50565b6000811161332c5760405162461bcd60e51b815260040161087990614b10565b8034146133095760405162461bcd60e51b815260206004820152603f60248201527f455243313135354175746f47726170684d696e7465723a205061796d656e742060448201527f616d6f756e7420646f6573206e6f74206d61746368206d73672e76616c7565006064820152608401610879565b6133a9613937565b6002805460ff191690557f5db9ee0a495bf2e6ff9c91a7834c1ba4fdd244a5e8aa4e537bd38aeae4b073aa335b6040516001600160a01b03909116815260200160405180910390a1565b6133fe600082612aa2565b6134695760405162461bcd60e51b815260206004820152603660248201527f57686974656c6973746564416464726573733a204661696c656420746f20616460448201527519081859191c995cdcc81d1bc81dda1a5d195b1a5cdd60521b6064820152608401610879565b6040516001600160a01b038216907f7e82e36808082f76da270c3c5e72976b35dd02ffdd5e28dc06990b08c959f7c090600090a250565b6001600160a01b03821661351c5760405162461bcd60e51b815260206004820152603b60248201527f455243313135354175746f47726170684d696e7465723a207061796d656e745460448201527f6f6b656e206d757374206e6f74206265206164647265737328302900000000006064820152608401610879565b6000811161353c5760405162461bcd60e51b815260040161087990614b10565b5050565b604080516001600160a01b0385811660248301528416604482015260648082018490528251808303909101815260849091019091526020810180516001600160e01b03166323b872dd60e01b179052610d80908590613980565b6001600160a01b03811660009081526001830160205260408120541515610742565b6060600061074283613a52565b6135d1612c66565b6002805460ff191660011790557f62e78cea01bee320cd4e420270b5ea74000d11b0c9f74754ebdbfc544b05a2586133d63390565b60008260ff168460ff161015801561362457508160ff168460ff1611155b949350505050565b600080600061363b8585613aae565b9150915061364881613af3565b509392505050565b600061366261365d6129b6565b612ab7565b9050600061366f42612a39565b63ffffffff166001600160e01b039092166401000000000263ffffffff19169190911760045550565b600380546001600160801b038381166fffffffffffffffffffffffffffffffff1983168117909355604080519190921680825260208201939093527f731084f9b029dd62a76ef1a3a3b66092329f601c3b6c509a6d7dd15fa47a87219101612ba6565b60005b8181101561088b5761373583838381811061371b5761371b614a24565b9050602002016020810190613730919061412e565b612bb2565b6001016136fe565b600081815260018301602052604081205461378457508154600181810184556000848152602080822090930184905584548482528286019093526040902091909155611bf1565b506000611bf1565b6000610742836001600160a01b038416613c3d565b60075460009081906137b89060ff16610e10614ae6565b905060006137c68442614b6d565b9050818110156137da575060019392505050565b5060009392505050565b60006137ee6129b6565b90508060000361384a5760405162461bcd60e51b815260206004820152602160248201527f526174654c696d697465643a206e6f2072617465206c696d69742062756666656044820152603960f91b6064820152608401610879565b8082111561389a5760405162461bcd60e51b815260206004820152601b60248201527f526174654c696d697465643a2072617465206c696d69742068697400000000006044820152606401610879565b6138a761365d8383614b6d565b6004806101000a8154816001600160e01b0302191690836001600160e01b031602179055506138d542612a39565b6004805463ffffffff191663ffffffff929092169190911790819055604080518481526401000000009092046001600160e01b031660208301527fc89b99870f6dd9f35bdd8bada9a4e2a6ba2862d2b5be9eaf54f6b8a6987368fe9101612ba6565b60025460ff16612cac5760405162461bcd60e51b815260206004820152601460248201527314185d5cd8589b194e881b9bdd081c185d5cd95960621b6044820152606401610879565b60006139d5826040518060400160405280602081526020017f5361666545524332303a206c6f772d6c6576656c2063616c6c206661696c6564815250856001600160a01b0316613d379092919063ffffffff16565b80519091501561088b57808060200190518101906139f39190614807565b61088b5760405162461bcd60e51b815260206004820152602a60248201527f5361666545524332303a204552433230206f7065726174696f6e20646964206e6044820152691bdd081cdd58d8d9595960b21b6064820152608401610879565b606081600001805480602002602001604051908101604052809291908181526020018280548015613aa257602002820191906000526020600020905b815481526020019060010190808311613a8e575b50505050509050919050565b6000808251604103613ae45760208301516040840151606085015160001a613ad887828585613d46565b94509450505050613aec565b506000905060025b9250929050565b6000816004811115613b0757613b07614b80565b03613b0f5750565b6001816004811115613b2357613b23614b80565b03613b705760405162461bcd60e51b815260206004820152601860248201527f45434453413a20696e76616c6964207369676e617475726500000000000000006044820152606401610879565b6002816004811115613b8457613b84614b80565b03613bd15760405162461bcd60e51b815260206004820152601f60248201527f45434453413a20696e76616c6964207369676e6174757265206c656e677468006044820152606401610879565b6003816004811115613be557613be5614b80565b036133095760405162461bcd60e51b815260206004820152602260248201527f45434453413a20696e76616c6964207369676e6174757265202773272076616c604482015261756560f01b6064820152608401610879565b60008181526001830160205260408120548015613d26576000613c61600183614b6d565b8554909150600090613c7590600190614b6d565b9050818114613cda576000866000018281548110613c9557613c95614a24565b9060005260206000200154905080876000018481548110613cb857613cb8614a24565b6000918252602080832090910192909255918252600188019052604090208390555b8554869080613ceb57613ceb614b96565b600190038181906000526020600020016000905590558560010160008681526020019081526020016000206000905560019350505050611bf1565b6000915050611bf1565b5092915050565b60606136248484600085613e0a565b6000807f7fffffffffffffffffffffffffffffff5d576e7357a4501ddfe92f46681b20a0831115613d7d5750600090506003613e01565b6040805160008082526020820180845289905260ff881692820192909252606081018690526080810185905260019060a0016020604051602081039080840390855afa158015613dd1573d6000803e3d6000fd5b5050604051601f1901519150506001600160a01b038116613dfa57600060019250925050613e01565b9150600090505b94509492505050565b606082471015613e6b5760405162461bcd60e51b815260206004820152602660248201527f416464726573733a20696e73756666696369656e742062616c616e636520666f6044820152651c8818d85b1b60d21b6064820152608401610879565b600080866001600160a01b03168587604051613e879190614bac565b60006040518083038185875af1925050503d8060008114613ec4576040519150601f19603f3d011682016040523d82523d6000602084013e613ec9565b606091505b5091509150613eda87838387613ee5565b979650505050505050565b60608315613f54578251600003613f4d576001600160a01b0385163b613f4d5760405162461bcd60e51b815260206004820152601d60248201527f416464726573733a2063616c6c20746f206e6f6e2d636f6e74726163740000006044820152606401610879565b5081613624565b6136248383815115613f695781518083602001fd5b8060405162461bcd60e51b81526004016108799190614bbe565b634e487b7160e01b600052604160045260246000fd5b60405160e081016001600160401b0381118282101715613fbb57613fbb613f83565b60405290565b60405161012081016001600160401b0381118282101715613fbb57613fbb613f83565b60405161014081016001600160401b0381118282101715613fbb57613fbb613f83565b604051601f8201601f191681016001600160401b038111828210171561402f5761402f613f83565b604052919050565b6001600160a01b038116811461330957600080fd5b803561405781614037565b919050565b600061010080838503121561407057600080fd5b604051908101906001600160401b038211818310171561409257614092613f83565b81604052833591506140a382614037565b8181526020840135602082015260408401356040820152606084013560608201526140d06080850161404c565b60808201526140e160a0850161404c565b60a082015260c084013560c082015260e084013560e0820152809250505092915050565b60006020828403121561411757600080fd5b81356001600160801b038116811461074257600080fd5b60006020828403121561414057600080fd5b813561074281614037565b60006020828403121561415d57600080fd5b5035919050565b600082601f83011261417557600080fd5b81356001600160401b0381111561418e5761418e613f83565b6141a1601f8201601f1916602001614007565b8181528460208386010111156141b657600080fd5b816020850160208301376000918101602001919091529392505050565b600082601f8301126141e457600080fd5b813560206001600160401b038083111561420057614200613f83565b8260051b61420f838201614007565b938452858101830193838101908886111561422957600080fd5b84880192505b858310156142e0578235848111156142475760008081fd5b880160e0818b03601f190181131561425f5760008081fd5b614267613f99565b87830135815260408084013589830152606080850135828401526080915081850135818401525060a080850135898111156142a25760008081fd5b6142b08f8c83890101614164565b838501525060c091508185013581840152508284013581830152508085525050508482019150848301925061422f565b98975050505050505050565b60008060006060848603121561430157600080fd5b833561430c81614037565b9250602084013561431c81614037565b915060408401356001600160401b0381111561433757600080fd5b614343868287016141d3565b9150509250925092565b60008083601f84011261435f57600080fd5b5081356001600160401b0381111561437657600080fd5b6020830191508360208260051b8501011115613aec57600080fd5b600080602083850312156143a457600080fd5b82356001600160401b038111156143ba57600080fd5b6143c68582860161434d565b90969095509350505050565b600080600080600080600080610100898b0312156143ef57600080fd5b88356143fa81614037565b97506020890135965060408901359550606089013594506080890135935060a08901356001600160401b0381111561443157600080fd5b61443d8b828c01614164565b93505060c089013561444e81614037565b8092505060e089013590509295985092959890939650565b60006020828403121561447857600080fd5b81356001600160401b038082111561448f57600080fd5b9083019061012082860312156144a457600080fd5b6144ac613fc1565b6144b58361404c565b81526020830135602082015260408301356040820152606083013560608201526080830135608082015260a0830135828111156144f157600080fd5b6144fd87828601614164565b60a08301525061450f60c0840161404c565b60c082015260e083810135908201526101009283013592810192909252509392505050565b60006020828403121561454657600080fd5b81356001600160401b038082111561455d57600080fd5b90830190610140828603121561457257600080fd5b61457a613fe4565b6145838361404c565b81526020830135602082015260408301356040820152606083013560608201526080830135608082015260a0830135828111156145bf57600080fd5b6145cb87828601614164565b60a0830152506145dd60c0840161404c565b60c08201526145ee60e0840161404c565b60e082015261010083810135908201526101209283013592810192909252509392505050565b6020808252825182820181905260009190848201906040850190845b818110156146555783516001600160a01b031683529284019291840191600101614630565b50909695505050505050565b60005b8381101561467c578181015183820152602001614664565b50506000910152565b6000815180845261469d816020860160208601614661565b601f01601f19169290920160200192915050565b6000602080830181845280855180835260408601915060408160051b870101925083870160005b8281101561470657603f198886030184526146f4858351614685565b945092850192908501906001016146d8565b5092979650505050505050565b60006020828403121561472557600080fd5b813560ff8116811461074257600080fd5b6000806040838503121561474957600080fd5b8235915060208301356001600160401b0381111561476657600080fd5b61477285828601614164565b9150509250929050565b6000806000806080858703121561479257600080fd5b843561479d81614037565b935060208501356147ad81614037565b925060408501356147bd81614037565b915060608501356001600160401b038111156147d857600080fd5b6147e4878288016141d3565b91505092959194509250565b9182526001600160a01b0316602082015260400190565b60006020828403121561481957600080fd5b8151801515811461074257600080fd5b6020808252600c908201526b15539055551213d49256915160a21b604082015260600190565b60006020828403121561486157600080fd5b815161074281614037565b60208082526037908201527f57686974656c6973746564416464726573733a2050726f76696465642061646460408201527f72657373206973206e6f742077686974656c6973746564000000000000000000606082015260800190565b600081518084526020808501945080840160005b838110156148f9578151875295820195908201906001016148dd565b509495945050505050565b6001600160a01b0384168152606060208201819052600090614928908301856148c9565b828103604084015261493a81856148c9565b9695505050505050565b60408152600061495760408301856148c9565b828103602084015261496981856148c9565b95945050505050565b634e487b7160e01b600052601160045260246000fd5b60ff8281168282160390811115611bf157611bf1614972565b6020808252602c908201527f455243313135354175746f47726170684d696e7465723a204661696c6564207460408201526b379039b2b7321022ba3432b960a11b606082015260800190565b60208082526018908201527f436f72655265663a206e6f20726f6c65206f6e20636f72650000000000000000604082015260600190565b634e487b7160e01b600052603260045260246000fd5b60008235605e19833603018112614a5057600080fd5b9190910192915050565b6000808335601e19843603018112614a7157600080fd5b8301803591506001600160401b03821115614a8b57600080fd5b602001915036819003821315613aec57600080fd5b8183823760009101908152919050565b600060018201614ac257614ac2614972565b5060010190565b63ffffffff828116828216039080821115613d3057613d30614972565b8082028115828204841417611bf157611bf1614972565b80820180821115611bf157611bf1614972565b6020808252603c908201527f455243313135354175746f47726170684d696e7465723a207061796d656e744160408201527f6d6f756e74206d7573742062652067726561746572207468616e203000000000606082015260800190565b81810381811115611bf157611bf1614972565b634e487b7160e01b600052602160045260246000fd5b634e487b7160e01b600052603160045260246000fd5b60008251614a50818460208701614661565b602081526000610742602083018461468556fea49807205ce4d355092ef5a8a18f56e8913cf4a201fbe287825b095693c21775e4076684f4d3f5034ae61b1f0eae4b6055db9c260cef3f2f3340433519266de7a2646970667358221220b5e9392364c866db288b2c6d037af765658a5db0cbec145260d41b7f314d42f464736f6c63430008120033";

    public VerifyDeploymentBase() : base(BYTECODE) { }
    public VerifyDeploymentBase(string byteCode) : base(byteCode) { }

}

public partial class GetHashFunction : GetHashFunctionBase { }

[Function("getHash", "bytes32")]
public class GetHashFunctionBase : FunctionMessage
{
    [Parameter("tuple", "input", 1)]
    public virtual HashInputsParams Input { get; set; }
}

public partial class VerifyMessageFunction : VerifyMessageFunctionBase { }

[Function("VerifyMessage", "address")]
public class VerifyMessageFunctionBase : FunctionMessage
{
    [Parameter("bytes32", "_hashedMessage", 1)]
    public virtual byte[] HashedMessage { get; set; }
    [Parameter("uint8", "_v", 2)]
    public virtual byte V { get; set; }
    [Parameter("bytes32", "_r", 3)]
    public virtual byte[] R { get; set; }
    [Parameter("bytes32", "_s", 4)]
    public virtual byte[] S { get; set; }
}

public partial class GetHashOutputDTO : GetHashOutputDTOBase { }

[FunctionOutput]
public class GetHashOutputDTOBase : IFunctionOutputDTO 
{
    [Parameter("bytes32", "", 1)]
    public virtual byte[] ReturnValue1 { get; set; }
}

public partial class VerifyMessageOutputDTO : VerifyMessageOutputDTOBase { }

[FunctionOutput]
public class VerifyMessageOutputDTOBase : IFunctionOutputDTO 
{
    [Parameter("address", "", 1)]
    public virtual string ReturnValue1 { get; set; }
}

public partial class HashInputsParams : HashInputsParamsBase { }

public class HashInputsParamsBase 
{
    [Parameter("address", "recipient", 1)]
    public virtual string Recipient { get; set; }
    [Parameter("uint256", "tokenId", 2)]
    public virtual BigInteger TokenId { get; set; }
    [Parameter("uint256", "units", 3)]
    public virtual BigInteger Units { get; set; }
    [Parameter("uint256", "salt", 4)]
    public virtual BigInteger Salt { get; set; }
    [Parameter("address", "nftContract", 5)]
    public virtual string NftContract { get; set; }
    [Parameter("address", "paymentToken", 6)]
    public virtual string PaymentToken { get; set; }
    [Parameter("uint256", "paymentAmount", 7)]
    public virtual BigInteger PaymentAmount { get; set; }
    [Parameter("uint256", "expiryToken", 8)]
    public virtual BigInteger ExpiryToken { get; set; }
}
// --------------------------
// mintForFree
public partial class MintForFreeFunction : MintForFreeFunctionBase { }

[Function("mintForFree")]
public class MintForFreeFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)] public virtual string Recipient { get; set; }
    [Parameter("uint256", "tokenId", 2)] public virtual BigInteger TokenId { get; set; }
    [Parameter("uint256", "units", 3)] public virtual BigInteger Units { get; set; }
    [Parameter("bytes32", "hash", 4)] public virtual byte[] Hash { get; set; }
    [Parameter("uint256", "salt", 5)] public virtual BigInteger Salt { get; set; }
    [Parameter("bytes", "signature", 6)] public virtual byte[] Signature { get; set; }

    [Parameter("address", "nftContract", 7)]
    public virtual string NftContract { get; set; }

    [Parameter("uint256", "expiryToken", 8)]
    public virtual BigInteger ExpiryToken { get; set; }
}

public partial class ClaimFunction : ClaimFunctionBase { }

[Function("claim")]
public class ClaimFunctionBase : FunctionMessage
{
    [Parameter("address", "recipient", 1)]
    public virtual string Recipient { get; set; }
    [Parameter("uint256", "tokenId", 2)]
    public virtual BigInteger TokenId { get; set; }
    [Parameter("bytes32", "hash", 3)]
    public virtual byte[] Hash { get; set; }
    [Parameter("uint256", "salt", 4)]
    public virtual BigInteger Salt { get; set; }
    [Parameter("bytes", "signature", 5)]
    public virtual byte[] Signature { get; set; }
    [Parameter("uint256", "expiryToken", 6)]
    public virtual BigInteger ExpiryToken { get; set; }
    [Parameter("string", "_tokenURI", 7)]
    public virtual string TokenURI { get; set; }
}