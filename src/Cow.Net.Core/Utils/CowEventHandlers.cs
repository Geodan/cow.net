using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class CowEventHandlers
    {
        //Connection
        public delegate void ConnectedHandler(object sender);
        public delegate void ConnectionErrorHandler(object sender, string error);
        public delegate void ConnectionClosedHandler(object sender);

        //Store
        public delegate void DatabaseErrorHandler(object sender, string error);
        public delegate void StoreSyncedHandler(object sender);

        //Collection
        public delegate void RecordCollectionChanged(
            object sender, List<StoreRecord> newRecords, List<StoreRecord> deletedRecords,
            List<StoreRecord> unchangedRecords, string key);
    }
}
