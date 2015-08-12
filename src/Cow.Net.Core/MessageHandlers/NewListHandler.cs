using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using WebSocketSharp;

namespace Cow.Net.Core.MessageHandlers
{
    internal class NewListHandler
    {
        internal static void Handle(string peerId, string message, CowStoreManager storeManager, WebSocket socket)
        {
            var newList = JsonConvert.DeserializeObject<CowMessage<NewListPayload>>(message, CoreSettings.Instance.SerializerSettings);
            var store = storeManager.GetStoreById(newList.Payload.SyncType.ToString());
            if (store == null)
                return;

            //Sync message
            List<StoreRecord> pushRecords;
            List<StoreRecord> requestRecords;
            store.Compare(newList.Payload.List, out pushRecords, out requestRecords, newList.Payload.Project);

            var syncMessage = CowMessageFactory.CreateSyncInfoMessage(newList.Payload.SyncType, pushRecords, requestRecords, newList.Payload.Project, peerId, newList.Sender);

            socket.Send(JsonConvert.SerializeObject(syncMessage, Formatting.None, CoreSettings.Instance.SerializerSettings));

            //Wanted message
            var wantedMessage = CowMessageFactory.CreateWantedMessage(newList.Payload.SyncType, requestRecords, newList.Payload.Project, peerId, newList.Sender);
            if(requestRecords.Any())
                socket.Send(JsonConvert.SerializeObject(wantedMessage, Formatting.None, CoreSettings.Instance.SerializerSettings));

            pushRecords = string.IsNullOrEmpty(newList.Payload.Project) ? pushRecords : pushRecords.Where(so => so.Identifier.Equals(newList.Payload.Project)).ToList(); 

            //Missing message
            foreach (var storeRecord in pushRecords)
            {
                var missingMessage = CowMessageFactory.CreateMissingRecordMessage(newList.Payload.SyncType, storeRecord, newList.Payload.Project, peerId, newList.Sender);
                socket.Send(JsonConvert.SerializeObject(missingMessage, Formatting.None, CoreSettings.Instance.SerializerSettings));
            }           
        }
    }
}
