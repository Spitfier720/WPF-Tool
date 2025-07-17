using System.Windows;

namespace WPF_Tool
{
    public class DialogService : IDialogService
    {
        public bool ConfirmCloseWithUnsavedChanges()
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you really want to exit?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }
    }
}