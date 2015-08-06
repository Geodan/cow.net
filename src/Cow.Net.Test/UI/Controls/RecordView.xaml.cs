using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Cow.Net.Core;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class RecordView
    {
        public StoreRecord Record { get; private set; }
        public CowStore Store { get; private set; }
        private bool _initializing;

        public RecordView(CowStore store, StoreRecord record)
        {
            _initializing = true;
            InitializeComponent();

            Store = store;
            Record = record;            
            SetDefaultInfo();

            CbDeleted.IsChecked = !Record.Deleted;

            Record.PropertyChanged += RecordPropertyChanged;
            Record.Deltas.CollectionChanged += Deltas_CollectionChanged;
            if (Record.SubRecordCollection != null)
            {
                foreach (var subrecordCollection in Record.SubRecordCollection)
                {
                    SubStoreList.Children.Add(new StoreView(subrecordCollection.Value){Margin = new Thickness(10,10,10,10)});
                }
            }

            _initializing = false;            
        }

        private void Deltas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetDefaultInfo();
        }

        private void SetDefaultInfo()
        {
            TxtProjectName.Text = Record.Data == null || !Record.Data.Any() || !Record.Data.ContainsKey("name") ? Record.Id : Record.Data["name"].ToString();
            TxtDeltas.Text = string.Format("deltas: {0}", Record.Deltas == null ? "0" : Record.Deltas.Count.ToString());

            var created = Core.Utils.TimeUtils.GetDateTimeFrom1970Milliseconds(Record.Created).ToLocalTime();
            TxtCreated.Text = string.Format("{0} {1}", created.ToShortDateString(), created.ToShortTimeString());

            if (Record.Created != Record.Updated)
            {
                var updated = Core.Utils.TimeUtils.GetDateTimeFrom1970Milliseconds(Record.Updated).ToLocalTime();
                TxtUpdated.Text = string.Format("updated {0} {1}", updated.ToShortDateString(), updated.ToShortTimeString());
            }
        }

        private void RecordPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ToLower().Equals("deleted"))
            {
                _initializing = true;
                CbDeleted.IsChecked = !Record.Deleted;
                _initializing = false;
            }

            SetDefaultInfo();
        }

        private void CbDeleted_OnChecked(object sender, RoutedEventArgs e)
        {
            if(_initializing)
                return;

            Record.SetDeleted(false);
            SyncDeleted();
        }

        private void CbDeleted_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;

            Record.SetDeleted(true);
            SyncDeleted();
        }

        private void SyncDeleted()
        {
            Record.Sync();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rw = new RecordWindow(Store, Record);
            rw.Show();
            rw.Activate();
            e.Handled = true;
        }
    }
}
