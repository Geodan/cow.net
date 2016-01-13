using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cow.Net.Core.Annotations;

namespace Cow.Net.test
{
    public class CowSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static CowSettings _instance;

        public CowSettings()
        {
            LoadCurrentSettings();
        }

        public static CowSettings Instance
        {
            get
            {
                return _instance ?? (_instance = new CowSettings());
            }
        }

        public void LoadCurrentSettings()
        {
            Server = Properties.Settings.Default.server;
            Port = Properties.Settings.Default.port;
            Endpoint = Properties.Settings.Default.endpoint;
            ServerKey = Properties.Settings.Default.serverKey;
            DatabaseLocation = Properties.Settings.Default.databaseLocation;
            IsAlpha = Properties.Settings.Default.isAlpha;
        }

        private string _server;
        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                Properties.Settings.Default.server = value;
                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                Properties.Settings.Default.port = value;
                OnPropertyChanged();
            }
        }

        private string _endpoint;
        public string Endpoint
        {
            get { return _endpoint; }
            set
            {
                _endpoint = value;
                Properties.Settings.Default.endpoint = value;
                OnPropertyChanged();
            }
        }

        private string _serverKey;
        public string ServerKey
        {
            get { return _serverKey; }
            set
            {
                _serverKey = value;
                Properties.Settings.Default.serverKey = value;
                OnPropertyChanged();
            }
        }

        private string _databaseLocation;
        public string DatabaseLocation
        {
            get { return _databaseLocation; }
            set
            {
                _databaseLocation = value;
                Properties.Settings.Default.databaseLocation = value;
                OnPropertyChanged();
            }
        }

        private bool _isAlpha;
        public bool IsAlpha
        {
            get { return _isAlpha; }
            set
            {
                _isAlpha = value;
                Properties.Settings.Default.isAlpha = value;
                OnPropertyChanged();
            }
        } 

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Properties.Settings.Default.Save();

            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
