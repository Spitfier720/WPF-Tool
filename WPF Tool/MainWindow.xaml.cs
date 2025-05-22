using System.Windows;
using System.Windows.Controls;

namespace WPF_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(new FileDialogService());
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if(sender is TreeViewItem item && item.DataContext is TreeNode node)
            {
                if(DataContext is MainWindowViewModel vm && vm.TreeNodeSelectedCommand.CanExecute(node))
                {
                    vm.TreeNodeSelectedCommand.Execute(node);
                }
            }

            e.Handled = true;
        }
    }
}