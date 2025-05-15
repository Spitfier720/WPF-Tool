using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            DataContext = new MainWindowViewModel();
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Start Service button clicked!");
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Stop Service button clicked!");
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Load Mock File button clicked!");
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clear Log button clicked!");
        }
    }
}