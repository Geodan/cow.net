using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class CommandPayload
    {
        [JsonProperty("command")]
        public string Commando { get; set; }

        [JsonProperty("more")]
        public string More { get; set; }

        [JsonProperty("params")]
        public string Parameters { get; set; }
    }
}
