using Cow.Net.Core.Models;
using Newtonsoft.Json;

namespace Cow.Net.Core.MessageHandlers
{
    public class CommandHandler
    {
        public static CowMessage<Command> Handle(string message)
        {
            var commandMessage = JsonConvert.DeserializeObject<CowMessage<Command>>(message);
            return commandMessage;
        }
    }
}
