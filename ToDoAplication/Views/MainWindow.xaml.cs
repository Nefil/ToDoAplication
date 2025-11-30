using System.Windows;
using ToDoAplication.Services;
using ToDoAplication.ViewModels;

namespace ToDoAplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Inject DialogService into MainViewModel
            DataContext = new MainViewModel(new DialogService());
        }
    }
}