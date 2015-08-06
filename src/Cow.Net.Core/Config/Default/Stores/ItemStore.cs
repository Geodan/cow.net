using System.Collections.Generic;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config.Default.Stores
{
    public class ItemStore : CowStore
    {
        public ItemStore(string id, SyncType syncType, IStorageProvider storageProvider, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool supportsDeltas = true) : 
            base(id, syncType, storageProvider, subStores, saveToLocalDatabase, supportsDeltas)
        {

        }
    }
}
