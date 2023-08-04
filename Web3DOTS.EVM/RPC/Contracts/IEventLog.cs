using Nethereum.RPC.Eth.DTOs;

namespace Web3Dots.RPC.Contracts
{
    public interface IEventLog
    {
        FilterLog Log { get; }
    }
}