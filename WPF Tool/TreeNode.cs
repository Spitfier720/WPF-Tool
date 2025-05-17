using System.Collections.ObjectModel;

namespace WPF_Tool
{
    public class TreeNode
    {
        public string Header { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; } = new();
        public TreeNode? Parent { get; set; }
        public object? Tag { get; set; }
    }
}