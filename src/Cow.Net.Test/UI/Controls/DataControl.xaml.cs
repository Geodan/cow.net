using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Cow.Net.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cow.Net.test.UI.Controls
{
    public partial class DataControl
    {
        private string _key;
        private object _value;
        private DataType _datatype;

        public DataControl()
        {
            InitializeComponent();
            SetComboBox();
        }

        public string Key
        {
            get { return TxtKey.Text; }
            set
            {
                _key = value;
                SetKey();
            }
        }

        public object Value
        {
            get { return TextToObject(TxtValue.Text); }
            set
            {
                _value = value;
                SetDataType(value);
                SetTextField(_value);
                CbDataType.SelectedItem = _datatype;                
            }
        }

        private void SetComboBox()
        {
            CbDataType.ItemsSource = Enum.GetValues(typeof(DataType)).Cast<DataType>();
        }

        private object TextToObject(string text)
        {
            switch (_datatype)
            {
                case DataType.STRING:
                    return text;
                case DataType.BOOL:
                    bool bOut;
                    return bool.TryParse(text, out bOut) && bOut;
                case DataType.DICTIONARY:
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(text, new DataConverter());
                case DataType.ARRAY:
                    return JsonConvert.DeserializeObject<IList>(text);
                case DataType.DOUBLE:
                    double dOut;
                    return double.TryParse(text, out dOut);
                case DataType.INT:
                    int iOut;
                    return int.TryParse(text, out iOut);
                case DataType.LONG:
                    long lOut2;
                    return Int64.TryParse(text, out lOut2) ? (long?)lOut2 : null;
            }

            return text;
        }

        private void SetDataType(object value)
        {
            if (value is string)            
                _datatype = DataType.STRING;
            else if (value is bool)
                _datatype = DataType.BOOL;
            else if (value is int)
                _datatype = DataType.INT;
            else if (value is double)
                _datatype = DataType.DOUBLE;
            else if (value is long)
                _datatype = DataType.LONG;
            else if (value is IDictionary<string, JToken>)
                _datatype = DataType.DICTIONARY;
            else if (value is IList)
                _datatype = DataType.ARRAY;
            else
            {
                _datatype = DataType.UNKNOWN;
            }
        }

        private void SetTextField(object value)
        {
            switch (_datatype)
            {
                case DataType.STRING:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.BOOL:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.DICTIONARY:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.ARRAY:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.DOUBLE:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.INT:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.LONG:
                    TxtValue.Text = value.ToString();
                    break;
                case DataType.UNKNOWN:
                    TxtValue.Text = value.ToString();
                    break;
            }
        }

        private void SetKey()
        {
            TxtKey.Text = _key;
        }

        public enum DataType
        {
            STRING,
            INT,
            DOUBLE,
            BOOL,
            LONG,
            DICTIONARY,
            ARRAY,
            UNKNOWN
        }

        private void CbDataType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _datatype = (DataType)Enum.Parse(typeof(DataType), CbDataType.SelectedItem.ToString());
        }
    }
}
