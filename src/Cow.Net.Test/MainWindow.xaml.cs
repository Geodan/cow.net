using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Cow.Net.Core;
using Cow.Net.Core.Config.Default;

namespace Cow.Net.test
{
    public partial class MainWindow
    {
        private readonly CowClient _client;
        private readonly DefaultConfig _config;

        public MainWindow()
        {
            InitializeComponent();
            
            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);
            
            _config = new DefaultConfig("wss://websocket.geodan.nl:443/eagle", "eagle", true, new SQLiteStorageProvider.Core.SQLiteStorageProvider("C:/cow.sqlite3"), context);            
            _client = new CowClient(_config);
            _client.PropertyChanged += ClientPropertyChanged;
            _client.CowDatabaseError += ClientCowDatabaseError;
            _client.CowSocketConnected += ClientCowSocketConnected;
            _client.CowSocketConnectionError += ClientCowSocketConnectionError;
            _client.CowConnectionInfoReceived += ClientCowConnectionInfoReceived;
            _client.Connect();
            
            PeerView.SetPeers(_config.Peers);
            SocketServerView.SetSocketServerStore(_config.SocketServers);
            ProjectView.SetProjects(_config.Projects);
            UserView.SetUserStore(_config.Users);
        }

        private void ClientCowConnectionInfoReceived(object sender, Core.Models.ConnectionInfo connectionInfo)
        {
            ConnectionInfoView.ConnectionInfo = _client.ConnectionInfo;
        }

        private void ClientCowSocketConnected(object sender)
        {
            
        }

        private void ClientCowSocketConnectionError(object sender, string error)
        {
            
        }

        private void ClientCowDatabaseError(object sender, string error)
        {
            
        }

        private void ClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
                    
        }
    }
}
