﻿using EasyMockLib.MatchingPolicies;
using EasyMockLib.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Xml.Serialization;
using Application = System.Windows.Application;

namespace WPF_Tool
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        RestRequestValueMatchingPolicy restMatchPolicy = new RestRequestValueMatchingPolicy()
        {
            RestServiceMatchingConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText("RestServiceMatchingConfig.json"))
        };

        SoapRequestValueMatchingPolicy soapMatchPolicy = new EasyMockLib.MatchingPolicies.SoapRequestValueMatchingPolicy()
        {
           SoapServiceMatchingConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText("SoapServiceMatchingConfig.json"))
        };

        public ObservableCollection<TreeNode> RootNodes { get; } = new();
        public ObservableCollection<RequestResponsePair> RequestResponsePairs { get; } = new();
        public ICommand MockNodeContextMenuCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand LoadMockFileCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }
        public ICommand TreeNodeSelectedCommand { get; }
        private readonly IFileDialogService _fileDialogService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isMockFileSelected;
        public bool IsMockFileSelected
        {
            get => _isMockFileSelected;
            set
            {
                if (_isMockFileSelected == value) return;
                _isMockFileSelected = value;
                OnPropertyChanged(nameof(IsMockFileSelected));
                OnPropertyChanged(nameof(IsMockNodeSelected));
            }
        }
        public bool IsMockNodeSelected => !_isMockFileSelected;

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

        public MainWindowViewModel(IFileDialogService fileDialogService)
        {
            //var parser = new MockFileParser(restServiceMatchingConfig, soapServiceMatchingConfig);
            //foreach (var file in Directory.EnumerateFiles(ConfigurationManager.AppSettings["MockFileFolder"], "*.txt"))
            //{
            //    var mock = parser.Parse(file);
            //    RootNodes.Add(new MockTreeNode(mock));
            //}

            foreach (var file in Directory.EnumerateFiles(ConfigurationManager.AppSettings["MockFileFolder"], "*.xml"))
            {
                var mockFileNode = new MockFileNode()
                {
                    MockFile = file,
                    Nodes = ParseXML(file)
                };

                RootNodes.Add(new MockTreeNode(mockFileNode));
            }

            MockNodeContextMenuCommand = new RelayCommand<object>(OnMockNodeContextMenuAction);
            ClearLogCommand = new RelayCommand<object>(_ =>
            {
                RequestResponsePairs.Clear();
                OnPropertyChanged(nameof(RequestResponsePairs));
            });
            LoadMockFileCommand = new RelayCommand<object>(_ => LoadMockFile());
            StartServiceCommand = new RelayCommand<object>(_ => StartWebServer(), _ => CanStartService);
            StopServiceCommand = new RelayCommand<object>(_ => StopWebServer(), _ => CanStopService);
            TreeNodeSelectedCommand = new RelayCommand<TreeNode>(OnTreeNodeSelected);
            _fileDialogService = fileDialogService;
        }

        private static string Dedent(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            if (!nonEmptyLines.Any()) return text.Trim();

            int minIndent = nonEmptyLines.Min(l => l.TakeWhile(char.IsWhiteSpace).Count());
            var dedented = lines.Select(l => l.Length >= minIndent ? l.Substring(minIndent) : l).ToArray();
            return string.Join(Environment.NewLine, dedented).Trim();
        }

        private void LoadMockFile()
        {
            var filePath = _fileDialogService.OpenFile("XML Files (*.xml)|*.xml");
            if (string.IsNullOrEmpty(filePath)) return;

            var mockFileNode = new MockFileNode()
            {
                MockFile = filePath,
                Nodes = ParseXML(filePath)
            };

            RootNodes.Insert(0, new MockTreeNode(mockFileNode));
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
                //AppendOutput($"Mock service started.\n");
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
            //AppendOutput($"Mock service stopped.{Environment.NewLine}");
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
                        string requestSummary = $"{method} {context.Request.Url.AbsoluteUri}";
                        string requestBody = requestContent;
                        string responseSummary = $"StatusCode: {mock.Response.StatusCode}";
                        string responseBody = mock.Response.ResponseBody?.Content ?? string.Empty;

                        Thread.Sleep(mock.Response.Delay * 1000);
                        OutputMockResponse(mock, context, response);

                        AddRequestResponsePair(requestSummary, requestBody, responseSummary, responseBody);
                    }
                    else
                    {
                        string requestSummary = $"{method} {context.Request.Url.AbsoluteUri}";
                        string requestBody = requestContent;
                        string responseSummary = $"StatusCode: {HttpStatusCode.NotFound}";
                        string responseBody = string.Empty;

                        response.StatusCode = (int)HttpStatusCode.NotFound;

                        AddRequestResponsePair(requestSummary, requestBody, responseSummary, responseBody);
                    }
                }
                catch (Exception e)
                {
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    // Optionally log the exception as a request/response pair
                    AddRequestResponsePair(
                        "Exception",
                        "",
                        "StatusCode: 500",
                        e.ToString()
                    );
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
                mock = GetMock(ServiceType.REST, url, method, requestContent, restMatchPolicy);
            }
            else if (context.Request.ContentType.StartsWith("text/xml"))
            {
                // SOAP request
                method = GetSoapAction(requestContent);
                mock = GetMock(ServiceType.SOAP, context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf('/') + 1), method, requestContent, soapMatchPolicy);
            }
            else
            {
                MessageBox.Show($"Unknow content type {context.Request.ContentType}");
            }
            return (mock, method, requestContent);
        }
        private MockNode?  GetMock(ServiceType serviceType, string service, string method, string requestContent, IMatchingPolicy matchingPolicy)
        {
            foreach (var node in RootNodes)
            {
                MockNode? mock = (node.Tag as MockFileNode)?.GetMock(serviceType, service, method, requestContent, matchingPolicy);
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
            if (selectedNode is MockTreeNode treeNode)
            {
                IsMockFileSelected = treeNode.NodeType == NodeTypes.MockFile;
                Console.WriteLine("Is Mock File Selected: " + IsMockFileSelected);

                if (treeNode.NodeType == NodeTypes.MockItem)
                {
                    var node = treeNode.Tag as MockNode;

                    //if (node?.Request.RequestType == ServiceType.SOAP)
                    //{
                    //    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Url} {node.Request.ServiceName} Request\r\n{node.Request.RequestBody.Content}\r\n\r\n");
                    //    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Url} {node.Request.ServiceName} Response\r\n");
                    //}
                    //else
                    //{
                    //    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.MethodName} {node.Url} Request\r\n{node.Request.RequestBody.Content}\r\n\r\n");
                    //    AppendOutput($"{DateTime.Now:HH:mm:ss.fffffff} {node.Url} {node.Request.ServiceName} Response\r\n");
                    //}

                    //if (node.Response != null)
                    //{
                    //    if (node.Response.StatusCode != HttpStatusCode.OK)
                    //    {
                    //        AppendOutput(node.Response.StatusCode + "\r\n\r\n");
                    //        if (node.Response.ResponseBody.Content != null)
                    //        {
                    //            AppendOutput(node.Response.ResponseBody.Content + "\r\n\r\n");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        AppendOutput(node.Response.ResponseBody.Content + "\r\n\r\n");
                    //    }
                    //}
                }
            }
        }

        private void SaveMock(MockTreeNode root)
        {
            var mockFileNode = root.Tag as MockFileNode;
            if (mockFileNode != null)
            {
                var serializer = new XmlSerializer(typeof(List<MockNode>));
                using (var writer = new StreamWriter(mockFileNode.MockFile))
                {
                    serializer.Serialize(writer, mockFileNode.Nodes);
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
                case "Add":
                    var editor = new MockNodeEditorWindow();
                    if (editor.ShowDialog() == true)
                    {
                        var vm = editor.EditorViewModel;
                        var newMockNode = new MockNode
                        {
                            MethodName = vm.MethodName,
                            Url = vm.Url,
                            Description = vm.Description,
                            Request = new Request
                            {
                                RequestBody = new Body
                                {
                                    Content = vm.RequestBody
                                }
                            },
                            Response = new Response
                            {
                                ResponseBody = new Body
                                {
                                    Content = vm.ResponseBody
                                },
                                StatusCode = Enum.TryParse(vm.ResponseStatusCode, out HttpStatusCode status) ? status : HttpStatusCode.OK,
                                Delay = int.TryParse(vm.ResponseDelay, out int delay) ? delay : 0
                            }
                        };
                        // Find the correct parent (service) and add the new node
                        if (node.Tag is MockFileNode existingFileNode)
                        {
                            existingFileNode.Nodes.Add(newMockNode);
                            node.Children.Add(new MockTreeNode(newMockNode) { Parent = node });
                        }
                    }
                    break;

                case "Edit":
                    if (node.Tag is MockNode mockNodeToEdit)
                    {
                        // Create and pre-fill the editor ViewModel
                        var vm = new MockNodeEditorViewModel
                        {
                            MethodName = mockNodeToEdit.MethodName,
                            Url = mockNodeToEdit.Url,
                            RequestBody = mockNodeToEdit.Request.RequestBody?.Content,
                            ResponseBody = mockNodeToEdit.Response.ResponseBody?.Content,
                            ResponseDelay = mockNodeToEdit.Response.Delay.ToString(),
                            ResponseStatusCode = mockNodeToEdit.Response.StatusCode.ToString(),
                            Description = mockNodeToEdit.Description
                        };

                        var mockNodeEditor = new MockNodeEditorWindow();
                        mockNodeEditor.DataContext = vm;

                        if (mockNodeEditor.ShowDialog() == true)
                        {
                            // Update the existing MockNode with edited values
                            mockNodeToEdit.MethodName = vm.MethodName;
                            mockNodeToEdit.Url = vm.Url;
                            mockNodeToEdit.Request.RequestBody.Content = vm.RequestBody;
                            mockNodeToEdit.Response.ResponseBody.Content = vm.ResponseBody;
                            mockNodeToEdit.Response.Delay = int.TryParse(vm.ResponseDelay, out int delay) ? delay : 0;
                            mockNodeToEdit.Response.StatusCode = Enum.TryParse(vm.ResponseStatusCode, out HttpStatusCode status) ? status : HttpStatusCode.OK;
                            mockNodeToEdit.Description = vm.Description;
                            // Notify UI if needed
                        }
                    }
                    break;

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
                            var refreshedMockFileNode = new MockFileNode()
                            {
                                MockFile = fileNode.MockFile,
                                Nodes = ParseXML(fileNode.MockFile)
                            };

                            RootNodes[index] = new MockTreeNode(refreshedMockFileNode);
                        }
                    }
                    break;

                case "Save":
                    SaveMock(node);
                    break;
            }
        }

        public void AddRequestResponsePair(string requestSummary, string requestBody, string responseSummary, string responseBody)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RequestResponsePairs.Add(new RequestResponsePair
                {
                    RequestSummary = requestSummary,
                    RequestBody = requestBody,
                    ResponseSummary = responseSummary,
                    ResponseBody = responseBody
                });
            });
        }

        private List<MockNode> ParseXML(string filepath)
        {
            var serializer = new XmlSerializer(typeof(List<MockNode>));
            List<MockNode> nodes;
            using (var xml = File.OpenRead(filepath))
            {
                nodes = (List<MockNode>)serializer.Deserialize(xml);
            }

            // Dedent and assign ContentObject for each node's request and response body
            foreach (var mockNode in nodes)
            {
                mockNode.Request.RequestBody.Content = Dedent(mockNode.Request.RequestBody.Content ?? string.Empty);
                mockNode.Response.ResponseBody.Content = Dedent(mockNode.Response.ResponseBody.Content ?? string.Empty);

                if (mockNode.Request?.RequestBody?.Content != null)
                {
                    var content = mockNode.Request.RequestBody.Content.Trim();
                    if (mockNode.RequestType == ServiceType.REST)
                    {
                        try { mockNode.Request.RequestBody.ContentObject = JToken.Parse(content); }
                        catch { mockNode.Request.RequestBody.ContentObject = null; }
                    }
                    else if (mockNode.RequestType == ServiceType.SOAP)
                    {
                        try { mockNode.Request.RequestBody.ContentObject = XElement.Parse(content); }
                        catch { mockNode.Request.RequestBody.ContentObject = null; }
                    }
                }

                if (mockNode.Response?.ResponseBody?.Content != null)
                {
                    var content = mockNode.Response.ResponseBody.Content.Trim();
                    if (mockNode.RequestType == ServiceType.REST)
                    {
                        try { mockNode.Response.ResponseBody.ContentObject = JToken.Parse(content); }
                        catch { mockNode.Response.ResponseBody.ContentObject = null; }
                    }
                    else if (mockNode.RequestType == ServiceType.SOAP)
                    {
                        try { mockNode.Response.ResponseBody.ContentObject = XElement.Parse(content); }
                        catch { mockNode.Response.ResponseBody.ContentObject = null; }
                    }
                }
            }

            return nodes;
        }
    }
}