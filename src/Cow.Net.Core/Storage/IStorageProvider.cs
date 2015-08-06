using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Storage
{
    public interface IStorageProvider
    {
        bool PrepareDatabase(List<string> stores);
        bool HasRecord(string storeId, string recordId);
        bool HasNonOutdatedLinkedStoreRecords(string storeId, string identifier, long outdatedTime);
        List<StoreRecord> GetStoreRecords(string storeId);        
        void AddStoreObjects(string storeId, List<StoreRecord> records);
        void UpdateStoreObjects(string storeId, List<StoreRecord> records);
        void RemoveObjects(string storeId, List<StoreRecord> records);
    }
}