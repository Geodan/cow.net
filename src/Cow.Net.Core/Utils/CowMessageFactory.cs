using System.Collections.Generic;
using Cow.Net.Core.Models;

namespace Cow.Net.Core.Utils
{
    public class CowMessageFactory
    {
        /// <summary>
        /// Create a CowMessage for syncing peers, alwways send an empty list
        /// </summary>
        /// <param name="connectionInfo">connection info received from websocket</param>
        /// <param name="syncType"></param>
        /// <param name="storeObjects"></param>
        /// <returns></returns>
        public static CowMessage<NewList> CreateSyncMessage(ConnectionInfo connectionInfo, SyncType syncType, List<StoreObject> storeObjects)
        {
            return new CowMessage<NewList>
            {
                Action = Action.newList,
                Sender = connectionInfo.PeerId,
                Payload = new NewList
                {
                    SyncType = syncType,
                    List = storeObjects
                }
            };
        }
    }
}
