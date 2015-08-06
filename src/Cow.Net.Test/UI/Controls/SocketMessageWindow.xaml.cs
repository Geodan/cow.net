using Cow.Net.Core;

namespace Cow.Net.test.UI.Controls
{
    public partial class SocketMessageWindow
    {
        private readonly CowClient _client;
        private int _count;

        public SocketMessageWindow(CowClient cowClient)
        {
            InitializeComponent();
            _client = cowClient;
            _client.CowSocketMessageReceived += _client_CowSocketMessageReceived;
            TxtMessages.Text = "Websocket message log:";
        }

        void _client_CowSocketMessageReceived(object sender, WebSocketSharp.MessageEventArgs message)
        {
            TxtMessages.Text += string.Format("\n{0}: {1}", _count, message.Data);
            _count++;
        }
    }
}
