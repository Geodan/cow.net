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
        public event CowEventHandlers.RecordSyncToPeersRequested SyncRecordsRequested;

        private readonly List<StoreRecord> _records;

        public CowRecordCollection()
        {
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

        internal void Compare(List<StoreRecord> newListRecords, out List<StoreRecord> pushList, out List<StoreRecord> requestList)
        {
            pushList = new List<StoreRecord>();
            requestList = new List<StoreRecord>();

            foreach (var sendRecord in newListRecords)
            {
                StoreRecord foundRecord = null;
                if (Records.Any(record => sendRecord.Id.Equals(record.Id)))
                {
                    foundRecord = sendRecord;
                }

                if(foundRecord == null || foundRecord.Updated < sendRecord.Updated)
                    requestList.Add(sendRecord);

                if(foundRecord != null && foundRecord.Updated > sendRecord.Updated)
                    pushList.Add(foundRecord);
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
                storeRecord.SyncToPeersRequested += StoreRecordSyncToPeersRequested;
            }

            _records.AddRange(records);
            OnCollectionChanged(records, key);
        }

        private void StoreRecordSyncToPeersRequested(object sender, StoreRecord records)
        {
            OnSyncRecordsRequested(records);
        }

        protected virtual void OnCollectionChanged(List<StoreRecord> newrecords, string key)
        {
            var handler = CollectionChanged;
            if(handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {            
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, newrecords, key),  null);
            }
            else
            {
                handler(this, newrecords, key);                
            }

            OnPropertyChanged("Records");
        }        

        protected virtual void OnSyncRecordsRequested(StoreRecord record)
        {
            var handler = SyncRecordsRequested;
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