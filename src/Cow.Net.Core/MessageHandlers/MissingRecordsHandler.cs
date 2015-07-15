using System;
using Cow.Net.Core.Models;
using Newtonsoft.Json;
using WebSocketSharp;

namespace Cow.Net.Core.MessageHandlers
{
    public class MissingRecordsHandler
    {
        public static void Handle(WebSocket socket, ConnectionInfo connectionInfo, string message, CowStoreManager storeManager)
        {
            var missingRecords = JsonConvert.DeserializeObject<CowMessage<NewList>>(message);         
            var storeId = missingRecords.Payload.SyncType.ToString();
            var store = storeManager.GetStoreById(storeId);
            
            if(store == null)
                throw new Exception(string.Format("A store is not configured (correctly): {0}", storeId));

            store.HandleMissingRecords(socket, connectionInfo, missingRecords);
        }
    }
}
