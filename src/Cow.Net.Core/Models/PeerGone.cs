using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class PeerGone
    {
        [JsonProperty("gonePeerID")] 
        public string GonePeerId;
    }
}
