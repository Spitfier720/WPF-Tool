using System.ComponentModel;
using System.Net;

namespace EasyMockLib.Models
{
    public class MockNode : INotifyPropertyChanged
    {
        public Request Request { get; set; }
        public Response Response { get; set; }
        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        private string _methodName;
        public string MethodName
        {
            get => _methodName;
            set
            {
                if (_methodName != value)
                {
                    _methodName = value;
                    OnPropertyChanged(nameof(MethodName));
                }
            }
        }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
