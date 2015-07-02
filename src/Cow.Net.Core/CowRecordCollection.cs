using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cow.Net.Core.Annotations;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;

namespace Cow.Net.Core
{
    public class CowRecordCollection : INotifyPropertyChanged
    {
        public event CowEventHandlers.RecordCollectionChanged CollectionChanged;    

        private readonly List<StoreRecord> _records;

        public CowRecordCollection()
        {
            _records = new List<StoreRecord>();
        }

        public ReadOnlyCollection<StoreRecord> Records
        {
            get { return new ReadOnlyCollection<StoreRecord>(_records); }
        }

        public void Add(StoreRecord record, string key = null)
        {
            AddRange(new List<StoreRecord>{record}, key);
        }

        public void AddRange(List<StoreRecord> records, string key = null)
        {
            var temp = _records;
            if (records == null)
                records = new List<StoreRecord>();

            _records.AddRange(records);
            OnCollectionChanged(records, new List<StoreRecord>(), temp, key);
        }

        public void Remove(StoreRecord record, string key = null)
        {
            RemoveRange(new List<StoreRecord> { record });
        }

        public void RemoveRange(List<StoreRecord> records, string key = null)
        {
            if (records == null)
                records = new List<StoreRecord>();

            foreach (var storeRecord in records)
            {
                _records.Remove(storeRecord);
            }

            OnCollectionChanged(new List<StoreRecord>(), records, _records, key);
        }

        protected virtual void OnCollectionChanged(List<StoreRecord> newrecords, List<StoreRecord> deletedrecords, List<StoreRecord> unchangedrecords, string key)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(this, newrecords, deletedrecords, unchangedrecords, key);
            OnPropertyChanged("Records");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
