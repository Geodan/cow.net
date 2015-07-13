using System;
using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Action = Cow.Net.Core.Models.Action;

namespace Cow.Net.Core.Utils
{
    public class CowMessageFactory
    {
        /// <summary>
        /// Create a sync message send to the alpha peer
        /// </summary>
        /// <param name="connectionInfo">connection info received from websocket</param>
        /// <param name="syncType"></param>
        /// <param name="storeObjects"></param>
        /// <returns></returns>
        public static CowMessage<Dictionary<string, object>> CreateSyncMessage(ConnectionInfo connectionInfo, SyncType syncType, IEnumerable<StoreRecord> storeObjects, string projectId = null)
        {
            var sendList = storeObjects.Select(storeObject => new Dictionary<string, object>
            {
                {"_id", storeObject.Id}, 
                {"timestamp", storeObject.Updated}, 
                {"deleted", storeObject.Deleted},                
            }).ToList();

            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"list", sendList}
            };

            if (!string.IsNullOrEmpty(projectId))
            {
                payload.Add("project", projectId);
            }

            return new CowMessage<Dictionary<string, object>>
            {
                Action = Action.newList,
                Sender = connectionInfo.PeerId,
                Payload = payload
            };
        }

        public static CowMessage<Dictionary<string, object>> CreateUpdateMessage(ConnectionInfo connectionInfo, SyncType syncType, StoreRecord record)
        {
            var recordInfo = new Dictionary<string, object>
            {
                {"_id", record.Id}, 
                {"created", record.Created},
                {"timestamp", record.Updated}, 
                {"deleted", record.Deleted},                
                {"data", record.Data},
                {"deltas", record.Deltas},
                {"dirty", record.Dirty} 
            };

            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"record", recordInfo},                
            };

            if (!string.IsNullOrEmpty(record.Identifier))
            {
                payload.Add("project", record.Identifier);
            }

            return new CowMessage<Dictionary<string, object>>
            {
                Action = Action.updatedRecord,
                Sender = connectionInfo.PeerId,
                Payload = payload
            };
        }

        public static CowMessage<Dictionary<string, object>> CreateMissingRecordsMessage(ConnectionInfo connectionInfo, SyncType syncType, string project, List<StoreRecord> records, string target = null)
        {
            var itemsList = new List<Dictionary<string, object>>();

            foreach (var storeRecord in records)
            {
                var recordInfo = new Dictionary<string, object>
                {
                    {"_id", storeRecord.Id}, 
                    {"created", storeRecord.Created},
                    {"timestamp", storeRecord.Updated}, 
                    {"deleted", storeRecord.Deleted},                
                    {"data", storeRecord.Data},
                    {"deltas", storeRecord.Deltas},
                    {"dirty", storeRecord.Dirty} 
                };

                itemsList.Add(recordInfo);
            }

            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"items", itemsList.ToArray()},
            };

            if (!string.IsNullOrEmpty(project))
            {
                payload.Add("project", project);
            }

            return new CowMessage<Dictionary<string, object>>
            {
                Action = Action.missingRecords,
                Sender = connectionInfo.PeerId,
                Target = target,
                Payload = payload
            };
        }
    }
}
