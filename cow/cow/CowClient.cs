using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using cow.core.Annotations;
using cow.core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Action = cow.core.Models.Action;

namespace cow.core
{
    public class CowClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly WebSocketSharp.WebSocket _socketClient;
                
        private ConnectionInfo _connectionInfo;        
        public ConnectionInfo ConnectionInfo
        {
            get { return _connectionInfo; }
            set
            {
                _connectionInfo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="protocols"></param>
        public CowClient(string address, string[] protocols = null)
        {
            _socketClient = new WebSocketSharp.WebSocket(address, protocols ?? new[] { "connect" });
            _socketClient.OnError += SocketClientOnError;
            _socketClient.OnOpen += SocketClientOnOpen;
            _socketClient.OnClose += SocketClientOnClose;
            _socketClient.OnMessage += SocketClientOnMessage;
        }

        public void Connect()
        {
            _socketClient.ConnectAsync();
        }

        private async void Sync()
        {
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            serializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            //Test sending peers
            var peersMessage = new CowMessage<NewList<string>>
            {
                Action = Action.newList,
                Sender = ConnectionInfo.PeerId,
                Payload = new NewList<string>
                {
                    SyncType = SyncType.peers, 
                    List = new List<string>()
                }
            };

            _socketClient.Send(JsonConvert.SerializeObject(peersMessage, Formatting.None, serializerSettings));
        }

        private void HandleReceivedMessage(string message)
        {
            dynamic d = JObject.Parse(message);

            var action = Action.Unknown;
            if(d.action == null || !Enum.TryParse((string)d.action, out action))
                return;

            switch (action)
            {
                case Action.connected:
                    HandleConnectedMessage(message);
                    break;
            }                       
        }

        private void HandleConnectedMessage(string message)
        {
            var connected = JsonConvert.DeserializeObject<CowMessage<ConnectionInfo>>(message);
            ConnectionInfo = connected.Payload;

            //Connection info received, load local data and start sync
            Sync();
        }

        private void SocketClientOnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            HandleReceivedMessage(e.Data);
        }

        private void SocketClientOnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            
        }

        private void SocketClientOnOpen(object sender, EventArgs e)
        {
            
        }

        private void SocketClientOnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
