using System.Collections.Generic;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;
using Cow.Net.SQLiteStorageProvider.Core.Utils;

namespace Cow.Net.SQLiteStorageProvider.Core
{
    public class SQLiteStorageProvider : IStorageProvider
    {
        private readonly string _dbLocation;

        /// <summary>
        /// Create a SQLite data provider for 
        /// </summary>
        /// <param name="dbLocation"></param>
        public SQLiteStorageProvider(string dbLocation)
        {
            _dbLocation = dbLocation;
        }

        public bool PrepareDatabase(List<string> stores)
        {
            return DatabaseCreation.Create(_dbLocation, stores);
        }

        public List<StoreRecord> GetStoreObjects()
        {
            return null;
        }

        public void AddStoreObject(List<StoreRecord> peers)
        {
            
        }

        public void UpdateStoreObject(List<StoreRecord> peers)
        {

        }
    }
}