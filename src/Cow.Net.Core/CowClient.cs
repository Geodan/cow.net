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
        public event CowEventHandlers.CowSocketMessageReceivedHandler CowSocketMessageReceived;        

        public event CowEventHandlers.CowErrorHandler CowError;

        public ICowClientConfig Config { get; }
                        
        private ConnectionInfo _connectionInfo;
        private bool _connected;
        private bool _initialized;
        private readonly StoreRecord _peer;
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

            if(Config.WebSocketConnectionProvider == null)
                throw new Exception("Please provide a IWebSocketConnectionProvider");

            if(Config.CowStoreManager == null)
                throw new Exception("Please provide a CowStoreManager");

            CoreSettings.Instance.SynchronizationContext = Config.SynchronizationContext;

            _peer = DefaultRecords.CreatePeerRecord("0", Config.IsAlphaPeer, User, ActiveProject);            
            Config.CowStoreManager.GetPeerStore().Add(_peer);
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
                OnPropertyChanged();
                UpdatePeerUserId(value.Id);
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
                UpdateActiveProject(value.Id);
                OnPropertyChanged();                
            }
        }

        private void UpdateActiveProject(string activeProjectId)
        {
            if (_peer == null)
                return;

            _peer.UpdateData("activeproject", activeProjectId);
            _peer.Sync();  
        }

        private void UpdatePeerUserId(string userId)
        {
            if (_peer == null)
                return;

            _peer.UpdateData("userid", userId);
            _peer.Sync();  
        }

        /// <summary>
        /// Start the client (loading from local storage) Call OpenAsync to 
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
        /// OpenAsync to the websocket and start syncing
        /// </summary>
        /// <param name="host"></param>
        /// <param name="endpoint"></param>
        /// <param name="subProtocol"></param>
        /// <param name="port"></param>
        public async void Connect(string host, int port, string[] endpoint, string subProtocol)
        {
            if (!_initialized)
                StartClient();

            SetupSocketClient();
            try
            {
                await Config.WebSocketConnectionProvider.OpenAsync(host, port, endpoint, subProtocol);
            }
            catch (Exception e)
            {
                SocketClientError(Config.WebSocketConnectionProvider, new Exception(e.Message));
            }            
        }

        /// <summary>
        /// CloseAsync from the websocket
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
                return;

            Config.WebSocketConnectionProvider.CloseAsync();
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
                var msg = CowMessageFactory.CreateSyncMessage(ConnectionInfo, ((CowStore) sender).SyncType, ((CowStore) sender).Records, identifier);
                var jsonString = JsonSerialize(msg);
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
            return JsonConvert.SerializeObject(message, Formatting.None, CoreSettings.Instance.SerializerSettingsOutgoing);
        }

        private string JsonSerialize(CowMessage<CommandPayload> message)
        {
            return JsonConvert.SerializeObject(message, Formatting.None, CoreSettings.Instance.SerializerSettingsOutgoing);
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

        private void HandleReceivedMessage(dynamic d, string message)
        {
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
                        NewListHandler.Handle(ConnectionInfo.PeerId, message, Config.CowStoreManager, Config.WebSocketConnectionProvider);                    
                    break;
            }
        }

        /// <summary>
        /// Send a custom command
        /// </summary>
        /// <param name="command">You custom CommandPayload</param>
        /// <param name="target">PeerId of target, if not supplied command will be broadcasted</param>
        public void SendCommand(CommandPayload command, string target = null)
        {
            var commandMessage = CowMessageFactory.CreateCommandMessage(command, ConnectionInfo.PeerId, target);
            Config.WebSocketConnectionProvider.SendAsync(JsonSerialize(commandMessage));
        }

        private void Send(string json)
        {
            Config.WebSocketConnectionProvider.SendAsync(json);
        }

        private void HandleNewConnection(string message)
        {
            ConnectionInfo = ConnectedHandler.Handle(message);
            if(CheckCowServerConnection(ConnectionInfo))
                return;

            _peer.Id = ConnectionInfo.PeerId;

            OnCowConnectionInfoReceived(ConnectionInfo);

            if(_activeProject != null)
                UpdateActiveProject(_activeProject.Id);
            if(CoreSettings.Instance.CurrentUser != null)
                UpdatePeerUserId(CoreSettings.Instance.CurrentUser.Id);

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


        private void SetupSocketClient()
        {
            //address, protocols ?? new[] { "connect" }
            Config.WebSocketConnectionProvider.SocketOpened += SocketClientOpened;
            Config.WebSocketConnectionProvider.SocketClosed += SocketClientClosed;
            Config.WebSocketConnectionProvider.SocketError += SocketClientError;
            Config.WebSocketConnectionProvider.SocketMessageReceived += SocketClientMessageReceived;
        }

        private void SocketClientError(object sender, Exception obj)
        {
            var handler = CowSocketConnectionError;
            if (handler == null)
                return;
            
            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, obj == null ? "" : obj.Message), null);
            }
            else
            {
                handler(this, obj.Message);
            }
        }

        private void SocketClientClosed(object sender, string message)
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

        private void SocketClientOpened(object sender)
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

        private void SocketClientMessageReceived(object sender, string message)
        {
            if (!Connected || string.IsNullOrEmpty(message))
                Connected = true;

            dynamic d = JObject.Parse(message);

            //Ignore broadcast to self
            if (d["sender"] != null && d["sender"].ToString().Equals(ConnectionInfo.PeerId) && (d["target"] == null || string.IsNullOrEmpty(d["target"].ToString())))
                return;

            HandleReceivedMessage(d, message);
            OnCowSocketMessageReceived(message);
        }

        private void OnCowSocketMessageReceived(string message)
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
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, commandmessage.Payload), null);
            }
            else
            {
                handler(this, commandmessage.Payload);
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
