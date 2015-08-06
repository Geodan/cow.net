using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class CommandHandler
    {
        internal static CowMessage<CommandPayload> Handle(string message)
        {
            var commandMessage = JsonConvert.DeserializeObject<CowMessage<CommandPayload>>(message);
            return commandMessage;
        }
    }
}
