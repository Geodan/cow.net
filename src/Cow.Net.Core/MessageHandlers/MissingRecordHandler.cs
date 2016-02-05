using System;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class MissingRecordHandler
    {
        internal static void Handle(ConnectionInfo connectionInfo, string message, CowStoreManager storeManager)
        {
            var missingRecords = JsonConvert.DeserializeObject<CowMessage<RecordPayload>>(message, CoreSettings.Instance.SerializerSettingsIncoming);
            var storeId = missingRecords.Payload.SyncType.ToString();
            var store = storeManager.GetStoreById(storeId);
            
            if(store == null)
                throw new Exception(string.Format("A store is not configured (correctly): {0}", storeId));

            store.HandleRecord(missingRecords);
        }
    }
}
