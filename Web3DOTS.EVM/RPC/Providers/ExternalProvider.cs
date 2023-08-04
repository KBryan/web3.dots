using System.Threading.Tasks;

namespace Web3Dots.RPC.Providers
{
    public interface IExternalProvider
    {
        // public bool IsMetaMask { get; }
        // public string GetHost();
        public string GetPath();

        public Task<T> Send<T>(string method, object[] parameters = null);
    }
}