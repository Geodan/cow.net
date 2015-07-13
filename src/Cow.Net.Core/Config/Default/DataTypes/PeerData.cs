using Newtonsoft.Json;

namespace Cow.Net.Core.Config.Default.DataTypes
{
    public class PeerData
    {
        [JsonProperty("userid")]
        public string Userid { get; set; }

        [JsonProperty("family")]
        public string Family { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("activeproject")]
        public string Activeproject { get; set; }
    }            
}
