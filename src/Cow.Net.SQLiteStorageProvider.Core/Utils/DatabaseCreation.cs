using System;
using System.Collections.Generic;
using SQLitePCL;

namespace Cow.Net.SQLiteStorageProvider.Core.Utils
{
    public class DatabaseCreation
    {
        public static bool Create(string dbLocation, List<string> stores)
        {
            try
            {
                using (var connection = new SQLiteConnection(dbLocation))
                {
                    foreach (var store in stores)
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
                                                {1} TEXT NOT NULL PRIMARY KEY,
                                                {2} BOOLEAN,
                                                {3} BOOLEAN,
                                                {4} BIGINT,
                                                {5} BIGINT,
                                                {6} TEXT,
                                                {7} TEXT,
                                                {8} TEXT);",
                                                table,
                                                Constants.FieldId,
                                                Constants.FieldDirty,
                                                Constants.FieldDeleted,
                                                Constants.FieldCreated,
                                                Constants.FieldUpdated,
                                                Constants.FieldData,
                                                Constants.FieldDeltas,
                                                Constants.FieldProject);
        }
    }
}