using ToDoAplication.Views;
using ToDoAplication.ViewModels;

namespace ToDoAplication.Services
{
    public class DialogService : IDialogService
    {
        public string? ShowDescriptionDialog(string taskText, string currentDescription)
        {
            var viewModel = new DescribeViewModel(taskText, currentDescription);
            var window = new DescribeWindow { DataContext = viewModel };

            return window.ShowDialog() == true ? viewModel.Description : null;
        }
    }
}