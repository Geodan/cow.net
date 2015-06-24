using System;
using Cow.Net.Core;

namespace Cow.Net.test
{
    public partial class MainWindow
    {
        private readonly CowClient _client;

        public MainWindow()
        {
            InitializeComponent();            
            _client = new CowClient(new SQLiteStorageProvider.Core.SQLiteStorageProvider("C:/cow.sqlite3"), "wss://websocket.geodan.nl/ontw");
            _client.PropertyChanged += ClientPropertyChanged;
            _client.CowDatabaseError += ClientCowDatabaseError;
            _client.CowConnected += ClientCowConnected;
            _client.CowConnectionError += ClientCowConnectionError;
            _client.Peers.CollectionChanged += PeersCollectionChanged;

           // _client.Connect();
        }

        private void ClientCowConnected(object sender)
        {
            throw new NotImplementedException();
        }

        private void ClientCowConnectionError(object sender, string error)
        {
            
        }

        private void ClientCowDatabaseError(object sender, string error)
        {
            
        }

        private void PeersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        private void ClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine(_client.ConnectionInfo.PeerId);
        }
    }
}
