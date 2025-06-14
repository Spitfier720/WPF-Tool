using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MockNodeEditorWindow : Window
    {
        public MockNodeEditorViewModel EditorViewModel { get; }

        public MockNodeEditorWindow()
        {
            InitializeComponent();
            EditorViewModel = new MockNodeEditorViewModel();
            DataContext = EditorViewModel;
            EditorViewModel.CloseRequested += (s, e) =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}
