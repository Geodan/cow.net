using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class PeerGone : IPayload
    {
        [JsonProperty("gonePeerID")] 
        public string GonePeerId;
    }
}
