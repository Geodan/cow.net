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
        private string _status;
        private bool _dirty;
        private long _created;
        private bool _deleted;
        private long _updated;
        private object _data;
        private Delta[] _deltas;

        [JsonProperty("_id")]
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        
        [JsonProperty("status")]
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("dirty")]
        public bool Dirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("created")]
        public long Created
        {
            get { return _created; }
            set
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
            set
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
        public Delta[] Deltas
        {
            get { return _deltas; }
            set
            {
                _deltas = value;
                OnPropertyChanged();
            }
        }

        internal void AddSubRecordList(string id)
        {            
            if(_subRecords != null && _subRecords.ContainsKey(id))
                return;

            if(_subRecords == null)
                _subRecords = new Dictionary<string, CowRecordCollection>();

            _subRecords.Add(id, new CowRecordCollection());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
