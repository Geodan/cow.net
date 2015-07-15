using System;
using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using WebSocketSharp;

namespace Cow.Net.Core
{
    public abstract class CowStore : CowRecordCollection
    {
        public event CowEventHandlers.StoreSyncedHandler StoreSynced;
        public event CowEventHandlers.StoreSyncRequestedHandler SyncRequested;
        public event CowEventHandlers.StoreMissingRecordsRequestedHandler SendMissingRecordsRequested;        

        public string Id { get; private set; }
        public SyncType SyncType { get; private set; }        
        public bool IsSubStore { get; private set; }
        public List<CowStore> SubStores { get; private set; }
        public bool SaveToLocalDatabase { get; private set; }

        private int _subSyncsNeeded;
        private int _subSyncCount;

        /// <summary>
        /// Create a new CowStore
        /// </summary>
        /// <param name="id">id of the store, relates to database table and cow synctype response</param>
        /// <param name="syncType">Type of store</param>
        /// <param name="subStores">List of substores for this Store i.e. Project substores -> Item, Groups </param>
        /// <param name="saveToLocalDatabase">Default true = create table and save and laod from local storage</param>        
        public CowStore(string id, SyncType syncType, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true)
        {
            Id = id;
            SaveToLocalDatabase = saveToLocalDatabase;
            SyncType = syncType;

            if (subStores == null) return;
            foreach (var store in subStores)
            {                
                AddSubStore(store);
            }
        }

        public void Add(StoreRecord record)
        {
            if (string.IsNullOrEmpty(record.Id))
                record.Id = DateTime.Now.Ticks.ToString();
            
            var recordInMemory = Records.FirstOrDefault(r => r.Id.Equals(record.Id));
            if (recordInMemory != null)
            {
                throw new Exception("Duplicate record");
            }

            //Add to memory
            base.Add(record);
                        
            //Add to database
            if (SaveToLocalDatabase)
            {
                
            }
        }

        internal void SyncStore(string identifier = null)
        {
            OnSyncRequested(identifier); 
        }

        /// <summary>
        /// Missing records after syncing with peers
        /// </summary>
        internal void HandleMissingRecords(WebSocket socket, ConnectionInfo connectionInfo, CowMessage<NewList> missingRecords)
        {
            //Add subrecord lists to a record if this store contains any substores
            if (SubStores != null)
            {
                foreach (var record in missingRecords.Payload.List)
                {
                    foreach (var subStore in SubStores)
                    {
                        record.AddSubRecordList(subStore.Id);   
                    }                    
                }
            }

            AddRange(missingRecords.Payload.List, missingRecords.Payload.Project);            

            if (SubStores != null)
            {
                _subSyncsNeeded = missingRecords.Payload.List.Count * SubStores.Count;

                foreach (var record in missingRecords.Payload.List)
                {
                    foreach (var subStore in SubStores)
                    {
                        subStore.SyncStore(record.Id);
                    }
                }
            }
            else
            {
                OnStoreSynced();
            }
        }

        /// <summary>
        /// A record got updated by a peer, update in memory and local storage
        /// </summary>        
        internal void HandleUpdatedRecord(CowMessage<UpdatedRecord> updatedRecord)
        {
            var recordFromMemory = Records.FirstOrDefault(r => r.Id.Equals(updatedRecord.Payload.Record.Id));
            if (recordFromMemory != null)
            {
                recordFromMemory.Update(updatedRecord.Payload.Record);
            }
            else
            {
                Add(updatedRecord.Payload.Record, updatedRecord.Payload.Project);
            }
        }

        internal void HandleWantedRecords(string project, CowMessage<WantedList> wantedRecords)
        {
            var wanted = wantedRecords.Payload.List.Select(s => Records.FirstOrDefault(r => r.Id.Equals(s))).Where(recordFromMemory => recordFromMemory != null).ToList();
            OnSendMissingRecordsRequested(project, wanted);
        }

        internal void LoadFromLocal()
        {
            //ToDo: Load from local database
        }

        private void AddSubStore(CowStore store)
        {
            if (SubStores == null)
            {
                SubStores = new List<CowStore>();
            }

            store.IsSubStore = true;            
            store.CollectionChanged += SubStoreCollectionChanged;
            store.StoreSynced += SubStoreSynced;

            SubStores.Add(store);
        }

        private void SubStoreSynced(object sender)
        {
            _subSyncCount++;            
            if (_subSyncCount != _subSyncsNeeded) 
                return;

            _subSyncCount = 0;
            OnStoreSynced();
        }

        /// <summary>
        /// A collection in a substore changed, set a reference to te subrecord in memory to the right record
        /// </summary>
        private void SubStoreCollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            var store = sender as CowStore;
            if(store == null || string.IsNullOrEmpty(key))
                throw new Exception();

            var record = Records.FirstOrDefault(i => i.Id.Equals(key));
            if (record == null)
            {
                throw new Exception("Error Substore synced but found non connectable records on the parrent store");
            }

            foreach (var r in newRecords.Where(r => string.IsNullOrEmpty(r.Identifier)))
            {
                r.Identifier = record.Id;
            }

            var subRecordCollections = record.SubRecordCollection;

            if (subRecordCollections != null && subRecordCollections.ContainsKey(store.Id))
            {
                var collection = record.SubRecordCollection[store.Id];
                collection.AddRange(newRecords);
            }
            else
            {
                throw new Exception("Record does not contain the right SubRecordList");
            }
        }

        protected virtual void OnStoreSynced()
        {
            var handler = StoreSynced;
            if (handler != null) handler(this);
        }

        protected virtual void OnSyncRequested(string identifier = null)
        {
            var handler = SyncRequested;
            if (handler != null) handler(this, identifier);
        }

        protected virtual void OnSendMissingRecordsRequested(string project, List<StoreRecord> records)
        {
            var handler = SendMissingRecordsRequested;
            if (handler != null) handler(this, project, records);
        }
    }
}
