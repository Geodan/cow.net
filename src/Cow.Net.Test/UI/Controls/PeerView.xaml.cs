using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Cow.Net.Core;
using Cow.Net.Core.Config.Default.DataTypes;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class PeerView
    {
        public CowStore Peers { get; set; }

        public PeerView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetPeers(CowStore peers)
        {
            Peers = peers;
            UpdateList(Peers.Records);
            Peers.CollectionChanged += PeersCollectionChanged;
        }

        private void PeersCollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            UpdateList(newRecords);
        }

        private void UpdateList(IEnumerable<StoreRecord> records)
        {
            foreach (var storeRecord in records)
            {
                storeRecord.PropertyChanged += StoreRecordPropertyChanged;
                if(storeRecord.Deleted)
                    continue;

                var txtBlock = new TextBlock { Text = storeRecord.Id, Tag = storeRecord, Foreground = new SolidColorBrush(Colors.Black) };
                PeersList.Items.Add(txtBlock);
            }
        }

        private void StoreRecordPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var record = sender as StoreRecord;
            if (record == null)
                return;

            if (e.PropertyName.ToLower().Equals("data"))
            {
                foreach (var item in PeersList.Items)
                {
                    var r = item as TextBlock;
                    if (r.Tag != record) continue;

                    r.Text = record.Id;
                    break;
                }
            }

            if (!e.PropertyName.ToLower().Equals("deleted"))
                return;

            if (record.Deleted)
            {
                foreach (var item in PeersList.Items)
                {
                    var r = item as TextBlock;
                    if (r.Tag != record) continue;
                    PeersList.Items.Remove(item);
                    break;
                }
            }
        }
    }
}
