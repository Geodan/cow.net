using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cow.Net.Core;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class StoreView
    {
        private readonly CowStore _store;
        private bool _collapsed;

        public StoreView(CowStore store)
        {
            InitializeComponent();
            _store = store;
            Initialize();
        }

        private void Initialize()
        {
            _store.StoreSynced += StoreSynced;
            TxtStoreName.Text = _store.Id;
            _store.CollectionChanged += StoreCollectionChanged;
            StoreCollectionChanged(this, _store.Records.ToList(), new List<StoreRecord>());
            CbShowDeleted_OnUnchecked(this, null);

            if (_store.IsLinkedStore)
            {
                BgRect.Fill = new SolidColorBrush(Color.FromArgb(255, 61, 60, 60));
            }

            Collapsed = _store.IsLinkedStore;
        }

        public bool Collapsed
        {
            get { return _collapsed; }
            set
            {
                _collapsed = value;
                SetCollapsedState();
            }
        }

        private void StoreSynced(object sender, string project)
        {
            CbSynced.IsChecked = _store.Synced;
        }

        private void StoreCollectionChanged(object sender, List<StoreRecord> newRecords, List<StoreRecord> deletedRecords)
        {
            foreach (var storeRecord in newRecords)
            {
                var rv = new RecordView(_store, storeRecord) { Tag = storeRecord };
                if (storeRecord.Deleted && !CbShowDeleted.IsChecked.Value)
                    rv.Visibility = Visibility.Collapsed;

                RecordsList.Children.Add(rv);
            }

            if (deletedRecords != null)
            {
                foreach (var deletedRecord in deletedRecords)
                {
                    foreach (var child in RecordsList.Children)
                    {
                        var rv = (RecordView) child;
                        if (rv == null || rv.Record != deletedRecord)
                            continue;

                        RecordsList.Children.Remove((RecordView) child);
                        break;
                    }
                }
            }

            TxtItems.Text = string.Format("({0})", _store.Records.Count);
        }

        private void CbShowDeleted_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var child in RecordsList.Children)
            {
                var rv = (RecordView)child;
                if (rv == null)
                    continue;

                rv.Visibility = Visibility.Visible;
            }
        }

        private void CbShowDeleted_OnUnchecked(object sender, RoutedEventArgs e)
        {
            foreach (var child in RecordsList.Children)
            {
                var rv = (RecordView)child;
                if (rv == null || !rv.Record.Deleted)
                    continue;

                rv.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonBase_OnClick_(object sender, RoutedEventArgs e)
        {
            var addEdit = new RecordWindow(_store);
            addEdit.Show();
        }

        private void ChangeCollapsed(object sender, MouseButtonEventArgs e)
        {
            Collapsed = !Collapsed;
            e.Handled = true;
        }

        private void SetCollapsedState()
        {
            if (Collapsed)
                SetCollapsed();
            else
                SetOpen();    
        }

        public void SetCollapsed()
        {
            ImgCollapsibleDown.Visibility = Visibility.Visible;
            ImgCollapsibleUp.Visibility = Visibility.Collapsed;
            RecordsList.Visibility = Visibility.Collapsed;
        }

        public void SetOpen()
        {
            ImgCollapsibleDown.Visibility = Visibility.Collapsed;
            ImgCollapsibleUp.Visibility = Visibility.Visible;
            RecordsList.Visibility = Visibility.Visible;
        }
    }
}
