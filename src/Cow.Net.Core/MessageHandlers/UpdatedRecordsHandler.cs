using System;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class UpdatedRecordsHandler
    {
        internal static void Handle(string message, CowStoreManager storeManager)
        {
            var updatedRecord = JsonConvert.DeserializeObject<CowMessage<UpdatedRecord>>(message);

            var storeId = updatedRecord.Payload.SyncType.ToString();
            var store = storeManager.GetStoreById(storeId);

            if (store == null)
                throw new Exception(string.Format("A store is not configured (correctly): {0}", storeId));

            store.HandleUpdatedRecord(updatedRecord);
        }
    }
}
