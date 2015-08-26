using System.Linq;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class PeerGonehandler
    {
        internal static void Handle(string message, CowStoreManager storeManager)
        {
            var peerGone = JsonConvert.DeserializeObject<CowMessage<PeerGone>>(message);
            var peerStore = storeManager.GetPeerStore();

            foreach (var storeObject in peerStore.Records.Where(storeObject => storeObject.Id.Equals(peerGone.Payload.GonePeerId)))
            {
                storeObject.Deleted = true;
                break;
            }
        }
    }
}
