using System.ComponentModel;
using System.Runtime.CompilerServices;
using cow.core.Annotations;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class ConnectionInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private long _serverTime;
        private string _peerId;
        private string _serverIp;
        private string _serverKey;
        private string _serverVersion;        

        [JsonProperty("peerID")]
        public string PeerId 
        {
            get { return _peerId; }
            set
            {
                _peerId = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("server_time")]
        public long ServerTime
        {
            get { return _serverTime; }
            set
            {
                _serverTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("server_ip")]
        public string ServerIp
        {
            get { return _serverIp; }
            set
            {
                _serverIp = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("server_key")]
        public string ServerKey
        {
            get { return _serverKey; }
            set
            {
                _serverKey = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("server_version")]
        public string ServerVersion
        {
            get { return _serverVersion; }
            set
            {
                _serverVersion = value;
                OnPropertyChanged();
            }
        }

        internal void Reset()
        {
            PeerId = null;
            ServerIp = null;
            ServerKey = null;
            ServerVersion = null;
            ServerTime = 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
