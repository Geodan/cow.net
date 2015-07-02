﻿using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class UpdatedRecord
    {
        [JsonProperty("syncType")]
        public SyncType SyncType { get; set; }

        [JsonProperty("record")]
        public StoreRecord Record { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }
    }
}
