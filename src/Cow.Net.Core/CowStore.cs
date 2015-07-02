using System;
using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using WebSocketSharp;

namespace Cow.Net.Core
{
    public class CowStore : CowRecordCollection
    {
        public event CowEventHandlers.StoreSyncedHandler StoreSynced;
        
        public string Id { get; private set; }
        public bool IsSubStore { get; private set; }
        public List<CowStore> SubStores { get; private set; }
        public bool SaveToLocalDatabase { get; private set; }

        private int _subSyncsNeeded;
        private int _subSyncCount;

        /// <summary>
        /// Create a new CowStore
        /// </summary>
        /// <param name="id">id of the store, relates to database table and cow synctype response</param>
        /// <param name="subStores">List of substores for this Store i.e. Project substores -> Item, Groups </param>
        /// <param name="saveToLocalDatabase">Default true = create table and save and laod from local storage</param>
        public CowStore(string id, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true)
        {
            Id = id;
            SaveToLocalDatabase = saveToLocalDatabase;

            if (subStores == null) return;
            foreach (var store in subStores)
            {
                AddSubStore(store);
            }
        }

        public void Sync(WebSocket socket, ConnectionInfo connectionInfo, string identifier = null)
        {
            var jsonString = JsonSerialize(CowMessageFactory.CreateSyncMessage(connectionInfo, Id, Records, identifier));
            socket.Send(jsonString);   
        }

        private string JsonSerialize(CowMessage<Dictionary<string, object>> message)
        {
            return JsonConvert.SerializeObject(message, Formatting.None, CoreSettings.Instance.SerializerSettings);
        }

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
                        subStore.Sync(socket, connectionInfo, record.Id);
                    }
                }
            }
            else
            {
                OnStoreSynced();
            }
        }

        public void LoadFromLocal()
        {
            
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
        private void SubStoreCollectionChanged(object sender, List<StoreRecord> newRecords, List<StoreRecord> deletedRecords, List<StoreRecord> unchangedRecords, string key)
        {
            var store = sender as CowStore;
            if(store == null || string.IsNullOrEmpty(key))
                throw new Exception();

            var record = Records.FirstOrDefault(i => i.Id.Equals(key));
            if (record == null)
            {
                throw new Exception("Error Substore synced but found non connectable records on the parrent store");
            }

            var subRecordCollections = record.SubRecordCollection;

            if (subRecordCollections != null && subRecordCollections.ContainsKey(store.Id))
            {
                var collection = record.SubRecordCollection[store.Id];

                collection.AddRange(newRecords);
                collection.RemoveRange(deletedRecords);
            }
            else
            {
                throw new Exception("Record does not contain the right SubRecordList");
            }
        }

        public void UpdateItem(StoreRecord item)
        {
            
        }

        public void RemoveItems(IList<StoreRecord> items)
        {

        }

        protected virtual void OnStoreSynced()
        {
            var handler = StoreSynced;
            if (handler != null) handler(this);
        }
    
        //protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    using (BlockReentrancy())
        //    {
        //        if (_suspendCollectionChangeNotification) return;
        //        var eventHandler = CollectionChanged;
        //        if (eventHandler == null)
        //        {
        //            return;
        //        }

        //        var delegates = eventHandler.GetInvocationList();

        //        foreach (var @delegate in delegates)
        //        {
        //            var handler = (NotifyCollectionChangedEventHandler) @delegate;

        //            if (CoreSettings.Instance.SynchronizationContext != null)
        //            {
        //                CoreSettings.Instance.SynchronizationContext.Post(delegate { handler(this, e); }, null);
        //            }
        //            else
        //            {
        //                handler(this, e);
        //            }
        //        }
        //    }
        //}
    }
}
