using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using cow.core.Annotations;
using Cow.Net.Core.Config;
using Cow.Net.Core.MessageHandlers;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Action = Cow.Net.Core.Models.Action;

namespace Cow.Net.Core
{
    public sealed class CowClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event CowEventHandlers.ConnectedHandler CowConnected;
        public event CowEventHandlers.ConnectionErrorHandler CowConnectionError;
        public event CowEventHandlers.ConnectionClosedHandler CowDisconnected;
        public event CowEventHandlers.DatabaseErrorHandler CowDatabaseError;

        public ICowClientConfig Config { get; private set; }
        
        private WebSocketSharp.WebSocket _socketClient;
        private bool _localStorageAvailable;        
        private ConnectionInfo _connectionInfo;                      

        public ConnectionInfo ConnectionInfo
        {
            get { return _connectionInfo; }
            set
            {
                _connectionInfo = value;
                OnPropertyChanged();
            }
        }

        public CowClient(ICowClientConfig config)
        {
            Config = config;
            CoreSettings.Instance.SynchronizationContext = Config.SynchronizationContext;

            Initialize(Config.StorageProvider, Config.Address);
        }

        public void Connect()
        {
            _socketClient.ConnectAsync();
        }

        public void Disconnect()
        {
            _socketClient.CloseAsync();
        }

        private void Initialize(IStorageProvider storageProvider, string address)
        {
            SetupDatabase(storageProvider);
            SetupSocketClient(address, null);
        }

        //private void ProjectsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems == null)
        //        return;

        //    foreach (Project item in e.NewItems)
        //    {
        //        _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.items, item.Items.ToList()), Formatting.None, _serializerSettings));
        //        _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.groups, item.Groups.ToList()), Formatting.None, _serializerSettings));
        //    }
        //}
         
        private void SetupDatabase(IStorageProvider storageProvider)
        {
            _localStorageAvailable = storageProvider.PrepareDatabase(Config.CowStoreManager.GetStoreIds(false));
            if (!_localStorageAvailable)
            {
                OnCowDatabaseError("Unable to start database");
            }
        }

        private void SetupSocketClient(string address, string[] protocols)
        {
            _socketClient = new WebSocketSharp.WebSocket(address, protocols ?? new[] { "connect" });
            _socketClient.OnError += SocketClientOnError;
            _socketClient.OnOpen += SocketClientOnOpen;
            _socketClient.OnClose += SocketClientOnClose;
            _socketClient.OnMessage += SocketClientOnMessage;
        }

        private async void Sync()
        {
            LoadFromStorage();
            SyncStoreWithPeers();
        }

        private void LoadFromStorage()
        {
            if (!_localStorageAvailable)
                return;

            //Peers.AddItems(_storageProvider.GetPeers());
        }

        private void SyncStoreWithPeers()
        {
            foreach (var store in Config.CowStoreManager.Stores)
            {
                store.Sync(_socketClient, ConnectionInfo);   
            }
        }

        private void HandleReceivedMessage(string message)
        {
            dynamic d = JObject.Parse(message);

            var action = Action.Unknown;
            if (d.action == null || !Enum.TryParse((string) d.action, out action))
            {
                Debug.WriteLine("Unknown action received");
                return;
            }

            switch (action)
            {
                case Action.connected:
                    ConnectionInfo = ConnectedHandler.Handle(message);
                    Sync();
                    break;
                case Action.missingRecords:
                    MissingRecordsHandler.Handle(_socketClient, ConnectionInfo, message, Config.CowStoreManager);
                    break;
                case Action.syncinfo: //What is going to sync                    
                    break;
                case Action.updatedRecord:
                    UpdatedRecordsHandler.Handle(message, Config.CowStoreManager);
                    break;
                case Action.wantedList: //List of items to broadcast
                    break;
                case Action.peerGone:
                    //PeerGonehandler.Handle(message, Peers);
                    break;
            }
        }

        #region websocket 

        private void SocketClientOnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            HandleReceivedMessage(e.Data);
        }

        private void SocketClientOnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            var handler = CowDisconnected;
            if (handler != null) handler(this);
        }

        private void SocketClientOnOpen(object sender, EventArgs e)
        {
            var handler = CowConnected;
            if (handler != null) handler(this);
        }

        private void SocketClientOnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            var handler = CowConnectionError;
            if (handler != null) handler(this, e.Message);
        }

        #endregion

        private void OnCowDatabaseError(string error)
        {
            var handler = CowDatabaseError;
            if (handler != null) handler(this, error);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
