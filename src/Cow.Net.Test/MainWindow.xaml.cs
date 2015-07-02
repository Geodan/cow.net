using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Cow.Net.Core;
using Cow.Net.Core.Config;
using Cow.Net.Core.Models;

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

            //CoreSettings.Instance.SynchronizationContext = context;            
            //_client = new CowClient(new SQLiteStorageProvider.Core.SQLiteStorageProvider("C:/cow.sqlite3"), "wss://websocket.geodan.nl/ontw");

            _config = new DefaultConfig("wss://websocket.geodan.nl:443/eagle", new SQLiteStorageProvider.Core.SQLiteStorageProvider("C:/cow.sqlite3"), context);

            _client = new CowClient(_config);
            _client.PropertyChanged += ClientPropertyChanged;
            _client.CowDatabaseError += ClientCowDatabaseError;
            _client.CowConnected += ClientCowConnected;
            _client.CowConnectionError += ClientCowConnectionError;
            _client.Connect();

            PeerView.SetPeers(_config.Peers);
        }

        private void ClientCowConnected(object sender)
        {
            
        }

        private void ClientCowConnectionError(object sender, string error)
        {
            
        }

        private void ClientCowDatabaseError(object sender, string error)
        {
            
        }

        private void ClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine(_client.ConnectionInfo.PeerId);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _config.Peers.Add(new StoreRecord { Id = Guid.NewGuid().ToString() });
        }
    }
}
