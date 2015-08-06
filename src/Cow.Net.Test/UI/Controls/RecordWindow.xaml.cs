using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Cow.Net.Core;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;

namespace Cow.Net.test.UI.Controls
{   
    public partial class RecordWindow
    {
        private readonly CowStore _store;
        private StoreRecord _record;

        public RecordWindow(CowStore store, StoreRecord record = null)
        {
            InitializeComponent();
            _store = store;
            _record = record;

            var now = TimeUtils.GetMillisencondsFrom1970().ToString();
            
            if (_record == null)
            {
                TxtId.Text = now;
                TxtUpdated.Text = now;
            }
            else
            {
                TxtId.Text = _record.Id;
                TxtCreated.Text = _record.Created.ToString();
                TxtUpdated.Text = _record.Updated.ToString();
                CbDeleted.IsChecked = _record.Deleted;

                if (record.Data != null)
                {
                    foreach (var dataControl in record.Data.Select(o => new DataControl
                    {
                        Key = o.Key, 
                        Value = o.Value, Margin = new Thickness(5,5,5,5)
                    }))
                    {
                        DataStack.Children.Add(dataControl);
                    }
                }
            }

            SetDeltas();
        }

        private void SetDeltas()
        {
            if(_record.Deltas == null || !_record.Deltas.Any())
                return;

            DeltaStack.Children.Clear();

            foreach (var delta in _record.Deltas)
            {
                DeltaStack.Children.Add(new DeltaControl(delta));
            } 
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_record == null)
            {
                _record = new StoreRecord(TxtId.Text);                
                _store.Add(_record);
            }

            SetDataForRecord(_record);
            _record.Sync();

            Close();
        }

        private void SetDataForRecord(StoreRecord record)
        {
            foreach (var child in DataStack.Children)
            {
                var dc = (DataControl)child;
                if (dc == null || string.IsNullOrEmpty(dc.Key))
                    continue;

                if(record.Data.ContainsKey(dc.Key))
                    record.UpdateData(dc.Key, dc.Value);
                else                
                    record.AddData(dc.Key, dc.Value);                
            }

            if (CbDeleted.IsChecked.HasValue)
            {
                if(CbDeleted.IsChecked.Value == record.Deleted)
                    return;

                record.SetDeleted(CbDeleted.IsChecked.Value);
            }            
        }

        private void BtnAddData_OnClick(object sender, RoutedEventArgs e)
        {
            DataStack.Children.Add(new DataControl());
        }
    }
}
