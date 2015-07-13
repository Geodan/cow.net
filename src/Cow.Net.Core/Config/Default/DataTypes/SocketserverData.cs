using Newtonsoft.Json;

namespace Cow.Net.Core.Config.Default.DataTypes
{
    public class SocketserverData
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("dir")]
        public string Dir { get; set; }
    }
}
