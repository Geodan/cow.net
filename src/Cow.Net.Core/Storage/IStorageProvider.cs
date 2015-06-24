using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Storage
{
    public interface IStorageProvider
    {
        bool PrepareDatabase();
        List<StoreObject> GetPeers();        
        void AddPeers(List<StoreObject> peers);
    }
}
