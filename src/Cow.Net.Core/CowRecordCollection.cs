using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Cow.Net.Core.Annotations;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;

namespace Cow.Net.Core
{
    public class CowRecordCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event CowEventHandlers.RecordCollectionChanged CollectionChanged;
        public event CowEventHandlers.RecordSyncToPeersRequested SyncRecordRequested;

        public string Id { get; private set; }
        public bool SupportsDeltas { get; private set; }

        private readonly List<StoreRecord> _records;

        public CowRecordCollection(string id, bool supportsDeltas)
        {
            Id = id;
            SupportsDeltas = supportsDeltas;
            _records = new List<StoreRecord>();
        }        

        public ReadOnlyCollection<StoreRecord> Records
        {
            get { return new ReadOnlyCollection<StoreRecord>(_records); }
        }

        public void SyncRecords()
        {
            var recordsToSync = _records.Where(storeRecord => storeRecord.HasChanges).ToList();
            foreach (var storeRecord in recordsToSync)
            {
                storeRecord.Sync();
            }
        }

        internal void Clear()
        {
            var temp = new StoreRecord[_records.Count];
            _records.CopyTo(temp);
            _records.Clear();
            OnCollectionChanged(new List<StoreRecord>(), temp.ToList());
        }

        internal void Compare(List<StoreRecord> newListRecords, out List<StoreRecord> pushList, out List<StoreRecord> requestList, string projectId)
        {
            pushList = new List<StoreRecord>();
            requestList = new List<StoreRecord>();

            foreach (var sendRecord in newListRecords)
            {
                var foundRecord = Records.FirstOrDefault(record => sendRecord.Id.Equals(record.Id));
                if(foundRecord == null || foundRecord.Updated < sendRecord.Updated)
                    requestList.Add(sendRecord);

                if (foundRecord != null && foundRecord.Updated > sendRecord.Updated)
                {
                    if(string.IsNullOrEmpty(projectId) || foundRecord.Identifier.Equals(projectId))
                        pushList.Add(foundRecord);
                }                    
            }

            pushList.AddRange(from record in Records let found = newListRecords.Any(storeRecord => storeRecord.Id.Equals(record.Id)) where !found select record);
        }

        internal void Add(StoreRecord record, string key = null)
        {
            AddRange(new List<StoreRecord>{record}, key);
        }

        internal void AddRange(List<StoreRecord> records, string key = null)
        {
            if (records == null)
                records = new List<StoreRecord>();

            foreach (var storeRecord in records)
            {
                storeRecord.SupportsDeltas = SupportsDeltas;
                storeRecord.SyncToPeersRequested += StoreRecordSyncToPeersRequested;
            }

            _records.AddRange(records);
            OnCollectionChanged(records, new List<StoreRecord>());
        }

        private void StoreRecordSyncToPeersRequested(object sender, StoreRecord records)
        {
            OnSyncRecordRequested(records);
        }

        protected virtual void OnCollectionChanged(List<StoreRecord> newrecords, List<StoreRecord> removedRecords)
        {
            var handler = CollectionChanged;
            if(handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, newrecords, removedRecords), null);
            }
            else
            {
                handler(this, newrecords, removedRecords);                
            }

            OnPropertyChanged("Records");
        }        

        protected virtual void OnSyncRecordRequested(StoreRecord record)
        {
            var handler = SyncRecordRequested;
            if (handler != null) handler(this, record);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, new PropertyChangedEventArgs(propertyName)), null);
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}