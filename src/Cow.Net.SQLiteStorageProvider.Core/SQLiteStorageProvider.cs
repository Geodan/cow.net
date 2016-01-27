using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Cow.Net.Core.Models;
using Cow.Net.Core.Storage;
using Cow.Net.Core.Utils;
using Cow.Net.SQLiteStorageProvider.Core.Utils;
using Newtonsoft.Json;
using SQLitePCL;

namespace Cow.Net.SQLiteStorageProvider.Core
{
    public class SQLiteStorageProvider : IStorageProvider
    {
        private readonly Object _lockObject = new Object();
        private readonly string _dbLocation;
        private bool _isWorking;        

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
            lock (_lockObject)
            {
                _isWorking = DatabaseCreation.Create(_dbLocation, stores);
                return _isWorking;
            }
        }

        public bool HasRecord(string storeId, string recordId)
        {
            lock (_lockObject)
            {
                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (
                        var statement =
                            connection.Prepare(string.Format(@"SELECT * FROM {0} WHERE {1} = '{2}'", storeId,
                                Constants.FieldId, recordId)))
                    {
                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool HasNonOutdatedLinkedStoreRecords(string storeId, string identifier, long maxDataAge)
        {
            lock (_lockObject)
            {
                var now = TimeUtils.GetMillisencondsFrom1970();

                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (
                        var statement =
                            connection.Prepare(string.Format(
                                @"SELECT * FROM {0} WHERE {1} = '{2}' AND {3} - {4} < {5}", storeId,
                                Constants.FieldProject, identifier, now, Constants.FieldUpdated, maxDataAge)))
                    {
                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public List<StoreRecord> GetStoreRecords(string storeId)
        {
            lock (_lockObject)
            {
                var records = new List<StoreRecord>();

                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (var statement = connection.Prepare(string.Format(@"SELECT * FROM {0}", storeId)))
                    {
                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            var id = (string) statement[Constants.FieldId];
                            var dirty = Convert.ToBoolean((Int64) statement[Constants.FieldDirty]);
                            var deleted = Convert.ToBoolean((Int64) statement[Constants.FieldDeleted]);
                            var created = (long) statement[Constants.FieldCreated];
                            var updated = (long) statement[Constants.FieldUpdated];
                            var data =
                                JsonConvert.DeserializeObject<ObservableDictionary<string, object>>(
                                    (string) statement[Constants.FieldData]);
                            var deltas =
                                JsonConvert.DeserializeObject<ObservableCollection<Delta>>(
                                    (string) statement[Constants.FieldDeltas]);
                            var project = (string) statement[Constants.FieldProject];

                            records.Add(new StoreRecord(id, project, created, deleted, updated, dirty, data, deltas));
                        }
                    }
                }

                return records;
            }
        }

        public void AddStoreObjects(string storeId, List<StoreRecord> records)
        {
            lock (_lockObject)
            {
                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (
                        var statement =
                            connection.Prepare(
                                string.Format(
                                    @"INSERT OR IGNORE INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES(@{1}, @{2}, @{3}, @{4}, @{5}, @{6}, @{7}, @{8});",
                                    storeId,
                                    Constants.FieldId,
                                    Constants.FieldDirty,
                                    Constants.FieldDeleted,
                                    Constants.FieldCreated,
                                    Constants.FieldUpdated,
                                    Constants.FieldData,
                                    Constants.FieldDeltas,
                                    Constants.FieldProject)))
                    {
                        foreach (var storeRecord in records)
                        {
                            try
                            {
                                statement.Bind(string.Format("@{0}", Constants.FieldId), storeRecord.Id);
                                statement.Bind(string.Format("@{0}", Constants.FieldDirty),
                                    Convert.ToInt64(storeRecord.Dirty));
                                statement.Bind(string.Format("@{0}", Constants.FieldDeleted),
                                    Convert.ToInt64(storeRecord.Deleted));
                                statement.Bind(string.Format("@{0}", Constants.FieldCreated), storeRecord.Created);
                                statement.Bind(string.Format("@{0}", Constants.FieldUpdated), storeRecord.Updated);
                                statement.Bind(string.Format("@{0}", Constants.FieldData),
                                    JsonConvert.SerializeObject(storeRecord.Data));
                                statement.Bind(string.Format("@{0}", Constants.FieldDeltas),
                                    JsonConvert.SerializeObject(storeRecord.Deltas));
                                statement.Bind(string.Format("@{0}", Constants.FieldProject), storeRecord.Identifier);
                                statement.Step();
                            }
                            catch (Exception)
                            {

                            }

                            statement.Reset();
                            statement.ClearBindings();
                        }
                    }
                }
            }
        }

        public void UpdateStoreObjects(string storeId, List<StoreRecord> records)
        {
            lock (_lockObject)
            {
                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (
                        var statement =
                            connection.Prepare(
                                string.Format(
                                    @"UPDATE {0} SET {1} = @{1}, {2} = @{2}, {3} = @{3}, {4} = @{4}, {5} = @{5}, {6} = @{6}, {7} = @{7}, {8} = @{8} WHERE {1} = @{1};",
                                    storeId,
                                    Constants.FieldId,
                                    Constants.FieldDirty,
                                    Constants.FieldDeleted,
                                    Constants.FieldCreated,
                                    Constants.FieldUpdated,
                                    Constants.FieldData,
                                    Constants.FieldDeltas,
                                    Constants.FieldProject)))
                    {
                        foreach (var storeRecord in records)
                        {
                            try
                            {
                                statement.Bind(string.Format("@{0}", Constants.FieldId), storeRecord.Id);
                                statement.Bind(string.Format("@{0}", Constants.FieldDirty),
                                    Convert.ToInt64(storeRecord.Dirty));
                                statement.Bind(string.Format("@{0}", Constants.FieldDeleted),
                                    Convert.ToInt64(storeRecord.Deleted));
                                statement.Bind(string.Format("@{0}", Constants.FieldCreated), storeRecord.Created);
                                statement.Bind(string.Format("@{0}", Constants.FieldUpdated), storeRecord.Updated);
                                statement.Bind(string.Format("@{0}", Constants.FieldData),
                                    JsonConvert.SerializeObject(storeRecord.Data));
                                statement.Bind(string.Format("@{0}", Constants.FieldDeltas),
                                    JsonConvert.SerializeObject(storeRecord.Deltas));
                                statement.Bind(string.Format("@{0}", Constants.FieldProject), storeRecord.Identifier);
                                statement.Step();
                            }
                            catch (Exception)
                            {

                            }

                            statement.Reset();
                            statement.ClearBindings();
                        }
                    }
                }
            }
        }

        public void RemoveObjects(string storeId, List<StoreRecord> records)
        {
            lock (_lockObject)
            {
                using (var connection = new SQLiteConnection(_dbLocation))
                {
                    using (var statement = connection.Prepare(string.Format(@"DELETE FROM {0} WHERE {1} = @{2};", storeId, Constants.FieldId, "recordid")))
                    {
                        foreach (var storeRecord in records)
                        {
                            try
                            {
                                statement.Bind("@recordid", storeRecord.Id);
                                statement.Step();
                            }
                            catch (Exception)
                            {

                            }

                            statement.Reset();
                            statement.ClearBindings();
                        }
                    }
                }
            }
        }
    }
}