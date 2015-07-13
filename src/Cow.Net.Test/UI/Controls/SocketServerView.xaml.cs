using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Cow.Net.Core.Config.Default.Stores;
using Cow.Net.Core.Models;

namespace Cow.Net.test.UI.Controls
{
    public partial class SocketServerView
    {
        public SocketServerStore SocketServerStore { get; set; }

        public SocketServerView()
        {
            InitializeComponent();
        }

        public void SetSocketServerStore(SocketServerStore socketserverStore)
        {
            SocketServerStore = socketserverStore;
            UpdateList(SocketServerStore.Records);
            SocketServerStore.CollectionChanged += UserStore_CollectionChanged;
        }

        private void UserStore_CollectionChanged(object sender, List<StoreRecord> newRecords, string key)
        {
            UpdateList(newRecords);
        }

        private void UpdateList(IEnumerable<StoreRecord> records)
        {
            foreach (var storeRecord in records)
            {
                if (storeRecord.Deleted)
                    continue;

                var txtBlock = new TextBlock { Text = storeRecord.Id, Tag = storeRecord, Foreground = new SolidColorBrush(Colors.Black) };
                SocketList.Items.Add(txtBlock);
            }
        }
    }
}
