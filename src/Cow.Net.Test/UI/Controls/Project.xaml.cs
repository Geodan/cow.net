using System.Windows;
using Cow.Net.Core;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class Project
    {
        private readonly StoreRecord _record;
        private bool _initializing;

        public Project(StoreRecord record)
        {
            _initializing = true;

            InitializeComponent();

            _record = record;
            TxtProjectName.Text = _record.Data["name"].ToString();
            CbDeleted.IsChecked = _record.Deleted;
            _record.PropertyChanged += RecordPropertyChanged;
            _initializing = false;
        }

        private void RecordPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ToLower().Equals("deleted"))
            {
                _initializing = true;
                CbDeleted.IsChecked = _record.Deleted;
                _initializing = false;
            }
        }

        private void CbDeleted_OnChecked(object sender, RoutedEventArgs e)
        {
            if(_initializing)
                return;

            _record.SetDeleted(true);
        }

        private void CbDeleted_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;

            _record.SetDeleted(false);
        }

        private void BtnSaveOnClick(object sender, RoutedEventArgs e)
        {
            _record.Sync(CoreSettings.Instance.CurrentUser != null ? CoreSettings.Instance.CurrentUser.Id : null);
        }
    }
}
