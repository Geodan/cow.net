using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Storage
{
    public interface IStorageProvider
    {
        bool PrepareDatabase(List<string> stores);
        List<StoreRecord> GetStoreObjects();        
        void AddStoreObject(List<StoreRecord> peers);
        void UpdateStoreObject(List<StoreRecord> peers);
    }
}
