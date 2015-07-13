using System.Linq;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class PeerGonehandler
    {
        public static void Handle(string message, CowStoreManager storeManager)
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
