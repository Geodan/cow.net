using System;
using System.Collections.ObjectModel;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class DefaultRecords
    {
        public static StoreRecord CreatePeerRecord(ConnectionInfo connectionInfo)
        {
            return new StoreRecord
            {
                Id = connectionInfo.PeerId,
                Created = DateTime.Now.Ticks,
                Data = null,
                Deleted = false,
                Deltas = new ObservableCollection<Delta>(),
                Updated = DateTime.Now.Ticks,
                Dirty = false,
            };
        }
    }
}
