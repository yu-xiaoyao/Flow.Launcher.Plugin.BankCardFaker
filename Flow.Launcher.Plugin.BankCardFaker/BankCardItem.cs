#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class BankCardItem : INotifyPropertyChanged
    {
        private bool _isChecked;

        public BankCardInfo Info { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Bin => Info.Bin;
        public string BankName => Info.BankName;
        public string CardName => Info.CardName;
        public int CardLength => Info.CardLength;
        public string CardType => Info.CardType;

        public BankCardItem(BankCardInfo info, bool isChecked)
        {
            Info = info;
            _isChecked = isChecked;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
