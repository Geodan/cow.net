using System.Linq;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class PeerGonehandler
    {
        public static void Handle(string message, CowStore peers)
        {
            var peerGone = JsonConvert.DeserializeObject<CowMessage<PeerGone>>(message);
            foreach (var storeObject in peers.Records.Where(storeObject => storeObject.Id.Equals(peerGone.Payload.GonePeerId)))
            {
                peers.Remove(storeObject);
                break;
            }
        }
    }
}
