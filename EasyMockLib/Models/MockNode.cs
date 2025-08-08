using System.ComponentModel;
using System.Net;

namespace EasyMockLib.Models
{
    public class MockNode : INotifyPropertyChanged
    {
        public Request Request { get; set; }
        private Response _response;
        public Response Response
        {
            get => _response;
            set
            {
                if (_response != value)
                {
                    // Unsubscribe from old response
                    if (_response != null)
                    {
                        _response.PropertyChanged -= OnResponsePropertyChanged;
                    }

                    _response = value;

                    // Subscribe to new response
                    if (_response != null)
                    {
                        _response.PropertyChanged += OnResponsePropertyChanged;
                    }

                    OnPropertyChanged(nameof(Response));
                    OnPropertyChanged(nameof(StatusCodeForHighlight));
                }
            }
        }
        public string Url { get; set; }
        public string MethodName { get; set; }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int StatusCodeForHighlight => (int)(Response?.StatusCode ?? HttpStatusCode.OK);

        private void OnResponsePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Response.StatusCode))
            {
                OnPropertyChanged(nameof(StatusCodeForHighlight));
            }
        }

    }
}
