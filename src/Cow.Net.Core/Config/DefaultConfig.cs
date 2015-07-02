using System.Collections.Generic;
using System.Threading;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config
{
    public class DefaultConfig : ICowClientConfig
    {
        public CowStore Peers { get; private set; }
        public CowStore Users { get; private set; }
        public CowStore SocketServers { get; private set; }
        public CowStore Projects { get; private set; }

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
            Peers = new CowStore("peers", null, false);
            Users = new CowStore("users");
            SocketServers = new CowStore("socketservers");
            Projects = new CowStore("projects", new List<CowStore>
            {
                new CowStore("items"),
                new CowStore("groups")
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
