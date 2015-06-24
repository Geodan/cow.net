using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class CowEventHandlers
    {
        //Connection
        public delegate void ConnectedHandler(object sender, ConnectionInfo connectionInfo);
        public delegate void ConnectionErrorHandler(object sender, string error);
        public delegate void ConnectionClosedHandler(object sender);

        //Syncing
        public delegate void SyncStartedHandler(object sender);
        public delegate void SyncFinishedHandler(object sender);
        public delegate void SyncFailedHandler(object sender, string error);

        //Store
        public delegate void DatabaseErrorHandler(object sender, string error);
    }
}
