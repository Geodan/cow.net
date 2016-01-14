using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class CommandPayload : IPayload
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
