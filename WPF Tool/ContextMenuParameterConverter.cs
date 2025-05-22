using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPF_Tool
{
    public class ContextMenuParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var treeViewItem = values[0] as TreeViewItem;
            var action = parameter as string;
            var node = treeViewItem?.DataContext as MockTreeNode;
            return Tuple.Create(action, node);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}