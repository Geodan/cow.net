using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Storage
{
    public interface IStorageProvider
    {
        bool PrepareDatabase();
        List<Peer> GetPeers();        
        void AddPeers(List<Peer> peers);
    }
}
