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

        private bool _isDescendantError;
        public bool IsDescendantError
        {
            get => _isDescendantError;
            set
            {
                if (_isDescendantError != value)
                {
                    _isDescendantError = value;
                    OnPropertyChanged(nameof(IsDescendantError));
                }
            }
        }

        public void UpdateAncestorErrorStates(MockTreeNode node)
        {
            while (node != null)
            {
                node.IsDescendantError = node.Children.OfType<MockTreeNode>().Any(
                    child => child.IsDescendantError || child.StatusCodeForHighlight != 200 || child.IsHovered
                );
                node = node.Parent as MockTreeNode;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
