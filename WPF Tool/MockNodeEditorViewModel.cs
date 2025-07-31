using EasyMockLib.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WPF_Tool
{
    public class MockNodeEditorViewModel : INotifyPropertyChanged
    {
        public double MaxEditorWidth => SystemParameters.WorkArea.Width * 0.8; // 80% of the screen width

        public List<ServiceType> ServiceTypes { get; } = new() { ServiceType.REST, ServiceType.SOAP };

        private ServiceType _serviceType = ServiceType.REST;
        public ServiceType ServiceType
        {
            get => _serviceType;
            set
            {
                if (_serviceType != value)
                {
                    _serviceType = value;
                    OnPropertyChanged(nameof(ServiceType));
                }
            }
        }
        public string MethodName { get => _methodName; set { _methodName = value; OnPropertyChanged(nameof(MethodName)); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(nameof(Url)); } }
        public string RequestBody { get => _requestBody; set { _requestBody = value; OnPropertyChanged(nameof(RequestBody)); } }
        public string ResponseBody { get => _responseBody; set { _responseBody = value; OnPropertyChanged(nameof(ResponseBody)); } }
        public string ResponseDelay { get => _responseDelay; set { _responseDelay = value; OnPropertyChanged(nameof(ResponseDelay)); } }
        public ObservableCollection<StatusCodeOption> StatusCodeOptions { get; }
        private StatusCodeOption _selectedStatusCodeOption;
        public StatusCodeOption SelectedStatusCodeOption
        {
            get => _selectedStatusCodeOption;
            set
            {
                if (_selectedStatusCodeOption != value)
                {
                    _selectedStatusCodeOption = value;
                    OnPropertyChanged(nameof(SelectedStatusCodeOption));
                }
            }
        }
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

            StatusCodeOptions = new ObservableCollection<StatusCodeOption>
            {
                new StatusCodeOption { Code = 200, Name = "OK" },
                new StatusCodeOption { Code = 400, Name = "BadRequest" },
                new StatusCodeOption { Code = 401, Name = "Unauthorized" },
                new StatusCodeOption { Code = 403, Name = "Forbidden" },
                new StatusCodeOption { Code = 404, Name = "NotFound" },
                new StatusCodeOption { Code = 500, Name = "InternalServerError" },
                new StatusCodeOption { Code = 502, Name = "BadGateway" }
            };
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