using EasyMockLib.Models;
using System.ComponentModel;
using System.IO;

namespace WPF_Tool
{
    internal enum NodeTypes
    {
        MockFile,
        MockItem
    }
    internal class MockTreeNode : TreeNode, INotifyPropertyChanged
    {

        public MockTreeNode(MockFileNode fileNode)
        {
            NodeType = NodeTypes.MockFile;
            Tag = fileNode;
            Header = Path.GetFileNameWithoutExtension(fileNode.MockFile);

            foreach(MockNode node in fileNode.Nodes)
            {
                var child = new MockTreeNode(node) { Parent = this };
                this.Children.Add(child);
            }
        }

        public MockTreeNode(MockNode node)
        {
            NodeType = NodeTypes.MockItem;
            Tag = node;
            Header = $"{node.Url} - {node.MethodName}";
            
            node.PropertyChanged += OnMockNodePropertyChanged;

            if (node.Response != null)
            {
                node.Response.PropertyChanged += OnResponsePropertyChanged;
            }
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        public NodeTypes NodeType { get; set; }

        public int StatusCodeForHighlight
        {
            get
            {
                if (Tag is MockNode mockNode)
                    return (int)mockNode.Response?.StatusCode;
                return 200; // Default to OK for non-MockNode nodes
            }
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    OnPropertyChanged(nameof(IsHovered));
                }
            }
        }

        private bool _isDescendant;
        public bool IsDescendant
        {
            get => _isDescendant;
            set
            {
                if (_isDescendant != value)
                {
                    _isDescendant = value;
                    OnPropertyChanged(nameof(IsDescendant));
                }
            }
        }

        public void UpdateAncestorErrorStates(MockTreeNode node)
        {
            while (node != null)
            {
                node.IsDescendant = node.Children.OfType<MockTreeNode>().Any(
                    child => child.IsDescendant || child.StatusCodeForHighlight != 200 || child.IsHovered
                );
                node = node.Parent as MockTreeNode;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnMockNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Only need to react if the Response object itself changes
            if (e.PropertyName == nameof(MockNode.Url) || e.PropertyName == nameof(MockNode.MethodName))
            {
                Header = $"{((MockNode)sender).Url} - {((MockNode)sender).MethodName}";
                OnPropertyChanged(nameof(Header));
            }
        }

        private void OnResponsePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Response.StatusCode))
            {
                OnPropertyChanged(nameof(StatusCodeForHighlight));
            }
        }
    }
}
