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

        public bool PrepareDatabase()
        {
            return DatabaseCreation.Create(_dbLocation);
        }

        public List<Peer> GetPeers()
        {
            return new List<Peer>();
        }

        public void AddPeers(List<Peer> peers)
        {

        }
    }
}