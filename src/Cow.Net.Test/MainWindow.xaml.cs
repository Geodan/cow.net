using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cow.Net.Core;
using Cow.Net.Core.Config.Default;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using Cow.Net.Socket.Net45;
using Cow.Net.test.UI.Controls;

namespace Cow.Net.test
{
    public partial class MainWindow
    {
        private CowClient _client;
        private DefaultConfig _config;
        private readonly PerformanceMonitor _performance;

        public MainWindow()
        {
            InitializeComponent();

            _performance = new PerformanceMonitor();
            _performance.PropertyChanged += PerformancePropertyChanged;
        }

        private void PerformancePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TxtMemoryUsage.Text = string.Format("{0} MB", _performance.Memory);
        }

        private void ClientCowConnectionInfoReceived(object sender, ConnectionInfo connectionInfo)
        {
            ConnectionInfoView.ConnectionInfo = _client.ConnectionInfo;
        }

        private void ClientCowSocketConnected(object sender)
        {
            BtnConnection.Content = new TextBlock { Text = "disconnect" };
            ElConnected.Visibility = Visibility.Visible;
            ElDisconnected.Visibility = Visibility.Collapsed;
        }

        private void ClientCowSocketDisconnected(object sender)
        {
            BtnConnection.Content = new TextBlock {Text = "connect"};
            ElConnected.Visibility = Visibility.Collapsed;
            ElDisconnected.Visibility = Visibility.Visible;
        }

        private void ClientCowSocketConnectionError(object sender, string error)
        {
            MessageBox.Show(string.Format("Error connection to socket: {0}", error));
        }

        private void ClientCowDatabaseError(object sender, string error)
        {
            MessageBox.Show(string.Format("Database error: {0} \nclient will continue without local storage", error));
        }

        private void ClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
                    
        }

        private void Start()
        {
            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);
            _config = new DefaultConfig(CowSettings.Instance.ServerKey, CowSettings.Instance.IsAlpha, new SQLiteStorageProvider.Core.SQLiteStorageProvider(CowSettings.Instance.DatabaseLocation), new Net45WebSocketConnectionProvider(),  context);     

            _client = new CowClient(_config);            
            _client.PropertyChanged += ClientPropertyChanged;
            _client.CowDatabaseError += ClientCowDatabaseError;
            _client.CowSocketConnected += ClientCowSocketConnected;
            _client.CowSocketDisconnected += ClientCowSocketDisconnected;
            _client.CowSocketConnectionError += ClientCowSocketConnectionError;
            _client.CowConnectionInfoReceived += ClientCowConnectionInfoReceived;
            _client.CowCommandReceived += ClientCowCommandReceived;
            _client.StartClient();

            StoreList.Children.Clear();
            foreach (var mainStore in _config.CowStoreManager.MainStores)
            {
                StoreList.Children.Add(new StoreView(mainStore){Margin = new Thickness(8,8,8,8)});
            }

            StartScreen.Visibility = Visibility.Collapsed;
            ControlGrid.Visibility = Visibility.Visible;
            TopGrid.Visibility = Visibility.Visible;            
            CowContent.Visibility = Visibility.Visible;

            _performance.Start();
        }

        private void ClientCowCommandReceived(object sender, CommandPayload commandMessage)
        {
            MessageBox.Show(string.Format("Non implemented command received: {0}", commandMessage.Command));
        }

        private void StartScreenOnStartClicked(object sender)
        {
            Start();
        }

        private void BtnConnectionOnClick(object sender, RoutedEventArgs e)
        {
            if (!_client.Connected)
            {
                _client.Connect(CowSettings.Instance.Server, CowSettings.Instance.Port, new[] { CowSettings.Instance.Endpoint }, "connect");
            }
            else
            {
                ClientCowConnectionInfoReceived(this, new ConnectionInfo());
                _client.Disconnect();
            }
        }

        private void BtnBackOnClick(object sender, RoutedEventArgs e)
        {
            _client.Disconnect();

            StartScreen.Visibility = Visibility.Visible;
            ControlGrid.Visibility = Visibility.Collapsed;
            TopGrid.Visibility = Visibility.Collapsed;
            CowContent.Visibility = Visibility.Collapsed;
        }

        private SocketMessageWindow _msgWindow;

        private void BtnLog_OnClick(object sender, RoutedEventArgs e)
        {
            _msgWindow = new SocketMessageWindow(_client);
            _msgWindow.Closed += _msgWindow_Closed;
            _msgWindow.Show();
        }

        void _msgWindow_Closed(object sender, System.EventArgs e)
        {
            _msgWindow = null;
        }
    }
}
