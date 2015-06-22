using System.Collections.Generic;
using Newtonsoft.Json;

namespace cow.core.Models
{
    public class NewList<T>
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("list")]
        public List<T> List { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
