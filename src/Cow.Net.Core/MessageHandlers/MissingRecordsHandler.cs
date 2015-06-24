using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class MissingRecordsHandler
    {
        public static void Handle(string message, ObservableCollection<Peer> peers)
        {
            var missingRecords = JsonConvert.DeserializeObject<CowMessage<NewList<object>>>(message);
            switch (missingRecords.Payload.SyncType)
            {
                case SyncType.peers:
                    HandleMissingPeers(missingRecords, peers);
                    break;
            }
        }

        private static void HandleMissingPeers(CowMessage<NewList<object>> missingRecords, ObservableCollection<Peer> peers)
        {
            var newPeers = JsonConvert.DeserializeObject<List<Peer>>(missingRecords.Payload.List.ToString());
            if (newPeers.Any() && peers == null)
                peers = new ObservableCollection<Peer>();

            foreach (var peer in newPeers)
            {
                peers.Add(peer);
            }
        }
    }
}
