using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cow.Net.Core.Models;
using Cow.Net.Core.Utils;

namespace Cow.Net.test.UI.Controls
{
    public partial class DeltaControl
    {
        private Delta _delta;

        public DeltaControl(Delta delta)
        {
            InitializeComponent();
            _delta = delta;
            SetDeltaInfo(_delta);
        }

        private void SetDeltaInfo(Delta delta)
        {
            var localTime = TimeUtils.GetDateTimeFrom1970Milliseconds(delta.TimeStamp).ToLocalTime();
            TxtTimestamp.Text = string.Format("{0} {1}", localTime.ToShortDateString(), localTime.ToShortTimeString());
            if (!string.IsNullOrEmpty(delta.UserId))
                TxtUser.Text = delta.UserId;

            TxtDeleted.Text = delta.Deleted.ToString();
            if (delta.Data != null)
            {
                foreach (var o in delta.Data.Keys)
                {                                        
                    var foreground = new SolidColorBrush(Color.FromArgb(255, 198, 198, 198));
                    var title = new TextBlock { Text = o, Foreground = foreground, Margin = new Thickness(0,5,0,0)};
                    DataStack.Children.Add(title);

                    var d = delta.Data.ContainsKey(o) ? delta.Data[o] == null ? "" : delta.Data[o].ToString() : "";
                    var data = new TextBox { Text = d, IsEnabled = false, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 0, 7) };
                    DataStack.Children.Add(data);
                }
            }
        }
    }
}
