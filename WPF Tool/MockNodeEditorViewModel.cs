using System.ComponentModel;
using System.Windows.Input;

namespace WPF_Tool
{
    public class MockNodeEditorViewModel : INotifyPropertyChanged
    {
        public string MethodName { get => _methodName; set { _methodName = value; OnPropertyChanged(nameof(MethodName)); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(nameof(Url)); } }
        public string RequestBody { get => _requestBody; set { _requestBody = value; OnPropertyChanged(nameof(RequestBody)); } }
        public string ResponseBody { get => _responseBody; set { _responseBody = value; OnPropertyChanged(nameof(ResponseBody)); } }
        public string ResponseDelay { get => _responseDelay; set { _responseDelay = value; OnPropertyChanged(nameof(ResponseDelay)); } }
        public string ResponseStatusCode { get => _responseStatusCode; set { _responseStatusCode = value; OnPropertyChanged(nameof(ResponseStatusCode)); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        private string _methodName, _url, _requestBody, _responseBody, _responseDelay, _responseStatusCode, _description;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ICommand OkCommand { get; }
        public event EventHandler? CloseRequested;

        public MockNodeEditorViewModel()
        {
            OkCommand = new RelayCommand<object>(_ => OnOk());
        }

        private void OnOk()
        {
            // Validation or data preparation logic here
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}