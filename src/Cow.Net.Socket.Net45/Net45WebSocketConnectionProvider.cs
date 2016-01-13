using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cow.Net.Core.Socket;
using WebSocket4Net;

namespace Cow.Net.Socket.Net45
{
    public class Net45WebSocketConnectionProvider : IWebSocketConnectionProvider
    {
        public event SocketOpenedHandler SocketOpened;
        public event SocketClosedHandler SocketClosed;
        public event SocketErrorHandler SocketError;
        public event SocketMessageReceivedHandler SocketMessageReceived;

        private WebSocket _socketClient;

        public async Task OpenAsync(string host, int port, string[] endpoint, string subProtocol)
        {
            if (_socketClient == null)
            {
                var url = CreateUrl(host, port, endpoint);
                _socketClient = string.IsNullOrEmpty(subProtocol) ? new WebSocket(url) : new WebSocket(url, subProtocol);
                _socketClient.Opened += SocketClientOpened;
                _socketClient.Closed += SocketClientClosed;
                _socketClient.Error += SocketClientError;
                _socketClient.MessageReceived += SocketClientMessageReceived;
            }

            await Task.Run(() =>
            {
                _socketClient.Open();
            });
        }

        private void SocketClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            SocketMessageReceived?.Invoke(this, e.Message);
        }

        private void SocketClientError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            SocketError?.Invoke(this, e.Exception);
        }

        private void SocketClientClosed(object sender, EventArgs e)
        {
            SocketClosed?.Invoke(this, e.ToString());
        }

        private void SocketClientOpened(object sender, EventArgs e)
        {
            SocketOpened?.Invoke(this);
        }

        public async Task CloseAsync()
        {
            await Task.Run(() =>
            {
                _socketClient.Close("Websocket close requested");
            });
        }

        public async Task SendAsync(string message)
        {
            await Task.Run(() =>
            {
                _socketClient.Send(message);
            });
        }

        private string CreateUrl(string host, int port, string[] endpoint)
        {
            var url = $"{host}:{port}";
            return endpoint == null ? url : endpoint.Aggregate(url, (current, str) => $"{current}/{str}");
        }
    }
}
