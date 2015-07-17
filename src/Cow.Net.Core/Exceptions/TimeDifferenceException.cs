using System;

namespace Cow.Net.Core.Exceptions
{
    public class TimeDifferenceException : Exception
    {
        public TimeDifferenceException(DateTime serverTime, DateTime clientTime) :
            base(string.Format("Client DateTime: {0} differs too much from Server {1}", serverTime.ToUniversalTime(), clientTime.ToUniversalTime()))
        {
            
        }
    }
}
