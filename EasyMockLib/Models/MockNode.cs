using System.ComponentModel;

namespace EasyMockLib.Models
{
    public class MockNode : INotifyPropertyChanged
    {
        public Request Request { get; set; }
        public Response Response { get; set; }
        public string Url { get; set; }
        public string MethodName { get; set; }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
