using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class ConnectedHandler
    {
        internal static ConnectionInfo Handle(string message)
        {
            var connected = JsonConvert.DeserializeObject<CowMessage<ConnectionInfo>>(message);
            return connected.Payload;
        }
    }
}
