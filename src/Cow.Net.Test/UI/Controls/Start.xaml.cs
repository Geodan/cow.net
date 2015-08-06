using System.Windows;

namespace Cow.Net.test.UI.Controls
{
    public delegate void StartClickedHandler(object sender);

    public partial class Start
    {
        public event StartClickedHandler StartClicked;        

        public Start()
        {
            InitializeComponent();

            TxtServer.Text = CowSettings.Instance.Server;
            TxtKey.Text = CowSettings.Instance.ServerKey;
            TxtDatabase.Text = CowSettings.Instance.DatabaseLocation;
            CbAlpha.IsChecked = CowSettings.Instance.IsAlpha;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            CowSettings.Instance.Server = TxtServer.Text;
            CowSettings.Instance.ServerKey = TxtKey.Text;
            CowSettings.Instance.DatabaseLocation = TxtDatabase.Text;
            CowSettings.Instance.IsAlpha = CbAlpha.IsChecked.HasValue && CbAlpha.IsChecked.Value;

            OnStartClicked();
        }

        protected virtual void OnStartClicked()
        {
            var handler = StartClicked;
            if (handler != null) handler(this);
        }
    }
}
