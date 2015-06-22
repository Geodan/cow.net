using Newtonsoft.Json;

namespace cow.core.Models
{
    public class CowMessage<T>
    {
        [JsonProperty("action")]
        public Action Action { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("payload")]
        public T Payload { get; set; }
    }
}
