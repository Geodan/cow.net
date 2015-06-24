using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class StoreObject
    {        
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("dirty")]
        public bool Dirty { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("deltas")]
        public Delta[] Deltas { get; set; }
    }
}
