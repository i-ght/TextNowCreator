using System.ComponentModel;
using System.Runtime.CompilerServices;
using DankWindowsWaifu.Properties;

namespace TextNow.Creator
{
    public class DataGridItem : INotifyPropertyChanged
    {
        private string _status;
        private string _account;

        public string Account
        {
            get => _account;
            set
            {
                if (_account == value)
                    return;

                _account = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;

                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
