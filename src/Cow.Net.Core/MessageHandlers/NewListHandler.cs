using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Cow.Net.Core.Socket;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class NewListHandler
    {
        internal static async void Handle(string peerId, string message, CowStoreManager storeManager, IWebSocketConnectionProvider socketProvider)
        {
            var newList = JsonConvert.DeserializeObject<CowMessage<NewListPayload>>(message, CoreSettings.Instance.SerializerSettingsIncoming);
            var store = storeManager.GetStoreById(newList.Payload.SyncType.ToString());
            if (store == null)
                return;

            //Sync message
            List<StoreRecord> pushRecords;
            List<StoreRecord> requestRecords;
            store.Compare(newList.Payload.List, out pushRecords, out requestRecords, newList.Payload.Project);

            var syncMessage = CowMessageFactory.CreateSyncInfoMessage(newList.Payload.SyncType, pushRecords, requestRecords, newList.Payload.Project, peerId, newList.Sender);

            await socketProvider.SendAsync(JsonConvert.SerializeObject(syncMessage, Formatting.None, CoreSettings.Instance.SerializerSettingsOutgoing));

            //Wanted message
            var wantedMessage = CowMessageFactory.CreateWantedMessage(newList.Payload.SyncType, requestRecords, newList.Payload.Project, peerId, newList.Sender);
            if(requestRecords.Any())
                await socketProvider.SendAsync(JsonConvert.SerializeObject(wantedMessage, Formatting.None, CoreSettings.Instance.SerializerSettingsOutgoing));

            pushRecords = string.IsNullOrEmpty(newList.Payload.Project) ? pushRecords : pushRecords.Where(so => so.Identifier.Equals(newList.Payload.Project)).ToList(); 

            //Missing message
            foreach (var storeRecord in pushRecords)
            {
                var missingMessage = CowMessageFactory.CreateMissingRecordMessage(newList.Payload.SyncType, storeRecord, newList.Payload.Project, peerId, newList.Sender);
                await socketProvider.SendAsync(JsonConvert.SerializeObject(missingMessage, Formatting.None, CoreSettings.Instance.SerializerSettingsOutgoing));
            }           
        }
    }
}
