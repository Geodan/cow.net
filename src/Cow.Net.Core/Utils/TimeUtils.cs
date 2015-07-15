using System;

namespace Cow.Net.Core.Utils
{
    public class TimeUtils
    {
        public static long GetMillisencondsFrom1970()
        {
            return (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
