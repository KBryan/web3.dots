using Newtonsoft.Json;
using Web3Dots.RPC.Transactions;

namespace Web3Dots.RPC.Blocks
{
    /// <summary>
    ///     Block including transaction objects
    /// </summary>
    public class BlockWithTransactions : Block
    {
        /// <summary>
        ///     Array - Array of transaction objects
        /// </summary>
        [JsonProperty(PropertyName = "transactions")]
        public TransactionResponse[] Transactions { get; set; }
    }
}