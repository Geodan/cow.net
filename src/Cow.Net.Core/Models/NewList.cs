using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class NewList<T>
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("list")]
        public T List { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
