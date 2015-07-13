using System;
using Newtonsoft.Json;

namespace Cow.Net.Core.Models
{
    public class Delta
    {
        [JsonProperty("timestamp")]
        public long TimeStamp { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        public Delta()
        {
            
        }

        public Delta(string userId, StoreRecord record)
        {
            UserId = userId;
            TimeStamp = DateTime.Now.Ticks;
            Deleted = record.Deleted;
            Data = record.Data;
        }
    }
}
