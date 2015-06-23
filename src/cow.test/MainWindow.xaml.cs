using System;
using cow.core;

namespace cow.test
{
    public partial class MainWindow
    {
        private readonly CowClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new CowClient("wss://websocket.geodan.nl/ontw");
            _client.PropertyChanged += client_PropertyChanged;
            _client.Connect();
            //var client = new cow.CowClient("ws://127.0.0.1:8081");
        }

        private void client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine(_client.ConnectionInfo.PeerId);
        }
    }
}
