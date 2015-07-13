using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        internal void Add(StoreRecord record, string key = null)
        {
            AddRange(new List<StoreRecord>{record}, key);
        }

        internal void AddRange(List<StoreRecord> records, string key = null)
        {
            if (records == null)
                records = new List<StoreRecord>();

            _records.AddRange(records);
            OnCollectionChanged(records, key);
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

        public event PropertyChangedEventHandler PropertyChanged;

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