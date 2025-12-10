using System.Windows;
using ToDoAplication.Services;
using ToDoAplication.ViewModels;

namespace ToDoAplication.Views
{
    public partial class ArchiveWindow : Window
    {
        public ArchiveWindow()
        {
            InitializeComponent();
            DataContext = new ArchiveViewModel(new DialogService());
        }
    }
}