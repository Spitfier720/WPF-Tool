using System.ComponentModel;

namespace EasyMockLib.Models
{
    public class MockNode : INotifyPropertyChanged
    {
        public Request Request { get; set; }
        public Response Response { get; set; }

        private string _simulateException;
        public string SimulateException
        {
            get => _simulateException;
            set
            {
                if (_simulateException != value)
                {
                    _simulateException = value;
                    OnPropertyChanged(nameof(SimulateException));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
