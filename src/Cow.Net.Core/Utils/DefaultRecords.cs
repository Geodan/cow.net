using System;
using System.Collections.ObjectModel;
using Cow.Net.Core.Config.Default.Records;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class DefaultRecords
    {
        public static StoreRecord CreatePeerRecord(ConnectionInfo connectionInfo)
        {
            var record = new PeerRecord
            {
                Id = connectionInfo.PeerId,
                Created = TimeUtils.GetMillisencondsFrom1970(),
                Data = null,
                Deleted = false,
                Deltas = new ObservableCollection<Delta>(),
                Updated = TimeUtils.GetMillisencondsFrom1970(),
                Dirty = true,
            };

            record.Data.Add("userid", null);
            record.Data.Add("family", "alpha");
            record.Data.Add("version", CoreSettings.Instance.Version);

            return record;
        }
    }
}
