
using System.ComponentModel;

namespace Carebed.Infrastructure.Message.UI
{
    public class AlertViewModel : INotifyPropertyChanged
    {
        private string alertText = "";
        private bool isCritical;
        private string source = "";
        private string time = "";

        public string AlertText
        {
            get => alertText;
            set
            {
                if (alertText != value)
                {
                    alertText = value;
                    OnPropertyChanged(nameof(AlertText));
                }
            }
        }

        public bool IsCritical
        {
            get => isCritical;
            set
            {
                if (isCritical != value)
                {
                    isCritical = value;
                    OnPropertyChanged(nameof(IsCritical));
                }
            }
        }

        public string Source
        {
            get => source;
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged(nameof(Source));
                }
            }
        }

        public string Time
        {
            get => time;
            set
            {
                if (time != value)
                {
                    time = value;
                    OnPropertyChanged(nameof(Source));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
