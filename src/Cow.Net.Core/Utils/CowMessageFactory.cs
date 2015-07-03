using System.Collections.Generic;
using System.Linq;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class CowMessageFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionInfo">connection info received from websocket</param>
        /// <param name="syncType"></param>
        /// <param name="storeObjects"></param>
        /// <returns></returns>
        public static CowMessage<Dictionary<string, object>> CreateSyncMessage(ConnectionInfo connectionInfo, string syncType, IEnumerable<StoreRecord> storeObjects, string projectId = null)
        {
            var sendList = storeObjects.Select(storeObject => new Dictionary<string, object>
            {
                {"_id", storeObject.Id}, 
                {"timestamp", storeObject.Updated}, 
                {"deleted", storeObject.Deleted},                
            }).ToList();

            var payload = new Dictionary<string, object>
            {
                {"syncType", syncType},
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

        public static CowMessage<Dictionary<string, object>> CreateUpdateMessage(ConnectionInfo connectionInfo, string syncType, StoreRecord record)
        {
            var recordInfo = new Dictionary<string, object>
            {
                {"_id", record.Id}, 
                {"timestamp", record.Updated}, 
                {"deleted", record.Deleted},
            };

            var payload = new Dictionary<string, object>
            {
                {"syncType", syncType},
                {"record", recordInfo}
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
    }
}
