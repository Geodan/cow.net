using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class NewList
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("list")]
        public List<StoreObject> List { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
