using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToDoAplication.Commands;
using ToDoAplication.Models;

namespace ToDoAplication.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _newTaskText = string.Empty;
        private TodoItem? _selectedTask;

        public ObservableCollection<TodoItem> Tasks { get; } = new ObservableCollection<TodoItem>();

        public string NewTaskText
        {
            get => _newTaskText;
            set { _newTaskText = value; OnPropertyChanged(); ((RelayCommand)AddCommand).RaiseCanExecuteChanged(); }
        }

        public TodoItem? SelectedTask
        {
            get => _selectedTask;
            set { _selectedTask = value; OnPropertyChanged(); ((RelayCommand)RemoveCommand).RaiseCanExecuteChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }

        public MainViewModel()
        {
            AddCommand = new RelayCommand(AddTask, CanAddTask);
            RemoveCommand = new RelayCommand(RemoveTask, CanRemoveTask);
        }

        private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskText);

        private void AddTask()
        {
            Tasks.Add(new TodoItem { Text = NewTaskText.Trim() });
            NewTaskText = string.Empty;
        }

        private bool CanRemoveTask() => SelectedTask != null;

        private void RemoveTask()
        {
            if (SelectedTask != null)
            {
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}