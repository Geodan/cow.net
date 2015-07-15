using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class StoreRecord : INotifyPropertyChanged
    {        
        private Dictionary<string, CowRecordCollection> _subRecords;

        public event PropertyChangedEventHandler PropertyChanged;
        public event CowEventHandlers.RecordSyncToPeersRequested SyncToPeersRequested;        

        [JsonIgnore]
        public ReadOnlyDictionary<string, CowRecordCollection> SubRecordCollection
        {
            get
            {
                return new ReadOnlyDictionary<string, CowRecordCollection>(_subRecords);
            }
        }
        
        private string _id;
        private string _identifier;        
        private bool _dirty;
        private long _created;
        private bool _deleted;
        private long _updated;
        private bool? _newDeletedState;
        private Dictionary<string, object> _data;
        private Dictionary<string, object> _dataChangedQueue;
        private ObservableCollection<Delta> _deltas;

        [JsonProperty("_id")]
        public string Id
        {
            get { return _id; }
            internal set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string Identifier
        {
            get { return _identifier; }
            internal set
            {
                _identifier = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool HasChanges
        {
            get {
                return (Dirty || _dataChangedQueue != null && _dataChangedQueue.Any()) || (_newDeletedState != null && _newDeletedState != Deleted);
            }
        }

        [JsonProperty("dirty")]
        public bool Dirty
        {
            get { return _dirty; }
            internal set
            {
                _dirty = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("created")]
        public long Created
        {
            get { return _created; }
            internal set
            {
                _created = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("deleted")]
        public bool Deleted
        {
            get { return _deleted; }
            internal set
            {
                _deleted = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("updated")]
        public long Updated
        {
            get { return _updated; }
            internal set
            {
                _updated = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("data")]
        [JsonConverter(typeof(DataConverter))]
        public Dictionary<string, object> Data
        {
            get { return _data ?? (_data = new Dictionary<string, object>()); }
            internal set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("deltas")]
        public ObservableCollection<Delta> Deltas
        {
            get { return _deltas; }
            internal set
            {
                _deltas = value;
                OnPropertyChanged();
            }
        }

        internal void Update(StoreRecord record)
        {
            record.Created = record.Created;
            record.Data = record.Data;
            record.Deleted = record.Deleted;
            record.Deltas = record.Deltas;
            record.Dirty = record.Dirty;
            record.Updated = record.Updated;
        }        

        /// <summary>
        /// Update the data for this record
        /// </summary>
        public void UpdateData(string key, object value)
        {
            if (Deltas == null)
                Deltas = new ObservableCollection<Delta>();

            if(!Data.ContainsKey(key))
                throw new Exception("Property does not exist in record");

            if (_dataChangedQueue == null)
                _dataChangedQueue = new Dictionary<string, object>();

            if (_dataChangedQueue.ContainsKey(key))
                _dataChangedQueue[key] = value;

            _dataChangedQueue.Add(key, value);
        }        

        public void SetDeleted(bool deleted)
        {
            _newDeletedState = deleted;
        }

        /// <summary>
        /// Save Changes and push to other peers
        /// </summary>
        public void Sync()
        {
            if(!Dirty && !HasChanges)
                return;

            //Create delta
            var newDeltaRecord = new StoreRecord();

            if (_dataChangedQueue != null)
            {
                foreach (var o in _dataChangedQueue)
                {
                    if (newDeltaRecord.Data == null)
                        newDeltaRecord.Data = new Dictionary<string, object>();

                    newDeltaRecord.Data.Add(o.Key, Data[o.Key]);
                }
            }

            newDeltaRecord.Created = Created;
            newDeltaRecord.Deleted = Deleted;
            newDeltaRecord.Dirty = Dirty;
            newDeltaRecord.Id = Id;
            newDeltaRecord.Updated = Updated;
            
            Deltas.Add(new Delta("", newDeltaRecord));

            if (_dataChangedQueue != null)
            {
                foreach (var o in _dataChangedQueue)
                {
                    if (Data == null)
                        Data = new Dictionary<string, object>();

                    Data.Add(o.Key, o.Value);
                }
            }

            Deleted = _newDeletedState != null ? _newDeletedState.Value : Deleted;
            Updated = TimeUtils.GetMillisencondsFrom1970();

            ResetChanges();
            Dirty = false;

            OnSyncToPeersRequested(this);
        }

        public void ResetChanges()
        {
            _dataChangedQueue = null;
            _newDeletedState = null;
        }

        internal void AddSubRecordList(string id)
        {            
            if(_subRecords != null && _subRecords.ContainsKey(id))
                return;

            if(_subRecords == null)
                _subRecords = new Dictionary<string, CowRecordCollection>();

            _subRecords.Add(id, new CowRecordCollection());
        }        

        protected virtual void OnSyncToPeersRequested(StoreRecord record)
        {
            var handler = SyncToPeersRequested;
            if (handler != null) handler(this, record);
        }

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
