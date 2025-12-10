using System.Windows;
using ToDoAplication.Services;
using ToDoAplication.ViewModels;

namespace ToDoAplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Inject DialogService into MainViewModel
            DataContext = new MainViewModel(new DialogService());
        }

        private void ViewArchive_Click(object sender, RoutedEventArgs e)
        {
            var archiveWindow = new Views.ArchiveWindow();
            archiveWindow.ShowDialog();
            
            // Refresh task list after archive window is closed
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RefreshTasks();
            }
        }
    }
}