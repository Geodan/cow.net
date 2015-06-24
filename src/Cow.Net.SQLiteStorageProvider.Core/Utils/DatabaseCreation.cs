using System;
using SQLitePCL;

namespace Cow.Net.SQLiteStorageProvider.Core.Utils
{
    public class DatabaseCreation
    {
        public static bool Create(string dbLocation)
        {
            try
            {
                using (var connection = new SQLiteConnection(dbLocation))
                {                    
                    foreach (var store in Constants.GetStores())
                    {
                        using (var statement = connection.Prepare(GetTableCreationSql(store)))
                        {
                            statement.Step();
                        }
                    }
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static string GetTableCreationSql(string table)
        {
            return string.Format(@"CREATE TABLE IF NOT EXISTS {0} (
                                                _id TEXT NOT NULL PRIMARY KEY,
                                                dirty BOOLEAN,
                                                deleted BOOLEAN,
                                                created BIGINT,
                                                updated BIGINT,
                                                data BLOB,
                                                deltas BLOB,
                                                projectid TEXT);",
                                                table);
        }
    }
}