using System.Windows;
using ToDoAplication.ViewModels;

namespace ToDoAplication.Views
{
    public partial class DescribeWindow : Window
    {
        public DescribeWindow()
        {
            InitializeComponent();
            
            // Subscribe to ViewModel's RequestClose event
            Loaded += DescribeWindow_Loaded;
        }

        private void DescribeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DescribeViewModel viewModel)
            {
                viewModel.RequestClose += ViewModel_RequestClose;
            }
        }

        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Unsubscribe to prevent memory leaks
            if (DataContext is DescribeViewModel viewModel)
            {
                viewModel.RequestClose -= ViewModel_RequestClose;
            }
        }
    }
}