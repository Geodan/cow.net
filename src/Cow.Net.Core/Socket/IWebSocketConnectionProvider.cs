using System;
using System.Threading.Tasks;

namespace Cow.Net.Core.Socket
{
    public delegate void SocketOpenedHandler(object sender);
    public delegate void SocketClosedHandler(object sender, string message);
    public delegate void SocketErrorHandler(object sender, Exception e);
    public delegate void SocketMessageReceivedHandler(object sender, string message);

    public interface IWebSocketConnectionProvider
    {
        Task OpenAsync(string host, int port, string[] endpoint, string subProtocol);
        Task CloseAsync();
        Task SendAsync(string message);

        event SocketOpenedHandler SocketOpened;
        event SocketClosedHandler SocketClosed;
        event SocketErrorHandler SocketError;
        event SocketMessageReceivedHandler SocketMessageReceived;
    }
}
