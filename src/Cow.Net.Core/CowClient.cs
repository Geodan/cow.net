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
using WebSocketSharp;
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
        public event CowEventHandlers.CowSocketMessageReceivedHandler CowSocketMessageReceived;        

        public event CowEventHandlers.CowErrorHandler CowError;

        public ICowClientConfig Config { get; private set; }
                        
        private ConnectionInfo _connectionInfo;
        private bool _connected;
        private WebSocket _socketClient;
        private bool _initialized;
        private StoreRecord _peer;
        private StoreRecord _activeProject;

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

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }

        public CowClient(ICowClientConfig config)
        {
            Config = config;
            CoreSettings.Instance.SynchronizationContext = Config.SynchronizationContext;
        }

        /// <summary>
        /// Set or get the current user, will update userid in peer
        /// </summary>
        public StoreRecord User
        {
            get { return CoreSettings.Instance.CurrentUser; }
            set
            {
                CoreSettings.Instance.CurrentUser = value;

                if (_peer == null)
                    return;

                _peer.UpdateData("userid", value.Id);
                _peer.Sync();
            }             
        }

        /// <summary>
        /// Set or get the current active project, will update activeproject id in peer
        /// </summary>
        public StoreRecord ActiveProject
        {
            get { return _activeProject; }
            set
            {
                _activeProject = value;

                if (_peer == null)
                    return;

                _peer.UpdateData("activeproject", value.Id);
                _peer.Sync();
            }
        }

        /// <summary>
        /// Start the client (loading from local storage) Call Connect to 
        /// start syncing with other peers
        /// </summary>
        public void StartClient()
        {
            if (_initialized)
                return;

            Initialize(Config.StorageProvider, Config.CowStoreManager);
            _initialized = true;
        }

        /// <summary>
        /// Connect to the websocket and start syncing
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="protocols"></param>
        public void Connect(string ip, string[] protocols)
        {
            if (!_initialized)
                StartClient();

            SetupSocketClient(ip, protocols);
            _socketClient.ConnectAsync();
        }

        /// <summary>
        /// Disconnect from the websocket
        /// </summary>
        public void Disconnect()
        {
            if (_socketClient == null || !Connected)
                return;

            _socketClient.CloseAsync();
            ConnectionInfo.Reset();
            var stores = Config.CowStoreManager.GetAllStores();
            foreach (var cowStore in stores)
            {
                if (cowStore.SaveToLocalDatabase)
                    continue;

                cowStore.Clear();
            }            
        }

        private void Initialize(IStorageProvider storageProvider, CowStoreManager storeManager)
        {
            SetupDatabase(storageProvider);
            var allStores = storeManager.GetAllStores();

            foreach (var cowStore in allStores)
            {
                cowStore.SyncRequested += CowStoreSyncRequested;
                cowStore.SyncRecordRequested += CowStoreSyncRecordRequested;
                cowStore.SendMissingRecordsRequested += CowStoreSendMissingRecordsRequested;
                cowStore.RequestedRecordsRequested += CowStoreRequestedRecordsRequested;
            }

            LoadFromStorage();
        }

        //ToDo: refactor: send one event from store: switch case on enum RecordAction instead of these 4 events
        private void CowStoreSyncRecordRequested(object sender, StoreRecord record)
        {
            if (Connected)
            {
                var jsonString = JsonSerialize(CowMessageFactory.CreateUpdateMessage(ConnectionInfo, ((CowStore) sender).SyncType, record));
                Send(jsonString);
            }
        }

        private void CowStoreSyncRequested(object sender, string identifier)
        {
            if (Connected)
            {
                var jsonString = JsonSerialize(CowMessageFactory.CreateSyncMessage(ConnectionInfo, ((CowStore) sender).SyncType,((CowStore) sender).Records, identifier));
                Send(jsonString);
            }
        }

        private void CowStoreSendMissingRecordsRequested(object sender, string project, List<StoreRecord> records)
        {
            if (Connected)
            {
                records = string.IsNullOrEmpty(project) ? records : records.Where(so => so.Identifier.Equals(project)).ToList();
                foreach (var storeRecord in records)
                {
                    var jsonString = JsonSerialize(CowMessageFactory.CreateMissingRecordMessage(((CowStore) sender).SyncType, storeRecord, project, ConnectionInfo.PeerId));
                    Send(jsonString);
                }
            }
        }

        private void CowStoreRequestedRecordsRequested(object sender, string project, List<StoreRecord> records)
        {
            if (!Connected)
                return;

            records = string.IsNullOrEmpty(project) ? records : records.Where(so => so.Identifier.Equals(project)).ToList();

            foreach (var storeRecord in records)
            {
                var jsonString = JsonSerialize(CowMessageFactory.CreateRequestedMessage(((CowStore)sender).SyncType, storeRecord, project, ConnectionInfo.PeerId));
                Send(jsonString);
            }            
        }

        private string JsonSerialize(CowMessage<DictionaryPayload> message)
        {
            return JsonConvert.SerializeObject(message, Formatting.None, CoreSettings.Instance.SerializerSettings);
        }
         
        private void SetupDatabase(IStorageProvider storageProvider)
        {
            var stores = (from store in Config.CowStoreManager.GetAllStores() where store.SaveToLocalDatabase select store.Id).ToList();
            CoreSettings.Instance.LocalStorageAvailable = storageProvider.PrepareDatabase(stores);
            if (!CoreSettings.Instance.LocalStorageAvailable)
            {
                OnCowDatabaseError("Unable to start database");
            }
        }

        private void SetupSocketClient(string address, string[] protocols)
        {
            _socketClient = new WebSocket(address, protocols ?? new[] { "connect" });
            _socketClient.OnError   += SocketClientOnError;
            _socketClient.OnOpen    += SocketClientOnOpen;
            _socketClient.OnClose   += SocketClientOnClose;
            _socketClient.OnMessage += SocketClientOnMessage;
        }

        private void LoadFromStorage()
        {
            if (!CoreSettings.Instance.LocalStorageAvailable)
                return;

            foreach (var store in Config.CowStoreManager.MainStores)
            {
                store.LoadFromStorage(Config.StorageProvider, Config.MaxDataAge);
            }
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
            
            //Ignore broadcast to self
            if (d["sender"] != null && d["sender"].ToString().Equals(ConnectionInfo.PeerId) && (d["target"] == null || string.IsNullOrEmpty(d["target"].ToString())))
                return;

            // ReSharper disable once RedundantAssignment
            var action = Action.Unknown;
            if (d["action"] == null || !Enum.TryParse((string)d["action"], out action))
            {
                Debug.WriteLine("Unknown action received");
                return;
            }

            switch (action)
            {
                //websocket confirms connection by returning the unique peerID (targeted)
                case Action.connected:
                    HandleNewConnection(message);
                    break;

                //Received a command
                case Action.command:
                    OnCowCommandReceived(CommandHandler.Handle(message));
                    break;

                //you just joined and receive the records you are missing (targeted)
                case Action.missingRecord:
                    MissingRecordHandler.Handle(ConnectionInfo, message, Config.CowStoreManager);
                    break;

                //you just joined and you receive info from the alpha peer on how much will be synced
                case Action.syncinfo:       
                    SyncInfoHandler.Handle(ConnectionInfo, message, Config.CowStoreManager);
                    break;

                //a peer sends a new or updated record
                case Action.updatedRecord:
                    UpdatedRecordHandler.Handle(message, Config.CowStoreManager);
                    break;

                //you just joined and you receive a list of records the others want (targeted)
                case Action.wantedList:
                    WantedRecordsHandler.Handle(message, Config.CowStoreManager);
                    break;

                //a new peer has arrived and sends everybody the records that are requested in the *wantedList*
                case Action.requestedRecord:
                    MissingRecordHandler.Handle(ConnectionInfo, message, Config.CowStoreManager);
                    break;

                //messenger tells everybody a peer has gone, with ID: peerID
                case Action.peerGone:
                    PeerGonehandler.Handle(message, Config.CowStoreManager);
                    break;

                //a new peer has arrived and gives a list of its records
                case Action.newList:
                    if (AmIAlpha())                    
                        NewListHandler.Handle(ConnectionInfo.PeerId, message, Config.CowStoreManager, _socketClient);                    
                    break;
            }
        }

        private void Send(string json)
        {
            _socketClient.Send(json);   
        }

        private void HandleNewConnection(string message)
        {
            ConnectionInfo = ConnectedHandler.Handle(message);
            if(CheckCowServerConnection(ConnectionInfo))
                return;

            OnCowConnectionInfoReceived(ConnectionInfo);
            _peer = DefaultRecords.CreatePeerRecord(ConnectionInfo, Config.IsAlphaPeer, User, ActiveProject);
            Config.CowStoreManager.GetPeerStore().Add(_peer);

            SyncStoreWithPeers();                      
        }

        private bool AmIAlpha()
        {
            if (!Config.IsAlphaPeer)
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

            return oldest != null && ConnectionInfo.PeerId.Equals(oldest.Id);
        }

        private bool CheckCowServerConnection(ConnectionInfo connectionInfo)
        {
            var error = false;

            //Check key
            if (!connectionInfo.ServerKey.Equals(Config.ServerKey))
            {             
                OnCowError(new IncorrectServerKeyException(Config.ServerKey, connectionInfo.ServerKey));
                error = true;
            }

            //Check time difference
            if (Math.Abs(connectionInfo.ServerTime - TimeUtils.GetMillisencondsFrom1970()) > 
                Config.MaxClientServerTimeDifference.TotalMilliseconds)
            {
                OnCowError(new TimeDifferenceException(TimeUtils.GetDateTimeFrom1970Milliseconds(connectionInfo.ServerTime),
                    TimeUtils.GetDateTimeFrom1970Milliseconds(TimeUtils.GetMillisencondsFrom1970())));
                error = true;
            }

            //Check server version
            if (!connectionInfo.ServerVersion.Equals(CoreSettings.Instance.SupportedServerVerion))
            {
                OnCowError(new IncorrectCowServerVersion(connectionInfo.ServerVersion, CoreSettings.Instance.SupportedServerVerion));
                error = true;
            }           
 
            if(error)
                Disconnect();

            return error;
        }        

        #region events

        //Bubble socket events up
        private void SocketClientOnMessage(object sender, MessageEventArgs e)
        {            
            if(!Connected)
                Connected = true;

            HandleReceivedMessage(e.Data);
            OnCowSocketMessageReceived(e);
        }

        private void OnCowSocketMessageReceived(MessageEventArgs message)
        {            
            var handler = CowSocketMessageReceived;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, message), null);
            }
            else
            {
                handler(this, message);
            }
        }

        private void SocketClientOnClose(object sender, CloseEventArgs e)
        {
            Connected = false;

            var handler = CowSocketDisconnected;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this), null);
            }
            else
            {
                handler(this);
            }

            Connected = false;
        }

        private void SocketClientOnOpen(object sender, EventArgs e)
        {
            Connected = true;

            var handler = CowSocketConnected;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this), null);
            }
            else
            {
                handler(this);
            }            
        }

        private void SocketClientOnError(object sender, ErrorEventArgs e)
        {
            var handler = CowSocketConnectionError;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, e.Message), null);
            }
            else
            {
                handler(this, e.Message);
            }
        }

        //Cow related events
        private void OnCowDatabaseError(string error)
        {
            var handler = CowDatabaseError;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, error), null);
            }
            else
            {
                handler(this, error);
            }
        }

        private void OnCowConnectionInfoReceived(ConnectionInfo connectioninfo)
        {
            var handler = CowConnectionInfoReceived;
            if (handler == null)
                return;
            
            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, connectioninfo), null);
            }
            else
            {
                handler(this, connectioninfo);
            }
        }

        private void OnCowCommandReceived(CowMessage<CommandPayload> commandmessage)
        {
            var handler = CowCommandReceived;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, commandmessage), null);
            }
            else
            {
                handler(this, commandmessage);
            }
        }

        private void OnCowError(Exception e)
        {
            var handler = CowError;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, e), null);
            }
            else
            {
                handler(this, e);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, new PropertyChangedEventArgs(propertyName)), null);
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
