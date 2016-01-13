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
            _client.CowSocketMessageReceived += _client_CowSocketMessageReceived1;
            TxtMessages.Text = "Websocket message log:";
        }

        private void _client_CowSocketMessageReceived1(object sender, string message)
        {
            TxtMessages.Text += $"\n{_count}: {message}";
            _count++;
        }
    }
}
