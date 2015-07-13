using Newtonsoft.Json;

namespace Cow.Net.Core.Config.Default.DataTypes
{
    public class UserData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
