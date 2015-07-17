using System;

namespace Cow.Net.Core.Exceptions
{
    public class IncorrectCowServerVersion : Exception
    {
        public IncorrectCowServerVersion(string serverVersion, string clientServerVersion) :
            base(string.Format("This client version supports COW server {0}, current server version = {1}",
            clientServerVersion, serverVersion)) { }
    }
}
