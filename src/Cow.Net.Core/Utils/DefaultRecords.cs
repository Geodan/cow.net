using System.Collections.ObjectModel;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    internal class DefaultRecords
    {
        internal static StoreRecord CreatePeerRecord(ConnectionInfo connectionInfo, bool isAlphaPeer, StoreRecord user, StoreRecord activeProject)
        {
            var record = new StoreRecord
            {
                Id = connectionInfo.PeerId,
                Created = TimeUtils.GetMillisencondsFrom1970(),
                Data = null,
                Deleted = false,
                Deltas = new ObservableCollection<Delta>(),
                Updated = TimeUtils.GetMillisencondsFrom1970(),
                Dirty = false,
            };

            if(user != null)
                record.Data.Add("userid", user.Id);

            if(activeProject != null)
                record.Data.Add("activeproject", null);

            if (isAlphaPeer)
            {                
                record.Data.Add("family", "alpha");
                record.Data.Add("version", CoreSettings.Instance.Version);
            }

            return record;
        }
    }
}
