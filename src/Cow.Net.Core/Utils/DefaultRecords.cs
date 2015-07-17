using System.Collections.ObjectModel;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    internal class DefaultRecords
    {
        internal static StoreRecord CreatePeerRecord(ConnectionInfo connectionInfo, bool isAlphaPeer)
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

            if (isAlphaPeer)
            {
                record.Data.Add("userid", null);
                record.Data.Add("family", "alpha");
                record.Data.Add("version", CoreSettings.Instance.Version);
            }

            return record;
        }
    }
}
