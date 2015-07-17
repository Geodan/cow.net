using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    internal class CommandHandler
    {
        internal static CowMessage<Command> Handle(string message)
        {
            var commandMessage = JsonConvert.DeserializeObject<CowMessage<Command>>(message);
            return commandMessage;
        }
    }
}
