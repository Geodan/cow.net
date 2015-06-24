using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class MissingRecordsHandler
    {
        public static void Handle(string message, ObservableCowCollection<StoreObject> peers)
        {
            var missingRecords = JsonConvert.DeserializeObject<CowMessage<NewList>>(message);
            switch (missingRecords.Payload.SyncType)
            {
                case SyncType.peers:
                    peers.AddRange(missingRecords.Payload.List);
                    break;
            }
        }
    }
}
