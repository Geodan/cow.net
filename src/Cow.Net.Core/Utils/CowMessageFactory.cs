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
        /// <param name="peers">peers to send</param>
        /// <returns></returns>
        public static CowMessage<NewList<List<Peer>>> CreatePeersMessage(ConnectionInfo connectionInfo, List<Peer> peers)
        {
            return new CowMessage<NewList<List<Peer>>>
            {
                Action = Action.newList,
                Sender = connectionInfo.PeerId,
                Payload = new NewList<List<Peer>>
                {
                    SyncType = SyncType.peers,
                    List = peers
                }
            };
        }
    }
}
