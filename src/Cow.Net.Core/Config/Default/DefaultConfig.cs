using System;
using System.Collections.Generic;
using System.Threading;
using Cow.Net.Core.Config.Default.Stores;
using Cow.Net.Core.Models;
using Cow.Net.Core.Socket;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config.Default
{
    public class DefaultConfig : ICowClientConfig
    {
        public PeerStore Peers { get; private set; }
        public UserStore Users { get; private set; }
        public SocketServerStore SocketServers { get; private set; }
        public ProjectStore Projects { get; private set; }

        public TimeSpan MaxClientServerTimeDifference { get; set; }
        public TimeSpan MaxDataAge { get; set; }
        public string ServerKey { get; set; }
        public bool IsAlphaPeer{ get; set; }
        public CowStoreManager CowStoreManager { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public IWebSocketConnectionProvider WebSocketConnectionProvider { get; set; }
        public SynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// Create a default config
        /// </summary>
        /// <param name="serverKey">Server key of the server instance</param>
        /// <param name="isAlphaPeer">Set to true if this peer can be used as alpha</param>
        /// <param name="storageProvider">Local storage provider to use</param>
        /// <param name="webSocketConnectionProvider">Provider for communicating with websockets</param>
        /// <param name="synchronizationContext">Context for synchronizing to the main thread if needed</param>
        public DefaultConfig(string serverKey, bool isAlphaPeer, IStorageProvider storageProvider, IWebSocketConnectionProvider webSocketConnectionProvider, SynchronizationContext synchronizationContext = null)
        {
            ServerKey = serverKey;
            StorageProvider = storageProvider;
            WebSocketConnectionProvider = webSocketConnectionProvider;
            IsAlphaPeer = isAlphaPeer;
            MaxClientServerTimeDifference = TimeSpan.FromMinutes(5);
            MaxDataAge = TimeSpan.FromDays(365);
            SynchronizationContext = synchronizationContext;
            SetupStores();
        }

        private void SetupStores()
        {
            Peers = new PeerStore("peers", SyncType.peers, null, null, false, false);
            Users = new UserStore("users", SyncType.users, StorageProvider, null, true, false);
            SocketServers = new SocketServerStore("socketservers", SyncType.socketservers, StorageProvider, null, true, false);
            Projects = new ProjectStore("projects", SyncType.projects, StorageProvider, new List<CowStore>
            {
                new ItemStore("items", SyncType.items, StorageProvider),
                new GroupStore("groups", SyncType.groups, StorageProvider, null, true, false)
            });

            CowStoreManager = new CowStoreManager(new List<CowStore>
            {
                Peers,
                Users,
                SocketServers,
                Projects 
            });
        }
    }
}
