using EasyMockLib;
using EasyMockLib.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Application = System.Windows.Application;

namespace WPF_Tool
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly int SERVICE_TIMEOUT_IN_SECONDS = int.Parse(ConfigurationManager.AppSettings["ServiceTimeoutInSeconds"]);

        public ObservableCollection<TreeNode> RootNodes { get; } = new();
        public ICommand MockNodeContextMenuCommand { get; }
        public ICommand SimulateExceptionCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand LoadMockFileCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }
        public ICommand TreeNodeSelectedCommand { get; }
        private readonly IFileDialogService _fileDialogService;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }

        private bool _isServiceRunning;
        public bool IsServiceRunning
        {
            get => _isServiceRunning;
            set
            {
                if (_isServiceRunning == value) return;
                _isServiceRunning = value;
                OnPropertyChanged(nameof(IsServiceRunning));
                OnPropertyChanged(nameof(CanStartService));
                OnPropertyChanged(nameof(CanStopService));
                OnPropertyChanged(nameof(CanLoadMockFile));
            }
        }

        public bool CanStartService => !IsServiceRunning;
        public bool CanStopService => IsServiceRunning;
        public bool CanLoadMockFile => !IsServiceRunning;

        private const string SimulateExceptionInternalServerError = "InternalServerError";
        private const string SimulateExceptionNotFound = "NotFound";
        private const string SimulateExceptionTimeOut = "TimeOut";

        private HttpListener listener = new HttpListener { Prefixes = { $"http://localhost:{ConfigurationManager.AppSettings["BindingPort"]}/" } };
        private CancellationTokenSource? tokenSource;
        private Dictionary<string, Dictionary<string, List<string>>> soapMatchingConfig;

        public MainWindowViewModel(IFileDialogService fileDialogService)
        {
            var parser = new MockFileParser();
            foreach (var file in Directory.EnumerateFiles(ConfigurationManager.AppSettings["MockFileFolder"], "*.txt"))
            {
                var mock = parser.Parse(file);
                RootNodes.Add(new MockTreeNode(mock));
            }

            MockNodeContextMenuCommand = new RelayCommand<object>(OnMockNodeContextMenuAction);
            SimulateExceptionCommand = new RelayCommand<object>(OnSimulateException);
            ClearLogCommand = new RelayCommand<object>(_ => LogText = string.Empty);
            LoadMockFileCommand = new RelayCommand<object>(_ => LoadMockFile());
            StartServiceCommand = new RelayCommand<object>(_ => StartWebServer(), _ => CanStartService);
            StopServiceCommand = new RelayCommand<object>(_ => StopWebServer(), _ => CanStopService);
            TreeNodeSelectedCommand = new RelayCommand<TreeNode>(OnTreeNodeSelected);
            _fileDialogService = fileDialogService;
            soapMatchingConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText("SoapServiceMatchingElements.json"));
        }
        private void LoadMockFile()
        {
            var filePath = _fileDialogService.OpenFile("Text Files (*.txt)|*.txt|All Files (*.*)|*.*");
            if (string.IsNullOrEmpty(filePath)) return;
            var parser = new MockFileParser();
            var mock = parser.Parse(filePath);
            RootNodes.Add(new MockTreeNode(mock));
        }

        private void StartWebServer()
        {
            tokenSource = new CancellationTokenSource();
            tokenSource.Token.Register(() =>
            {
                if (listener.IsListening)
                {
                    listener.Stop();
                }
            });

            Task.Run(async () =>
            {
                listener.Prefixes.Add("http://localhost:8080/");
                listener.Start();
                AppendOutput($"Mock service started.\n");
                Application.Current.Dispatcher.Invoke(() => IsServiceRunning = true);

                while (!tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var context = await listener.GetContextAsync();
                        Task.Run(() => ProcessRequest(context), tokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        if (e is HttpListenerException) return;
                    }
                }
            });
        }

        public void StopWebServer()
        {
            AppendOutput($"Mock service stopped.{Environment.NewLine}");
            Application.Current.Dispatcher.Invoke(() => IsServiceRunning = false);
            tokenSource?.Cancel();
        }
        private void ProcessRequest(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                try
                {
                    //var handled = false;
                    (var mock, var method, var requestContent) = GetMock(context);
                    if (mock != null)
                    {
                        //AppendOutput($"From file {mock.Parent.Text}{Environment.NewLine}");
                        if (mock.Request.RequestType == ServiceType.REST)
                        {
                            AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {method} {context.Request.Url.AbsoluteUri} Request{Environment.NewLine}");
                            AppendOutput($"RequestBody: {requestContent}");
                            AppendOutput(Environment.NewLine);
                        }
                        else
                        {
                            AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {context.Request.Url.AbsoluteUri} {method} Request{Environment.NewLine}");
                            AppendOutput($"{requestContent}");
                            AppendOutput(Environment.NewLine);
                        }
                        AppendOutput(Environment.NewLine);
                        if (!string.IsNullOrEmpty(mock.SimulateException))
                        {
                            if (mock.SimulateException == SimulateExceptionInternalServerError)
                            {
                                OutputResponseContent("", HttpStatusCode.InternalServerError, context, response);
                            }
                            else if (mock.SimulateException == SimulateExceptionNotFound)
                            {
                                OutputResponseContent("", HttpStatusCode.NotFound, context, response);
                            }
                            else if (mock.SimulateException == SimulateExceptionTimeOut)
                            {
                                Thread.Sleep(SERVICE_TIMEOUT_IN_SECONDS * 1000);
                                return;
                            }
                        }
                        else if (mock.Response == null)
                        {
                            // No response received, do not send back anything to simulate timeout scenario
                            Thread.Sleep(SERVICE_TIMEOUT_IN_SECONDS * 1000);
                            return;
                        }
                        else if (mock.Response.StatusCode == HttpStatusCode.OK)
                        {
                            OutputMockResponse(mock, context, response);
                            if (mock.Request.RequestType == ServiceType.REST)
                            {
                                AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {method} {context.Request.Url.AbsoluteUri} Response{Environment.NewLine}");
                                AppendOutput($"ResponseBody: {mock.Response.ResponseBody.Content}");
                            }
                            else
                            {
                                AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {context.Request.Url.AbsoluteUri} {method} Response{Environment.NewLine}");
                                AppendOutput($"{mock.Response.ResponseBody.Content}");
                            }
                            AppendOutput(Environment.NewLine + Environment.NewLine);
                        }
                        else
                        {
                            if (mock.Response.ResponseBody != null)
                            {
                                OutputMockResponse(mock, context, response);
                            }
                            if (mock.Request.RequestType == ServiceType.REST)
                            {
                                AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {method} {context.Request.Url.AbsoluteUri} Response{Environment.NewLine}");
                                AppendOutput($"StatusCode: {mock.Response.StatusCode}");
                            }
                            else
                            {
                                AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {context.Request.Url.AbsoluteUri} {method} Response{Environment.NewLine}");
                                AppendOutput($"StatusCode: {mock.Response.StatusCode}");
                            }
                            AppendOutput(Environment.NewLine + Environment.NewLine);
                        }
                    }
                    else
                    {
                        AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {context.Request.Url.AbsoluteUri} {method} Request{Environment.NewLine}");
                        AppendOutput($"{requestContent}");
                        AppendOutput(Environment.NewLine + Environment.NewLine);
                        AppendOutput($"{DateTime.Now.ToString("HH:mm:ss.fff")} {context.Request.Url.AbsoluteUri} {method} Response{Environment.NewLine}");
                        AppendOutput($"StatusCode: {HttpStatusCode.NotFound}");
                        AppendOutput(Environment.NewLine + Environment.NewLine);
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (Exception e)
                {
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private (MockNode, string, string) GetMock(HttpListenerContext context)
        {
            MockNode mock = null;
            string requestContent = ReadRequest(context);
            string method = context.Request.HttpMethod.ToString();
            if (context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) || context.Request.ContentType.StartsWith("application/json"))
            {
                // REST request
                method = context.Request.HttpMethod.ToString();
                string url = context.Request.HttpMethod == HttpMethod.Get.ToString() ? context.Request.Url.PathAndQuery : context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf('/') + 1);
                mock = GetMock(ServiceType.REST, url, method, requestContent);
            }
            else if (context.Request.ContentType.StartsWith("text/xml"))
            {
                // SOAP request
                method = GetSoapAction(requestContent);
                mock = GetMock(ServiceType.SOAP, context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf('/') + 1), method, requestContent);
            }
            else
            {
                MessageBox.Show($"Unknow content type {context.Request.ContentType}");
            }
            return (mock, method, requestContent);
        }
        private MockNode? GetMock(ServiceType serviceType, string service, string method, string requestContent)
        {
            foreach (var node in RootNodes)
            {

                var mock = (node.Tag as MockFileNode)?.GetMock(serviceType, service, method, requestContent, soapMatchingConfig);
                if (mock != null)
                {
                    return mock;
                }
            }
            return null;
        }
        private static string ReadRequest(HttpListenerContext context)
        {
            using (var body = context.Request.InputStream)
            using (var reader = new StreamReader(body, context.Request.ContentEncoding))
            {
                //Get the data that was sent to us
                return reader.ReadToEnd();
            }
        }
        private static string GetSoapAction(string soapEnv)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XDocument doc = XDocument.Parse(soapEnv);
            return doc.Descendants(soap + "Body").First().Elements().First().Name.LocalName.Replace("Request", "");
        }
        private void OutputResponseContent(string content, HttpStatusCode status, HttpListenerContext context, HttpListenerResponse response)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            response.ContentType = context.Request.ContentType;
            response.StatusCode = (int)status;
            var correlationId = context.Request.Headers.AllKeys.FirstOrDefault(h => h.Equals("X-BMO-CorrelationId", StringComparison.OrdinalIgnoreCase));
            if (correlationId != null)
            {
                response.Headers.Add(correlationId, context.Request.Headers[correlationId]);
            }
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
        }

        private void OutputMockResponse(MockNode mock, HttpListenerContext context, HttpListenerResponse response)
        {
            OutputResponseContent(mock.Response.ResponseBody.Content, mock.Response.StatusCode, context, response);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTreeNodeSelected(TreeNode selectedNode)
        {
            if(selectedNode is MockTreeNode treeNode && treeNode.NodeType == NodeTypes.MockItem)
            {
                var node = treeNode.Tag as MockNode;

                if (node?.Request.RequestType == ServiceType.SOAP)
                {
                    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Request.Url} {node.Request.ServiceName} Request\r\n{node.Request.RequestBody.Content}\r\n\r\n");
                    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Request.Url} {node.Request.ServiceName} Response\r\n");
                }
                else
                {
                    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Request.MethodName} {node.Request.Url} Request\r\n{node.Request.RequestBody.Content}\r\n\r\n");
                    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Request.Url} {node.Request.ServiceName} Response\r\n");
                }

                if(node.Response != null)
                {
                    if(node.Response.StatusCode != HttpStatusCode.OK)
                    {
                        AppendOutput(node.Response.StatusCode + "\r\n\r\n");
                        if(node.Response.ResponseBody.Content != null)
                        {
                            AppendOutput(node.Response.ResponseBody.Content + "\r\n\r\n");
                        }
                    }
                    else
                    {
                        AppendOutput(node.Response.ResponseBody.Content + "\r\n\r\n");
                    }
                }
            }
        }

        private void SaveMock(MockTreeNode root)
        {
            using (StreamWriter writer = new StreamWriter((root.Tag as MockFileNode).MockFile))
            {
                foreach (MockTreeNode treeNode in RootNodes)
                {
                    var node = treeNode.Tag as MockNode;
                    if (node?.Request.RequestType == ServiceType.REST)
                    {
                        writer.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} {node.Request.MethodName} {node.Request.Url} Request");
                        writer.WriteLine($"RequestBody: {node.Request.RequestBody.Content}");
                        writer.WriteLine();
                        writer.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} {node.Request.MethodName} {node.Request.Url} Response");
                        if (node.Response.StatusCode == HttpStatusCode.OK)
                        {
                            writer.WriteLine($"ResponseBody: {node.Response.ResponseBody.Content}");
                        }
                        else
                        {
                            writer.WriteLine($"StatusCode:{node.Response.StatusCode}");
                        }
                        writer.WriteLine();
                    }
                    else
                    {
                        writer.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} {node.Request.Url} {node.Request.MethodName} Request");
                        if (!node.Request.MethodName.Equals(HttpMethod.Get.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteLine(node.Request.RequestBody.Content);
                        }
                        writer.WriteLine();
                        writer.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} {node.Request.Url} {node.Request.MethodName} Response");
                        writer.WriteLine(node.Response.ResponseBody.Content);
                        writer.WriteLine();
                    }
                }
            }
        }

        private void OnMockNodeContextMenuAction(object parameter)
        {
            if (parameter is not Tuple<string, MockTreeNode> ctxParam)
                return;

            string action = ctxParam.Item1;
            MockTreeNode node = ctxParam.Item2;
            if (node == null) return;

            switch (action)
            {
                case "Remove":
                    if (node.NodeType == NodeTypes.MockFile)
                    {
                        if (node.IsDirty)
                        {
                            var result = MessageBox.Show(
                                "You have made changes to the mockup. Do you want to save it before remove it?",
                                "Confirmation", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                SaveMock(node);
                                node.IsDirty = false;
                                MessageBox.Show("Mock saved.", "Saved", MessageBoxButton.OK);
                            }
                        }
                    }
                    else
                    {
                        var root = node;
                        while (root.Tag as MockFileNode == null && root.Parent != null)
                            root = (MockTreeNode)root.Parent;
                        root.IsDirty = true;
                    }
                    if (node.Parent != null)
                        node.Parent.Children.Remove(node);
                    else
                        RootNodes.Remove(node);
                    break;

                case "Refresh":
                    if (node.Tag is MockFileNode fileNode)
                    {
                        int index = RootNodes.IndexOf(node);
                        if (index >= 0)
                        {
                            var parser = new MockFileParser();
                            var refreshedMock = parser.Parse(fileNode.MockFile);
                            RootNodes.RemoveAt(index);
                            RootNodes.Insert(index, new MockTreeNode(refreshedMock));
                        }
                    }
                    break;

                case "Save":
                    SaveMock(node);
                    break;
            }
        }

        private void OnSimulateException(object parameter)
        {
            if (parameter is not Tuple<string, MockTreeNode> ctxParam)
                return;

            var exceptionText = ctxParam.Item1;
            var node = ctxParam.Item2;

            if (node?.Tag is not MockNode mockNode)
                return;

            // Toggle the exception
            if (mockNode.SimulateException == exceptionText)
            {
                mockNode.SimulateException = string.Empty;
            }
            else
            {
                mockNode.SimulateException = exceptionText;
            }
        }

        private void AppendOutput(string text) {LogText += text;}
    }
}
