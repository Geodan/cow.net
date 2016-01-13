using System;
using System.Threading;
using Cow.Net.Core.Socket;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config
{
    public interface ICowClientConfig
    {
        string ServerKey { get; set; }
        bool IsAlphaPeer { get; set; }
        TimeSpan MaxClientServerTimeDifference { get; set; }
        TimeSpan MaxDataAge { get; set; }
        CowStoreManager CowStoreManager { get; set; }
        IStorageProvider StorageProvider { get; set; }
        IWebSocketConnectionProvider WebSocketConnectionProvider { get; set; }
        SynchronizationContext SynchronizationContext { get; set; }
    }
}
