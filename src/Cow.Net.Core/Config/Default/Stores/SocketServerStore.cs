﻿using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Config.Default.Stores
{
    public class SocketServerStore : CowStore
    {
        public SocketServerStore(string id, SyncType syncType, IEnumerable<CowStore> subStores = null, bool saveToLocalDatabase = true, bool createDeltas = true)
            : base(id, syncType, subStores, saveToLocalDatabase, createDeltas)
        {
            CollectionChanged += SocketServerStore_CollectionChanged;
        }

        void SocketServerStore_CollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            
        }
    }
}
