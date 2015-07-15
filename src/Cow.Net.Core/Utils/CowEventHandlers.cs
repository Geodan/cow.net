using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class CowEventHandlers
    {
        //Connection
        public delegate void ConnectedHandler(object sender);
        public delegate void ConnectionInfoReceivedHandler(object sender, ConnectionInfo connectionInfo);
        public delegate void ConnectionErrorHandler(object sender, string error);
        public delegate void ConnectionClosedHandler(object sender);

        //Store
        public delegate void DatabaseErrorHandler(object sender, string error);
        public delegate void StoreSyncedHandler(object sender);
        public delegate void StoreSyncRequestedHandler(object sender, string identifier = null);
        public delegate void StoreUpdateRecordRequestedHandler(object sender, StoreRecord record);
        public delegate void StoreMissingRecordsRequestedHandler(object sender, string project, List<StoreRecord> records);

        //Record
        public delegate void RecordSyncToPeersRequested(object sender, StoreRecord records);        

        //Other
        public delegate void CommandReceivedHandler(object sender, CowMessage<Command> commandMessage);

        //Collection
        public delegate void RecordCollectionChanged(
            object sender, List<StoreRecord> newRecords, string key);
    }
}
