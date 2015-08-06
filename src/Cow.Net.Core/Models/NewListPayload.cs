using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class NewListPayload
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("list")]
        public List<StoreRecord> List { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
