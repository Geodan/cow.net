using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Cow.Net.Core.Annotations;

namespace Cow.Net.Core.Utils
{
    public class PerformanceMonitor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _memory;
        private Timer _timer;

        /// <summary>
        /// Start requesting memory usage
        /// </summary>
        /// <param name="updateInterval">Interval to update the Performance Numbers (in milliseconds)</param>
        public void Start(int updateInterval = 1000)
        {
            if (_timer != null)
                Stop();

            _timer = new Timer(HandleTimerTick, new AutoResetEvent(false), 0, updateInterval);                  
        }

        public void Stop()
        {
            _timer.Dispose();
        }

        public int Memory
        {
            get
            {
                return _memory;
            }
            private set
            {
                _memory = value; OnPropertyChanged();
            }
        }

        private void HandleTimerTick(object state)
        {            
            Memory = (Int32)Math.Round((double) (GC.GetTotalMemory(false) / 1048576), 0 );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            if (CoreSettings.Instance.SynchronizationContext != null)
            {
                CoreSettings.Instance.SynchronizationContext.Post(o => handler(this, new PropertyChangedEventArgs(propertyName)), null);
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
