using System.Threading;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config
{
    public interface ICowClientConfig
    {
        string Address { get; set; }
        CowStoreManager CowStoreManager { get; set; }
        IStorageProvider StorageProvider { get; set; }
        SynchronizationContext SynchronizationContext { get; set; }
    }
}
