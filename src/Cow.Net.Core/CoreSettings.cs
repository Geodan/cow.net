using System.Collections.Generic;
using System.Threading;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cow.Net.Core
{
    internal class CoreSettings
    {
        private static CoreSettings _instance;

        public readonly string Version = "2.0.5";
        public readonly string SupportedServerVerion = "0.1";
        public bool LocalStorageAvailable;
        public SynchronizationContext SynchronizationContext;
        public ConnectionInfo ConnectionInfo;               
        public StoreRecord CurrentUser { get; set; }

        public readonly JsonSerializerSettings SerializerSettingsIncoming = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, 
            TypeNameHandling = TypeNameHandling.Objects, 
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    CamelCaseText = true
                }, new LZWConverter()
            }
        };

        public readonly JsonSerializerSettings SerializerSettingsOutgoing = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,            
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    CamelCaseText = true
                }, new LZWConverter()
            }
        };   

        private CoreSettings() { }

        public static CoreSettings Instance
        {
           get { return _instance ?? (_instance = new CoreSettings()); }
        }
    }
}
