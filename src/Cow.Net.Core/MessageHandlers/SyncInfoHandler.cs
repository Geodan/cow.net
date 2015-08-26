using System;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class SyncInfoHandler
    {
        internal static void Handle(ConnectionInfo connectionInfo, string message, CowStoreManager storeManager)
        {
            var syncInfo = JsonConvert.DeserializeObject<CowMessage<SyncInfoPayload>>(message, CoreSettings.Instance.SerializerSettingsIncoming);
            var storeId = syncInfo.Payload.SyncType.ToString();
            var store = storeManager.GetStoreById(storeId);

            if (store == null)
                throw new Exception(string.Format("A store is not configured (correctly): {0}", storeId));

            store.HandleSyncInfo(syncInfo);
        }
    }
}
