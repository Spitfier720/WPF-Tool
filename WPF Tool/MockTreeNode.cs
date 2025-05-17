using EasyMockLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Tool
{
    internal enum NodeTypes
    {
        MockFile,
        MockItem
    }
    internal class MockTreeNode : TreeNode
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
            Header = $"{node.Request.ServiceName} - {node.Request.MethodName}";
        }

        public bool IsDirty { get; set; }

        public NodeTypes NodeType { get; set; }
    }
}
