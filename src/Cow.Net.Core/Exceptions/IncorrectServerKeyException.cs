using System;

namespace Cow.Net.Core.Exceptions
{
    public class IncorrectServerKeyException : Exception
    {
        public IncorrectServerKeyException(string configKey, string serverKey) : base(string.Format("Given server key \"{0}\" is incorrect, server uses \"{1}\"", configKey, serverKey)){}
    }
}
