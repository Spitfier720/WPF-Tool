public class ContextMenuParameterConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var treeViewItem = values[0] as TreeViewItem;
        var action = parameter as string;
        var node = treeViewItem?.DataContext as MockTreeNode;
        return new ContextMenuActionParameter { Action = action, Node = node };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}