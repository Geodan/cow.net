using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using cow.core.Annotations;
using Cow.Net.Core.Config;
using Cow.Net.Core.Exceptions;
using Cow.Net.Core.MessageHandlers;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Action = Cow.Net.Core.Models.Action;

namespace Cow.Net.Core
{
    public sealed class CowClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event CowEventHandlers.ConnectedHandler CowSocketConnected;
        public event CowEventHandlers.ConnectionInfoReceivedHandler CowConnectionInfoReceived;
        public event CowEventHandlers.ConnectionErrorHandler CowSocketConnectionError;
        public event CowEventHandlers.ConnectionClosedHandler CowSocketDisconnected;
        public event CowEventHandlers.DatabaseErrorHandler CowDatabaseError;
        public event CowEventHandlers.CommandReceivedHandler CowCommandReceived;        

        public ICowClientConfig Config { get; private set; }
                
        private bool _localStorageAvailable;        
        private ConnectionInfo _connectionInfo;
        private WebSocketSharp.WebSocket _socketClient;

        public ConnectionInfo ConnectionInfo
        {
            get { return _connectionInfo; }
            set
            {
                _connectionInfo = value;
                CoreSettings.Instance.ConnectionInfo = _connectionInfo;
                OnPropertyChanged();
            }
        }

        public CowClient(ICowClientConfig config)
        {
            Config = config;
            CoreSettings.Instance.SynchronizationContext = Config.SynchronizationContext;

            Initialize(Config.StorageProvider, Config.CowStoreManager, Config.Address);
        }

        public void Connect()
        {
            _socketClient.ConnectAsync();
        }

        public void Disconnect()
        {
            _socketClient.CloseAsync();
        }

        private void Initialize(IStorageProvider storageProvider, CowStoreManager storeManager, string address)
        {
            SetupDatabase(storageProvider);
            SetupSocketClient(address, null);
            var allStores = storeManager.GetAllStores();

            foreach (var cowStore in allStores)
            {
                cowStore.SyncRequested += CowStoreSyncRequested;
                cowStore.SyncRecordsRequested += CowStoreSyncRecordsRequested;
                cowStore.SendMissingRecordsRequested += CowStoreSendMissingRecordsRequested;
            }
        }

        private void CowStoreSyncRecordsRequested(object sender, StoreRecord record)
        {
            var store = sender as CowStore;
            if (store == null) return;

            var jsonString = JsonSerialize(CowMessageFactory.CreateUpdateMessage(ConnectionInfo, store.SyncType, record));
            _socketClient.Send(jsonString);
        }

        private void CowStoreSyncRequested(object sender, string identifier)
        {
            var store = sender as CowStore;            
            if (store == null) return;

            var jsonString = JsonSerialize(CowMessageFactory.CreateSyncMessage(ConnectionInfo, store.SyncType, store.Records, identifier));
            _socketClient.Send(jsonString);
        }

        private void CowStoreSendMissingRecordsRequested(object sender, string project, List<StoreRecord> records)
        {
            var store = sender as CowStore;
            if (store == null) return;

            var jsonString = JsonSerialize(CowMessageFactory.CreateMissingRecordsMessage(ConnectionInfo, store.SyncType, project, records));
            _socketClient.Send(jsonString);
        }

        private string JsonSerialize(CowMessage<Dictionary<string, object>> message)
        {
            return JsonConvert.SerializeObject(message, Formatting.None, CoreSettings.Instance.SerializerSettings);
        }
         
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
            _socketClient.OnError   += SocketClientOnError;
            _socketClient.OnOpen    += SocketClientOnOpen;
            _socketClient.OnClose   += SocketClientOnClose;
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
        }

        private void SyncStoreWithPeers()
        {
            foreach (var store in Config.CowStoreManager.MainStores)
            {
                store.SyncStore();   
            }
        }

        private void HandleReceivedMessage(string message)
        {
            dynamic d = JObject.Parse(message);

            // ReSharper disable once RedundantAssignment
            var action = Action.Unknown;
            if (d.action == null || !Enum.TryParse((string) d.action, out action))
            {
                Debug.WriteLine("Unknown action received");
                return;
            }

            switch (action)
            {
                case Action.connected:
                    HandleNewConnection(message);
                    break;
                case Action.command:
                    OnCowCommandReceived(CommandHandler.Handle(message));
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
                    Debug.WriteLine(message);
                    break;
                case Action.peerGone:
                    PeerGonehandler.Handle(message, Config.CowStoreManager);
                    break;
                case Action.newList:
                    var amiAlpha = AmIAlpha();
                    break;
            }
        }

        private void HandleNewConnection(string message)
        {
            ConnectionInfo = ConnectedHandler.Handle(message);
            CheckServerkey(ConnectionInfo);
            OnCowConnectionInfoReceived(ConnectionInfo);
            Config.CowStoreManager.GetPeerStore().Add(DefaultRecords.CreatePeerRecord(ConnectionInfo));
            Config.CowStoreManager.GetPeerStore().SyncRecords();
            Sync();
        }

        private bool AmIAlpha()
        {
            if (Config.IsAlphaPeer)
                return false;

            var peerStore = Config.CowStoreManager.GetPeerStore();
            if (peerStore.Records == null || !peerStore.Records.Any())
                return false;

            StoreRecord oldest = null;

            foreach (var storeRecord in peerStore.Records)
            {
                if (storeRecord.Deleted || storeRecord.Data == null || !storeRecord.Data.ContainsKey("family") || !storeRecord.Data["family"].Equals("alpha"))
                    continue;

                if (oldest == null)
                    oldest = storeRecord;

                if (storeRecord.Created < oldest.Created)
                    oldest = storeRecord;
            }

            return ConnectionInfo.PeerId.Equals(oldest.Id);
        }

        private void CheckServerkey(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.ServerKey.Equals(Config.ServerKey)) return;

            Disconnect();
            throw new IncorrectServerKeyException(Config.ServerKey, connectionInfo.ServerKey);
        }

        private void SocketClientOnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            HandleReceivedMessage(e.Data);
        }

        private void SocketClientOnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            var handler = CowSocketDisconnected;
            if (handler != null) handler(this);
        }

        private void SocketClientOnOpen(object sender, EventArgs e)
        {
            var handler = CowSocketConnected;
            if (handler != null) handler(this);
        }

        private void SocketClientOnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            var handler = CowSocketConnectionError;
            if (handler != null) handler(this, e.Message);
        }

        private void OnCowDatabaseError(string error)
        {
            var handler = CowDatabaseError;
            if (handler != null) handler(this, error);
        }

        private void OnCowConnectionInfoReceived(ConnectionInfo connectioninfo)
        {
            var handler = CowConnectionInfoReceived;
            if (handler != null) handler(this, connectioninfo);
        }

        private void OnCowCommandReceived(CowMessage<Command> commandmessage)
        {
            var handler = CowCommandReceived;
            if (handler != null) handler(this, commandmessage);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
