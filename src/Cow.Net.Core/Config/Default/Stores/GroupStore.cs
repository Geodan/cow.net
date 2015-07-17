using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Config.Default.Stores
{
    public class GroupStore : CowStore
    {
        public GroupStore(string id, SyncType syncType, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool createDeltas = true)
            : base(id, syncType, subStores, saveToLocalDatabase, createDeltas)
        {
        }
    }
}
