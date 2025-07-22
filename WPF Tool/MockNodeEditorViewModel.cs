using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WPF_Tool
{
    public class MockNodeEditorViewModel : INotifyPropertyChanged
    {
        public double MaxEditorWidth => SystemParameters.WorkArea.Width * 0.8; // 80% of the screen width
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

        private bool _OKPressed;
        public bool OKPressed
        {
            get => _OKPressed;
            set
            {
                if (_OKPressed != value)
                {
                    _OKPressed = value;
                    OnPropertyChanged(nameof(OKPressed));
                }
            }
        }

        public MockNodeEditorViewModel()
        {
            OkCommand = new RelayCommand<object>(OnOk);
        }

        private void OnOk(object? windowObj)
        {
            if (string.IsNullOrWhiteSpace(MethodName))
            {
                MessageBox.Show("Method Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Url))
            {
                MessageBox.Show("URL is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(ResponseDelay) && (!int.TryParse(ResponseDelay, out int delay) || delay < 0))
            {
                MessageBox.Show("Response Delay must be a non-negative integer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Enum.TryParse(typeof(System.Net.HttpStatusCode), ResponseStatusCode, true, out var statusCode) || !Enum.IsDefined(typeof(System.Net.HttpStatusCode), statusCode))
            {
                MessageBox.Show("Response Status Code must be a valid HTTP status code (e.g., 200, OK, NotFound).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (windowObj is Window window)
            {
                OKPressed = true;
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Invalid window object.");
            }
            
        }
    }
}