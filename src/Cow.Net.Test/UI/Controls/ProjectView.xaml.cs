using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Cow.Net.Core.Config.Default.DataTypes;
using Cow.Net.Core.Config.Default.Stores;
using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.test.UI.Controls
{
    public partial class ProjectView
    {
        public ProjectStore Projects { get; set; }

        public ProjectView()
        {
            InitializeComponent();
        }

        public void SetProjects(ProjectStore projects)
        {
            Projects = projects;
            UpdateList(Projects.Records);
            Projects.CollectionChanged += PeersCollectionChanged;
        }

        private void PeersCollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            UpdateList(newRecords);
        }

        private void UpdateList(IEnumerable<StoreRecord> records)
        {
            foreach (var storeRecord in records)
            {                
                if (storeRecord.Deleted)
                    continue;
                
                var data = storeRecord.GetData<ProjectData>();
                var txtBlock = new TextBlock { Text = data.Name, Tag = storeRecord, Foreground = new SolidColorBrush(Colors.Black) };
                ProjectList.Items.Add(txtBlock);
            }
        }

        public class ProjectData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("location")]
            public Location IncidentLocation { get; set; }

            [JsonProperty("teams")]
            public Team[] Teams { get; set; }            
        }

        public class Location
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }

            [JsonProperty("lon")]
            public double Longitude { get; set; }

            [JsonProperty("zoomlevel")]
            public int Zoom { get; set; }
        }

        public class Team
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("boxes")]
            public Box[] Boxes { get; set; }
        }

        public class Box
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}
