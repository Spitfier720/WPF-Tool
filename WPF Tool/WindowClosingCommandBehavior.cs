using System;
using System.Windows;
using System.Windows.Input;

namespace WPF_Tool
{
    public static class WindowClosingCommandBehavior
    {
        public static readonly DependencyProperty ClosingCommandProperty =
            DependencyProperty.RegisterAttached(
                "ClosingCommand",
                typeof(ICommand),
                typeof(WindowClosingCommandBehavior),
                new PropertyMetadata(null, OnClosingCommandChanged));

        public static ICommand GetClosingCommand(Window window) =>
            (ICommand)window.GetValue(ClosingCommandProperty);

        public static void SetClosingCommand(Window window, ICommand value) =>
            window.SetValue(ClosingCommandProperty, value);

        private static void OnClosingCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                window.Closing -= Window_Closing;
                if (e.NewValue is ICommand)
                {
                    window.Closing += Window_Closing;
                }
            }
        }

        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Window window)
            {
                var command = GetClosingCommand(window);
                if (command != null && command.CanExecute(e))
                {
                    command.Execute(e);
                }
            }
        }
    }
}