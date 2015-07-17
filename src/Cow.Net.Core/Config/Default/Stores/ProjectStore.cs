using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Config.Default.DataTypes;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Config.Default.Stores
{
    public delegate void ProjectsChangedHandler(object sender, List<ProjectData> newProjects);

    public class ProjectStore : CowStore
    {        
        public event ProjectsChangedHandler ProjectsChanged;

        public ProjectStore(string id, SyncType syncType, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool createDeltas = true)
            : base(id, syncType, subStores, saveToLocalDatabase, createDeltas)
        {
            CollectionChanged += ProjectStoreCollectionChanged;            
        }

        private void ProjectStoreCollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            var projects = new List<ProjectData>();
            foreach (var record in newRecords)
            {
                try
                {
                    //var data = record.GetData<ProjectData>();
                    //projects.Add(data);
                }
                catch{}
            }

            if(!projects.Any())
                return;

            OnProjectsChanged(projects);
        }

        protected virtual void OnProjectsChanged(List<ProjectData> newprojects)
        {
            var handler = ProjectsChanged;
            if (handler != null) handler(this, newprojects);
        }
    }
}
