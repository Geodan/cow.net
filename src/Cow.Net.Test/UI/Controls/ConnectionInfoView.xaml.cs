using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cow.Net.Core.Annotations;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class ConnectionInfoView : INotifyPropertyChanged
    {
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

        public ConnectionInfoView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
