using System;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class WantedRecordsHandler
    {
        internal static void Handle(string message, CowStoreManager storeManager)
        {
            var wantedRecords = JsonConvert.DeserializeObject<CowMessage<WantedList>>(message);
            var storeId = wantedRecords.Payload.SyncType.ToString();
            var store = storeManager.GetStoreById(storeId);

            if (store == null)
                throw new Exception(string.Format("A store is not configured (correctly): {0}", storeId));

            store.HandleWantedRecords(wantedRecords.Payload.Project, wantedRecords);
        }
    }
}
