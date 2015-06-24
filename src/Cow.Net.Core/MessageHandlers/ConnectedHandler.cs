using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class ConnectedHandler
    {
        public static ConnectionInfo Handle(string message)
        {
            var connected = JsonConvert.DeserializeObject<CowMessage<ConnectionInfo>>(message);
            return connected.Payload;
        }
    }
}
