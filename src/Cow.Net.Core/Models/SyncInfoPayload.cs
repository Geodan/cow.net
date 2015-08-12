using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class SyncInfoPayload : IPayload
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("syncInfo")]
        public SyncInfo SyncInfo { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }      
    }
}
