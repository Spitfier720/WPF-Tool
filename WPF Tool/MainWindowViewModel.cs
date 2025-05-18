using EasyMockLib;
using EasyMockLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_Tool
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TreeNode> RootNodes { get; } = new();
        public ICommand RemoveNodeCommand { get; }
        public ICommand RefreshNodeCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand LoadMockFileCommand { get; }
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

        public MainWindowViewModel(IFileDialogService fileDialogService)
        {
            var parser = new MockFileParser();
            foreach (var file in Directory.EnumerateFiles(ConfigurationManager.AppSettings["MockFileFolder"], "*.txt"))
            {
                var mock = parser.Parse(file);
                RootNodes.Add(new MockTreeNode(mock));
            }

            RemoveNodeCommand = new RelayCommand<TreeNode>(RemoveNode);
            RefreshNodeCommand = new RelayCommand<TreeNode>(RefreshNode);
            ClearLogCommand = new RelayCommand<object>(_ => LogText = string.Empty);
            LoadMockFileCommand = new RelayCommand<object>(_ => LoadMockFile());
            TreeNodeSelectedCommand = new RelayCommand<TreeNode>(OnTreeNodeSelected);
            _fileDialogService = fileDialogService;
        }

        private void RemoveNode(TreeNode? node)
        {
            if (node == null) return;
            if (node.Parent != null)
                node.Parent.Children.Remove(node);
            else
                RootNodes.Remove(node);
        }

        private void RefreshNode(TreeNode? node)
        {
            if (node is not MockTreeNode mockTreeNode) return;
            if (mockTreeNode.Tag is not MockFileNode fileNode) return;

            int index = RootNodes.IndexOf(mockTreeNode);
            if (index < 0) return;

            var parser = new MockFileParser();
            var refreshedMock = parser.Parse(fileNode.MockFile);

            RootNodes.RemoveAt(index);
            RootNodes.Insert(index, new MockTreeNode(refreshedMock));
        }

        private void LoadMockFile()
        {
            var filePath = _fileDialogService.OpenFile("Text Files (*.txt)|*.txt|All Files (*.*)|*.*");
            if (string.IsNullOrEmpty(filePath)) return;
            var parser = new MockFileParser();
            var mock = parser.Parse(filePath);
            RootNodes.Add(new MockTreeNode(mock));
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

        private void AppendOutput(string text) {LogText += text;}
    }
}
