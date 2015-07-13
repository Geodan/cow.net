using System.Collections.Generic;
using System.Threading;
using Cow.Net.Core.Config.Default.Stores;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config.Default
{
    public class DefaultConfig : ICowClientConfig
    {
        public PeerStore Peers { get; private set; }
        public UserStore Users { get; private set; }
        public SocketServerStore SocketServers { get; private set; }
        public ProjectStore Projects { get; private set; }

        public string Address { get; set; }
        public CowStoreManager CowStoreManager { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public SynchronizationContext SynchronizationContext { get; set; }

        public DefaultConfig(string address, IStorageProvider storageProvider, SynchronizationContext synchronizationContext = null)
        {
            Address = address;
            StorageProvider = storageProvider;
            SynchronizationContext = synchronizationContext;
            SetupStores();
        }

        private void SetupStores()
        {
            Peers = new PeerStore("peers", SyncType.peers, null, false);
            Users = new UserStore("users", SyncType.users);
            SocketServers = new SocketServerStore("socketservers", SyncType.socketservers);
            Projects = new ProjectStore("projects", SyncType.projects, new List<CowStore>
            {
                new ItemStore("items", SyncType.items),
                new GroupStore("groups", SyncType.groups)
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
