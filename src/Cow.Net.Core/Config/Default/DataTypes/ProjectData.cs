using Newtonsoft.Json;

namespace Cow.Net.Core.Config.Default.DataTypes
{
    public class ProjectData
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }

        [JsonProperty("incidentlocation")]
        public IncidentLocation IncidentLocation { get; set; }
    }

    public class Status
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Type
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class IncidentLocation
    {
        [JsonProperty("lat")]
        public int Latitude { get; set; }

        [JsonProperty("lng")]
        public string Longitude { get; set; }

        [JsonProperty("zoom")]
        public string Zoom { get; set; }
    }
}
