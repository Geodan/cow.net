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
        internal static CowMessage<DictionaryPayload> CreateSyncMessage(ConnectionInfo connectionInfo, SyncType syncType, IEnumerable<StoreRecord> storeObjects, string projectId = null)
        {
            var objectToSync = string.IsNullOrEmpty(projectId) ? storeObjects : storeObjects.Where(so => so.Identifier.Equals(projectId));
            var sendList = objectToSync.Select(storeObject => new Dictionary<string, object>
            {
                {"_id", storeObject.Id}, 
                {"updated", storeObject.Updated}, 
                {"deleted", storeObject.Deleted},                
            }).ToList();

            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"list", sendList}
            };

            if (!string.IsNullOrEmpty(projectId))
            {
                payload.Add("project", projectId);
            }

            return new CowMessage<DictionaryPayload>
            {
                Sender = connectionInfo.PeerId,
                Action = Action.newList,                
                Payload = payload
            };
        }

        internal static CowMessage<DictionaryPayload> CreateUpdateMessage(ConnectionInfo connectionInfo, SyncType syncType, StoreRecord record)
        {
            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"record", record},                
            };

            if (!string.IsNullOrEmpty(record.Identifier))
            {
                payload.Add("project", record.Identifier);
            }

            return new CowMessage<DictionaryPayload>
            {
                Sender = connectionInfo.PeerId,
                Action = Action.updatedRecord,                
                Payload = payload
            };
        }

        internal static CowMessage<DictionaryPayload> CreateSyncInfoMessage(SyncType syncType, List<StoreRecord> pushRecords, List<StoreRecord> requestRecords, string projectId, string sender, string target)
        {
            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
                {"syncinfo", new SyncInfo
                    {
                        WillSent = pushRecords.Select(storeRecord => storeRecord.Id).ToList(),
                        ShallReceive = requestRecords.Select(storeRecord => storeRecord.Id).ToList()
                    }
                }
            };

            if(!string.IsNullOrEmpty(projectId))
                payload.Add("project", projectId);

            return new CowMessage<DictionaryPayload>
            {
                Sender = sender,
                Target = target,
                Action = Action.syncinfo,
                Payload = payload
            };
        }

        internal static CowMessage<DictionaryPayload> CreateWantedMessage(SyncType syncType, List<StoreRecord> records, string projectId, string sender, string target)
        {
            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
            };

            if(records.Any())
                payload.Add("list", records.Select(storeRecord => storeRecord.Id).ToList());

            if (!string.IsNullOrEmpty(projectId))
                payload.Add("project", projectId);

            return new CowMessage<DictionaryPayload>
            {
                Sender = sender,
                Target = target,
                Action = Action.wantedList,
                Payload = payload
            };
        }

        internal static CowMessage<DictionaryPayload> CreateRequestedMessage(SyncType syncType, StoreRecord record, string projectId, string sender)
        {
            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},                
                {"record", record}
            };                            

            if (!string.IsNullOrEmpty(projectId))
                payload.Add("project", projectId);

            return new CowMessage<DictionaryPayload>
            {
                Sender = sender,                
                Action = Action.requestedRecord,
                Payload = payload
            };
        }

        internal static CowMessage<DictionaryPayload> CreateMissingRecordMessage(SyncType syncType, StoreRecord record, string projectId, string sender, string target = null)
        {
            var payload = new DictionaryPayload
            {
                {"syncType", Enum.GetName(typeof(SyncType), syncType)},
                {"record", record},
            };

            if (!string.IsNullOrEmpty(projectId))            
                payload.Add("project", projectId);

            return new CowMessage<DictionaryPayload>
            {
                Sender = sender,
                Target = target,
                Action = Action.missingRecord,                                
                Payload = payload
            };
        }
    }
}
