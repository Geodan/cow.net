using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Config.Default.Stores
{
    public class PeerStore : CowStore
    {
        public PeerStore(string id, SyncType syncType, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true) : 
            base(id, syncType, subStores, saveToLocalDatabase)
        {

        }
    }
}
