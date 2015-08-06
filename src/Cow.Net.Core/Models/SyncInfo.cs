using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class SyncInfo
    {
        [JsonProperty("IWillSent")]
        public List<string> WillSent { get; set; }

        [JsonProperty("IShallReceive")]
        public List<string> ShallReceive { get; set; }
    }
}
