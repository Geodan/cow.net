using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;
using Cow.Net.Core.Utils;

namespace Cow.Net.Core
{
    public class CowStore : CowRecordCollection
    {
        public event CowEventHandlers.RecordCollectionChanged CollectionChanged;
        public event CowEventHandlers.StoreSyncedHandler StoreSynced;
        public event CowEventHandlers.StoreSyncRequestedHandler SyncRequested;
        public event CowEventHandlers.StoreMissingRecordsRequestedHandler SendMissingRecordsRequested;
        public event CowEventHandlers.StoreRequestedRecordsHandler RequestedRecordsRequested;        

        //ToDo: Use either Id or SyncType preferably Id
        public SyncType SyncType { get; private set; }        
        public bool IsSubStore { get; private set; }
        public bool IsLinkedStore { get; private set; }
        public string LinkedStoreIdentifier { get; private set; }
        public CowStore MainStore { get; private set; }
        public List<CowStore> SubStores { get; private set; }
        public bool SaveToLocalDatabase { get; private set; }
        public bool Synced { get; private set; }
        private readonly IStorageProvider _storageProvider;

        private int _subSyncsNeeded;
        private int _subSyncCount;
        private List<SyncObject> _toReceiveRecords;
        private List<string> _toSendRecords; 

        /// <summary>
        /// Create a new CowStore
        /// </summary>
        /// <param name="id">id of the store, relates to database table and cow synctype response</param>
        /// <param name="syncType">Type of store</param>
        /// <param name="storageProvider">Provider for local storage</param>
        /// <param name="subStores">List of substores for this Store i.e. Project substores -> Item, Groups </param>
        /// <param name="saveToLocalDatabase">Default true = create table and save and load from local storage</param>
        /// <param name="supportsDeltas"></param>        
        protected CowStore(string id, SyncType syncType, IStorageProvider storageProvider,
            IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool supportsDeltas = true)
            : base(id, supportsDeltas)
        {
            SaveToLocalDatabase = saveToLocalDatabase;
            SyncType = syncType;
            _storageProvider = storageProvider;

            if (subStores == null) return;
            foreach (var store in subStores)
            {
                AddSubStore(store);
            }
        }

        /// <summary>
        /// Create a linked store this is a cowstore coupled in a record which had references to the 
        /// actual records in a main (sub)store
        /// </summary>
        /// <param name="cowStore"></param>
        /// <param name="linkedStoreIdentifier"></param>
        internal CowStore(CowStore cowStore, string linkedStoreIdentifier)
            : base(cowStore.Id, cowStore.SupportsDeltas)
        {
            IsLinkedStore = true;
            LinkedStoreIdentifier = linkedStoreIdentifier;
            MainStore = cowStore;
            MainStore.StoreSynced += MainStoreStoreSynced;            
        }

        private void MainStoreStoreSynced(object sender, string project)
        {
            if(project == null)
                return;

            if(project.Equals(LinkedStoreIdentifier))
                OnStoreSynced(project);
        }

        /// <summary>
        /// Add a record to memory, Call SyncRecord on the record or SyncRecords on CowStore to save 
        /// to database and sync to the other peers
        /// </summary>
        /// <param name="record"></param>
        public void Add(StoreRecord record)
        {
            AddSubrecordList(record);

            if (IsLinkedStore)
            {
                record.Identifier = LinkedStoreIdentifier;
                MainStore.Add(record);
                return;
            }

            if (string.IsNullOrEmpty(record.Id))
                record.Id = TimeUtils.GetMillisencondsFrom1970().ToString();
            
            var recordInMemory = Records.FirstOrDefault(r => r.Id.Equals(record.Id));
            if (recordInMemory != null)
            {
                throw new Exception("Duplicate record");
            }

            base.Add(record);
        }

        internal void AddStoreRecordLink(StoreRecord record)
        {
            var recordInMemory = Records.FirstOrDefault(r => r.Id.Equals(record.Id));
            if (recordInMemory != null)
            {
                throw new Exception("Duplicate record");
            }

            base.Add(record);
        }

        internal void SyncStore(string identifier = null)
        {
            if (IsLinkedStore)
            {
                MainStore.OnSyncRequested(identifier);                
            }
            else
            {
                OnSyncRequested(identifier);    
            }            
        }

        internal async void HandleSyncInfo(CowMessage<SyncInfoPayload> syncInfo)
        {
            foreach (var s in syncInfo.Payload.SyncInfo.WillSent)
            {
                if(_toReceiveRecords == null)
                    _toReceiveRecords = new List<SyncObject>();
                
                _toReceiveRecords.Add(new SyncObject{ Project =  syncInfo.Payload.Project, RecordId = s});
            }
            
            _toSendRecords = syncInfo.Payload.SyncInfo.ShallReceive;

            if (_toReceiveRecords == null || !_toReceiveRecords.Any())
            {
                if (SubStores != null)
                {
                    StartSubSync();
                }
                else
                {
                    OnStoreSynced(syncInfo.Payload.Project);
                } 
            }
        }

        internal void StartSubSync()
        {
            _subSyncsNeeded = Records.Count * SubStores.Count;

            foreach (var record in Records)
            {
                if (record.SubRecordCollection == null)
                    continue;

                foreach (var store in record.SubRecordCollection)
                {
                    store.Value.SyncStore(record.Id);
                }
            }
        }

        /// <summary>
        /// A new record or a record gets updated by a peer, update in memory and local storage
        /// </summary>
        internal async void HandleRecord(CowMessage<RecordPayload> handleRecord)
        {
            if (handleRecord.Payload.Record == null)
                return;
            
            if (!string.IsNullOrEmpty(handleRecord.Payload.Project))
            {
                handleRecord.Payload.Record.Identifier = handleRecord.Payload.Project;
            }

            var recordFromMemory = Records.FirstOrDefault(r => r.Id.Equals(handleRecord.Payload.Record.Id));
            if (recordFromMemory != null)
            {
                await UpdateRecord(recordFromMemory, handleRecord);
            }
            else
            {
                await AddRecord(handleRecord);
            }
        }

        internal async Task UpdateRecord(StoreRecord recordFromMemory, CowMessage<RecordPayload> updatedRecord)
        {
            recordFromMemory.Update(updatedRecord.Payload.Record);
            if (CoreSettings.Instance.LocalStorageAvailable && SaveToLocalDatabase && _storageProvider != null)
            {
                _storageProvider.UpdateStoreObjects(Id, new List<StoreRecord> {updatedRecord.Payload.Record});
            }
        }

        internal async Task AddRecord(CowMessage<RecordPayload> missingRecord)
        {
            //Add subrecord lists to a record if this store contains any substores
            if (SubStores != null)
            {
                foreach (var subStore in SubStores)
                {
                    missingRecord.Payload.Record.AddSubRecordList(subStore, missingRecord.Payload.Record.Id);
                }
            }

            Add(missingRecord.Payload.Record, missingRecord.Payload.Project);

            if (CoreSettings.Instance.LocalStorageAvailable && SaveToLocalDatabase && _storageProvider != null)
            {
                _storageProvider.AddStoreObjects(Id, new List<StoreRecord> { missingRecord.Payload.Record });
            }

            if (_toReceiveRecords != null && _toReceiveRecords.Any())
            {
                foreach (var receiveRecord in _toReceiveRecords)
                {
                    if (!receiveRecord.RecordId.Equals(missingRecord.Payload.Record.Id))
                        continue;

                    _toReceiveRecords.Remove(receiveRecord);

                    //Check if removed is last to receive record from project id and start subsync or set synced for project
                    if (!string.IsNullOrEmpty(receiveRecord.Project))
                    {
                        var projectRecordsToReceive = _toReceiveRecords.Count(r => r.Project.Equals(receiveRecord.Project)) - 1;

                        if (projectRecordsToReceive == 0)
                        {
                            if (SubStores != null)
                            {
                                StartSubSync();
                            }
                            else
                            {
                                OnStoreSynced(receiveRecord.Project);
                            }
                        }
                    }

                    break;
                }
            }
            
            //All records received start subsync or set store to synced
            if (!Synced && (_toReceiveRecords == null || !_toReceiveRecords.Any()))
            {
                if (SubStores != null)
                {
                    StartSubSync();
                }
                else
                {
                    OnStoreSynced();
                }
            }
        }

        internal void AddSubrecordList(StoreRecord record)
        {
            if (SubStores == null) return;
            foreach (var subStore in SubStores)
            {
                record.AddSubRecordList(subStore, record.Id);
            }
        }

        internal void LoadFromStorage(IStorageProvider storageProvider, TimeSpan maxDataAge)
        {
            if (!CoreSettings.Instance.LocalStorageAvailable || !SaveToLocalDatabase)
                return;

            var records = storageProvider.GetStoreRecords(Id);
            var maxDiff = maxDataAge.TotalMilliseconds;
            var now = TimeUtils.GetMillisencondsFrom1970();

            var toDelete = records.Where(r => now - r.Updated > maxDiff).ToList();
            if (toDelete.Any())
            {
                //filter out records which has Non outdated records
                for(var i = toDelete.Count - 1; i >= 0; i--)
                {
                    var record = toDelete[i];
                    if (!HasNonOutdatedSubRecords(storageProvider, record, (long) maxDataAge.TotalMilliseconds))
                        toDelete.Remove(record);
                }

                storageProvider.RemoveObjects(Id, toDelete);
                records = storageProvider.GetStoreRecords(Id);
            }

            foreach (var storeRecord in records)
            {
                AddSubrecordList(storeRecord);
            }

            AddRange(records);

            if (SubStores != null)
            {
                foreach (var subStore in SubStores)
                {
                    subStore.LoadFromStorage(storageProvider, maxDataAge);
                }
            }
        }

        //ToDo: move to record + should be recursive, every record can have subrecords
        internal bool HasNonOutdatedSubRecords(IStorageProvider storageProvider, StoreRecord storeRecord, long maxDataAge)
        {
            if (storeRecord.SubRecordCollection == null)
                return true;

            foreach (var rc in storeRecord.SubRecordCollection)
            {
                var hasNonOutdatedSubRecods = storageProvider.HasNonOutdatedLinkedStoreRecords(rc.Key, storeRecord.Identifier, maxDataAge);
                if(hasNonOutdatedSubRecods)
                    return true;
            }

            return false;
        }

        internal void HandleWantedRecords(string project, CowMessage<WantedList> wantedRecords)
        {
            var wanted = wantedRecords.Payload.List.Select(s => Records.FirstOrDefault(r => r.Id.Equals(s))).Where(recordFromMemory => recordFromMemory != null).ToList();
            OnRequestedRecordsRequested(project, wanted);
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

        private void SubStoreSynced(object sender, string project)
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
        private void SubStoreCollectionChanged(object sender, List<StoreRecord> newRecords, List<StoreRecord> deletedRecords)
        {
            if(newRecords == null || !newRecords.Any())
                return;

            var store = sender as CowStore;
            foreach (var storeRecord in newRecords)
            {
                var record = Records.FirstOrDefault(i => i.Id.Equals(storeRecord.Identifier));
                if (record == null)
                {
                    throw new Exception("Error Substore synced but found non connectable records on the parrent store");
                }

                var subRecordCollections = record.SubRecordCollection;
                if (subRecordCollections != null && subRecordCollections.ContainsKey(store.Id))
                {
                    var collection = record.SubRecordCollection[store.Id];
                    collection.AddStoreRecordLink(storeRecord);
                }
                else
                {
                    throw new Exception("Record does not contain the right SubRecordList");
                }
            }
        }

        protected override void OnSyncRecordRequested(StoreRecord record)
        {
            if (CoreSettings.Instance.LocalStorageAvailable && SaveToLocalDatabase && _storageProvider != null)
            {
                if (_storageProvider.HasRecord(Id, record.Id))
                {
                    _storageProvider.UpdateStoreObjects(Id, new List<StoreRecord> {record});
                }
                else
                {
                    _storageProvider.AddStoreObjects(Id, new List<StoreRecord> {record});
                }
            }

            base.OnSyncRecordRequested(record);
        }

        protected override void OnCollectionChanged(List<StoreRecord> newrecords, List<StoreRecord> deletedRecords)
        {
            var handler = CollectionChanged;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, newrecords, deletedRecords), null);
            }
            else
            {
                handler(this, newrecords, deletedRecords);
            }

            OnPropertyChanged("Records");
        } 

        protected internal virtual void OnStoreSynced(string project = null)
        {
            Synced = true;

            var handler = StoreSynced;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, project), null);
            }
            else
            {
                handler(this, project);
            }
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

        protected virtual void OnRequestedRecordsRequested(string project, List<StoreRecord> records)
        {
            var handler = RequestedRecordsRequested;
            if (handler != null) handler(this, project, records);
        }
    }
}
