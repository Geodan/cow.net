using System;
using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;
using Action = Cow.Net.Core.Models.Action;

namespace Cow.Net.Core.Utils
{
    internal class CowMessageFactory
    {
        /// <summary>
        /// Create a sync message send to the alpha peer
        /// </summary>
        /// <param name="connectionInfo">connection info received from websocket</param>
        /// <param name="syncType"></param>
        /// <param name="storeObjects"></param>
        /// <returns></returns>
        internal static CowMessage<Dictionary<string, object>> CreateSyncMessage(ConnectionInfo connectionInfo, SyncType syncType, IEnumerable<StoreRecord> storeObjects, string projectId = null)
        {
            var sendList = storeObjects.Select(storeObject => new Dictionary<string, object>
            {
                {"_id", storeObject.Id}, 
                {"updated", storeObject.Updated}, 
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
                Sender = connectionInfo.PeerId,
                Action = Action.newList,                
                Payload = payload
            };
        }

        internal static CowMessage<Dictionary<string, object>> CreateUpdateMessage(ConnectionInfo connectionInfo, SyncType syncType, StoreRecord record)
        {
            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"record", record},                
            };

            if (!string.IsNullOrEmpty(record.Identifier))
            {
                payload.Add("project", record.Identifier);
            }

            return new CowMessage<Dictionary<string, object>>
            {
                Sender = connectionInfo.PeerId,
                Action = Action.updatedRecord,                
                Payload = payload
            };
        }

        internal static CowMessage<Dictionary<string, object>> CreateSyncInfoMessage(SyncType syncType, List<StoreRecord> pushRecords, List<StoreRecord> requestRecords, string project, string sender, string target)
        {
            var info = new Dictionary<string, object> 
            {
                {"IWillSent", pushRecords.Select(storeRecord => storeRecord.Id).ToList()}, 
                {"IShallReceive", requestRecords.Select(storeRecord => storeRecord.Id).ToList()}
            };

            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
                {"syncinfo", info},
            };

            if(!string.IsNullOrEmpty(project))
                payload.Add("project", project);

            return new CowMessage<Dictionary<string, object>>
            {
                Sender = sender,
                Target = target,
                Action = Action.syncinfo,
                Payload = payload
            };
        }

        internal static CowMessage<Dictionary<string, object>> CreateWantedMessage(SyncType syncType, List<StoreRecord> records, string project, string sender, string target)
        {
            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
            };

            if(records.Any())
                payload.Add("list", records.Select(storeRecord => storeRecord.Id).ToList());

            if (!string.IsNullOrEmpty(project))
                payload.Add("project", project);

            return new CowMessage<Dictionary<string, object>>
            {
                Sender = sender,
                Target = target,
                Action = Action.wantedList,
                Payload = payload
            };
        }

        internal static CowMessage<Dictionary<string, object>> CreateRequestedMessage(SyncType syncType, List<StoreRecord> records, string project, string sender)
        {
            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
                {"list", records}
            };                            

            if (!string.IsNullOrEmpty(project))
                payload.Add("project", project);

            return new CowMessage<Dictionary<string, object>>
            {
                Sender = sender,                
                Action = Action.requestedRecords,
                Payload = payload
            };
        }

        internal static CowMessage<Dictionary<string, object>> CreateMissingRecordsMessage(SyncType syncType, List<StoreRecord> records, string project, string sender, string target = null)
        {
            var payload = new Dictionary<string, object>
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"list", records},
            };

            if (!string.IsNullOrEmpty(project))            
                payload.Add("project", project);            

            return new CowMessage<Dictionary<string, object>>
            {
                Sender = sender,
                Target = target,
                Action = Action.missingRecords,                                
                Payload = payload
            };
        }
    }
}
