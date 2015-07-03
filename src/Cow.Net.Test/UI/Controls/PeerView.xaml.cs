using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
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

        private void Peers_CollectionChanged(object sender, List<Core.Models.StoreRecord> newRecords, List<Core.Models.StoreRecord> deletedRecords, List<Core.Models.StoreRecord> unchangedRecords, string key)
        {
            
        }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
}
