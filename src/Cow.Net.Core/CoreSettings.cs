using System.Collections.Generic;
using System.Threading;
using Cow.Net.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cow.Net.Core
{
    public class CoreSettings
    {
        private static CoreSettings _instance;
        public SynchronizationContext SynchronizationContext;
        public ConnectionInfo ConnectionInfo;
        public readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } } };        

        private CoreSettings() { }

        public static CoreSettings Instance
        {
           get { return _instance ?? (_instance = new CoreSettings()); }
        }
    }
}
