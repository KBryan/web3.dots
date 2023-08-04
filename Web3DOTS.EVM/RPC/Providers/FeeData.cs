using System.Numerics;

namespace Web3Dots.RPC.Providers
{
    public class FeeData
    {
        public BigInteger MaxFeePerGas { get; set; }
        public BigInteger MaxPriorityFeePerGas { get; set; }
        public BigInteger GasPrice { get; set; }
    }
}