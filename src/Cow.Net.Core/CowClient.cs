using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using cow.core.Annotations;
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
        public event CowEventHandlers.SyncStartedHandler CowSyncStarted;
        public event CowEventHandlers.SyncFinishedHandler CowSyncFinished;

        public ObservableCowCollection<StoreObject> Peers { get; private set; }
        public ObservableCowCollection<StoreObject> Projects { get; private set; }
        public ObservableCowCollection<StoreObject> SocketServers { get; private set; }
        public ObservableCowCollection<StoreObject> Items { get; private set; }
        public ObservableCowCollection<StoreObject> Groups { get; private set; }

        private IStorageProvider _storageProvider;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageProvider"></param>
        /// <param name="address"></param>
        /// <param name="protocols"></param>
        public CowClient(IStorageProvider storageProvider, string address, string[] protocols = null)
        {
            Initialize(storageProvider, address, protocols);
        }

        public void Connect()
        {
            _socketClient.ConnectAsync();
        }

        public void Disconnect()
        {
            _socketClient.CloseAsync();
        }

        private void Initialize(IStorageProvider storageProvider, string address, string[] protocols)
        {
            SetupCollections();
            SetupDatabase(storageProvider);
            SetupSocketClient(address, protocols);
        }

        private void SetupCollections()
        {
            Peers = new ObservableCowCollection<StoreObject>();
            Projects = new ObservableCowCollection<StoreObject>();
            SocketServers = new ObservableCowCollection<StoreObject>();
            Items = new ObservableCowCollection<StoreObject>();
            Groups = new ObservableCowCollection<StoreObject>();
        }

        private void SetupDatabase(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            _localStorageAvailable = _storageProvider.PrepareDatabase();
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
            OnCowSyncStarted();
            LoadFromStorage();
            SyncWithPeers();
            OnCowSyncFinished();
        }

        private void LoadFromStorage()
        {
            if (!_localStorageAvailable)
                return;

            Peers.AddRange(_storageProvider.GetPeers());
        }

        private void SyncWithPeers()
        {
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            serializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.peers, Peers.ToList()), Formatting.None, serializerSettings));
            _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.projects, Projects.ToList()), Formatting.None, serializerSettings));
            _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.projects, SocketServers.ToList()), Formatting.None, serializerSettings));
            _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.projects, Items.ToList()), Formatting.None, serializerSettings));
            _socketClient.Send(JsonConvert.SerializeObject(CowMessageFactory.CreateSyncMessage(ConnectionInfo, SyncType.projects, Groups.ToList()), Formatting.None, serializerSettings));
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
                    MissingRecordsHandler.Handle(message, Peers);
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

        private void OnCowSyncStarted()
        {
            var handler = CowSyncStarted;
            if (handler != null) handler(this);
        }

        private void OnCowSyncFinished()
        {
            var handler = CowSyncFinished;
            if (handler != null) handler(this);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
