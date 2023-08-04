// Interface for signing messages
namespace Web3Dots.RPC.Utils.Interfaces
{
    public interface IMessageSigner
    {
        string SignMessage(string privateKey, string message);
    }
}

