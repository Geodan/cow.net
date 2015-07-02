using Cow.Net.Core;

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
            Peers.CollectionChanged += Peers_CollectionChanged;
        }

        private void Peers_CollectionChanged(object sender, System.Collections.Generic.List<Core.Models.StoreRecord> newRecords, System.Collections.Generic.List<Core.Models.StoreRecord> deletedRecords, System.Collections.Generic.List<Core.Models.StoreRecord> unchangedRecords, string key)
        {
            
        }
    }
}
