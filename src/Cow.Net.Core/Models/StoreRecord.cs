using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class StoreRecord : INotifyPropertyChanged
    {        
        private Dictionary<string, CowRecordCollection> _subRecords;

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
        private object _data;
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
            set
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
        public object Data
        {
            get { return _data; }
            set
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
        /// <param name="data"></param>
        public void UpdateData(object data)
        {
            if (Deltas == null)
                Deltas = new ObservableCollection<Delta>();

            Deltas.Add(new Delta("", this));
            Data = data;
            Dirty = true;
        }

        internal void AddSubRecordList(string id)
        {            
            if(_subRecords != null && _subRecords.ContainsKey(id))
                return;

            if(_subRecords == null)
                _subRecords = new Dictionary<string, CowRecordCollection>();

            _subRecords.Add(id, new CowRecordCollection());
        }

        public T GetData<T>()
        {
            return Data == null ? default(T) : JsonConvert.DeserializeObject<T>(Data.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
