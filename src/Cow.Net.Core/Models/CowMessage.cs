using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class CowMessage<T>
    {
        [JsonProperty("action")]
        public Action Action { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("payload")]
        public T Payload { get; set; }
    }
}
