using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class WantedList
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("list")]
        public List<string> List { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
