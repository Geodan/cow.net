using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Config.Default.DataTypes;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;

namespace Cow.Net.Core.Config.Default.Stores
{
    public delegate void ProjectsChangedHandler(object sender, List<ProjectData> newProjects);

    public class ProjectStore : CowStore
    {        
        public ProjectStore(string id, SyncType syncType, IStorageProvider storageProvider, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool supportsDeltas = true)
            : base(id, syncType, storageProvider, subStores, saveToLocalDatabase, supportsDeltas)
        {        
        }
    }
}
