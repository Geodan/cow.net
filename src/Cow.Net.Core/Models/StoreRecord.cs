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
        private Dictionary<string, CowStore> _subRecords;

        public event PropertyChangedEventHandler PropertyChanged;
        public event CowEventHandlers.RecordSyncToPeersRequested SyncToPeersRequested;        

        [JsonIgnore]
        public ReadOnlyDictionary<string, CowStore> SubRecordCollection
        {
            get
            {
                return _subRecords == null ? null : new ReadOnlyDictionary<string, CowStore>(_subRecords);
            }
        }
        
        private string _id;
        private string _identifier;        
        private bool _dirty;
        private long _created;
        private bool _deleted;
        private long _updated;
        private bool? _newDeletedState;
        private ObservableDictionary<string, object> _data;
        private Dictionary<string, object> _dataChangedQueue;
        private Dictionary<string, object> _dataAddedQueue;
        private ObservableCollection<Delta> _deltas;

        internal StoreRecord()
        {
            
        }

        public StoreRecord(string id = null, ObservableDictionary<string, object> data = null)
        {
            var now = TimeUtils.GetMillisencondsFrom1970();
            if (string.IsNullOrEmpty(id))
                id = now.ToString();

            Data = data;
            Dirty = true;
            Id = id;
            Created = now;
        }

        public StoreRecord(string id, string projectId, long created, bool deleted, long updated, bool dirty, ObservableDictionary<string, object> data, ObservableCollection<Delta> deltas)
        {
            Id = id;
            Identifier = projectId;
            Created = created;
            Deleted = deleted;
            Updated = updated;
            Dirty = dirty;
            Data = data;            
            Deltas = deltas;
        }

        [JsonProperty("_id")]
        public string Id
        {
            get { return _id; }
            internal set
            {
                if (_id == value)
                    return;

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
                if (_identifier == value)
                    return;

                _identifier = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool HasChanges
        {
            get {
                return (_dataChangedQueue != null && _dataChangedQueue.Any()) || (_dataAddedQueue != null && _dataAddedQueue.Any()) || (_newDeletedState != null && _newDeletedState != Deleted);
            }
        }

        [JsonIgnore]
        public bool SupportsDeltas { get; internal set; }

        [JsonProperty("dirty")]
        public bool Dirty
        {
            get { return _dirty; }
            internal set
            {
                if (_dirty == value)
                    return;

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
                if (_created == value)
                    return;

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
                if(_deleted == value)
                    return;

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
                if (_updated == value)
                    return;

                _updated = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("data")]
        [JsonConverter(typeof(DataConverter))]
        public ObservableDictionary<string, object> Data
        {
            get { return _data ?? (Data = new ObservableDictionary<string, object>()); }
            internal set
            {
                if(value == null)
                    value = new ObservableDictionary<string, object>();

                if (_data != null)
                    _data.PropertyChanged -= DataPropertyChanged;

                _data = value;
                _data.PropertyChanged += DataPropertyChanged;

                OnPropertyChanged();
            }
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        [JsonProperty("deltas")]
        public ObservableCollection<Delta> Deltas
        {
            get { return _deltas ?? (_deltas = new ObservableCollection<Delta>()); }
            internal set
            {
                _deltas = value;
                OnPropertyChanged();
            }
        }

        internal void Update(StoreRecord record)
        {
            if (!Created.Equals(record.Created))
                Created = record.Created;

            if (record.Data != null)
            {
                foreach (var key in record.Data.Keys)
                {
                    if ((Data.ContainsKey(key) && (Data[key] != record.Data[key])) ||
                        !Data.ContainsKey(key))
                    {
                        Data[key] = record.Data[key];
                    }
                }
                foreach (var key in Data.Keys)
                {
                    if (!record.Data.ContainsKey(key))
                    {
                        Data.Remove(key);
                    }
                }
            }

            if (Identifier != record.Identifier || (Identifier != null && record.Identifier != null && !Identifier.Equals(record.Identifier)))
                Identifier = record.Identifier;

            if (Deleted != record.Deleted)
                Deleted = record.Deleted;

            if ((Deltas == null && record.Deltas != null && record.Deltas.Any()) || 
               (Deltas != null && record.Deltas != null && Deltas.Count != record.Deltas.Count))
                Deltas = record.Deltas;

            if (!Updated.Equals(record.Updated))
                Updated = record.Updated;
        }

        internal void CreateFirstDelta(string userId)
        {
            if (!SupportsDeltas)
                return;

            if(Deltas != null && Deltas.Any())
                return;

            var delta = new Delta
            {
                TimeStamp = TimeUtils.GetMillisencondsFrom1970(), 
                Deleted = Deleted,
                Data = Data
            };

            if (userId != null)
                delta.UserId = userId;

            if(Deltas == null)
                Deltas = new ObservableCollection<Delta>();

            Deltas.Add(delta);
        }

        /// <summary>
        /// Update the data for this record
        /// </summary>
        public void UpdateData(string key, object value)
        {
            if (Deltas == null)
                Deltas = new ObservableCollection<Delta>();

            if (!Data.ContainsKey(key))
            {
                AddData(key, value);
                return;
            }

            if (_dataChangedQueue == null)
                _dataChangedQueue = new Dictionary<string, object>();

            if (JsonConvert.SerializeObject(Data[key]).Equals(JsonConvert.SerializeObject(value)))
                return;

            if (_dataChangedQueue.ContainsKey(key))
            {
                _dataChangedQueue[key] = value;
                return;
            }

            _dataChangedQueue.Add(key, value);
        }

        public void AddData(string key, object value)
        {
            if (Data.ContainsKey(key))
            {
                UpdateData(key, value);
                return;
            }

            if (_dataAddedQueue == null)
                _dataAddedQueue = new Dictionary<string, object>();

            if (_dataAddedQueue.ContainsKey(key))
            {
                _dataAddedQueue[key] = value;
                return;
            }

            _dataAddedQueue.Add(key, value);
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

            var user = CoreSettings.Instance.CurrentUser == null ? null : CoreSettings.Instance.CurrentUser.Id;
            if (!Dirty && HasChanges && SupportsDeltas)
            {
                //Create delta
                var newDeltaRecord = new Delta();

                if (_dataChangedQueue != null)
                {
                    foreach (var o in _dataChangedQueue)
                    {
                        if (newDeltaRecord.Data == null)
                            newDeltaRecord.Data = new ObservableDictionary<string, object>();

                        newDeltaRecord.Data.Add(o.Key, o.Value);
                    }
                }

                newDeltaRecord.Deleted = Deleted;
                newDeltaRecord.UserId = user;
                newDeltaRecord.TimeStamp = TimeUtils.GetMillisencondsFrom1970();

                Deltas.Add(newDeltaRecord);
            }

            if (_dataAddedQueue != null)
            {
                foreach (var o in _dataAddedQueue)
                {
                    if (Data == null)
                        Data = new ObservableDictionary<string, object>();

                    Data.Add(o.Key, o.Value);
                }
            }

            if (_dataChangedQueue != null)
            {
                foreach (var o in _dataChangedQueue)
                {
                    if (Data == null)
                        Data = new ObservableDictionary<string, object>();

                    Data[o.Key] = o.Value;                   
                }
            }

            if (Dirty && SupportsDeltas)
            {
                CreateFirstDelta(user);
            }

            Deleted = _newDeletedState != null ? _newDeletedState.Value : Deleted;
            Updated = TimeUtils.GetMillisencondsFrom1970();

            ResetChanges();
            Dirty = false;

            OnSyncToPeersRequested(this);
        }

        internal void ResetChanges()
        {
            _dataAddedQueue = null;
            _dataChangedQueue = null;
            _newDeletedState = null;
        }

        internal void AddSubRecordList(CowStore cowStore, string linkedStoreId)
        {
            if (_subRecords != null && _subRecords.ContainsKey(cowStore.Id))
                return;

            if(_subRecords == null)
                _subRecords = new Dictionary<string, CowStore>();

            _subRecords.Add(cowStore.Id, new CowStore(cowStore, linkedStoreId));
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
