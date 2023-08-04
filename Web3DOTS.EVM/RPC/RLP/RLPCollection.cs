using System.Collections.Generic;

namespace Web3Dots.RPC.RLP
{
    public class RLPCollection : List<IRLPElement>, IRLPElement
    {
        public byte[] RLPData { get; set; }
    }
}