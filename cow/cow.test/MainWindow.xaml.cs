using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
