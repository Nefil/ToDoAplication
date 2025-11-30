using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ToDoAplication.Commands;
using ToDoAplication.Data;
using ToDoAplication.Models;
using ToDoAplication.Services;

namespace ToDoAplication.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoDbContext _context;
        private string _newTaskText = string.Empty;
        private TodoItem? _selectedTask;
        private readonly IDialogService _dialogService;

        public ObservableCollection<TodoItem> Tasks { get; } = new ObservableCollection<TodoItem>();

        public string NewTaskText
        {
            get => _newTaskText;
            set { _newTaskText = value; OnPropertyChanged(); ((RelayCommand)AddCommand).RaiseCanExecuteChanged(); }
        }

        public TodoItem? SelectedTask
        {
            get => _selectedTask;
            set 
            { 
                _selectedTask = value; 
                OnPropertyChanged(); 
                ((RelayCommand)RemoveCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DescribeCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand DescribeCommand { get; }

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            // Initialize database context
            _context = new TodoDbContext(new DbContextOptions<TodoDbContext>());
            _context.Database.EnsureCreated();

            // Load existing tasks from database
            LoadTasksFromDatabase();

            AddCommand = new RelayCommand(AddTask, CanAddTask);
            RemoveCommand = new RelayCommand(RemoveTask, CanRemoveTask);
            DescribeCommand = new RelayCommand(DescribeTask, CanDescribeTask);
        }

        private void LoadTasksFromDatabase()
        {
            var tasksFromDb = _context.TodoItems.ToList();
            foreach (var task in tasksFromDb)
            {
                Tasks.Add(task);
                
                // Subscribe to PropertyChanged event for each task
                task.PropertyChanged += Task_PropertyChanged;
            }
        }

        private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // If IsDone or Describe changed, save to database
            if (e.PropertyName == nameof(TodoItem.IsDone) || e.PropertyName == nameof(TodoItem.Describe))
            {
                _context.SaveChanges();
            }
        }

        private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskText);

        private void AddTask()
        {
            var newTask = new TodoItem { Text = NewTaskText.Trim() };
            
            // Add to database
            _context.TodoItems.Add(newTask);
            _context.SaveChanges();
            
            // Add to UI collection
            Tasks.Add(newTask);
            
            // Subscribe PropertyChanged for new task
            newTask.PropertyChanged += Task_PropertyChanged;
            
            NewTaskText = string.Empty;
        }

        private bool CanRemoveTask() => SelectedTask != null;

        private void RemoveTask()
        {
            if (SelectedTask != null)
            {
                // Unsubscribe PropertyChanged
                SelectedTask.PropertyChanged -= Task_PropertyChanged;
                
                // Remove from database
                _context.TodoItems.Remove(SelectedTask);
                _context.SaveChanges();
                
                // Remove from UI collection
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        private bool CanDescribeTask() => SelectedTask != null;

        private void DescribeTask()
        {
            if (SelectedTask == null)
                return;

            var newDescription = _dialogService.ShowDescriptionDialog(SelectedTask.Text, SelectedTask.Describe);
            
            if (newDescription != null)
            {
                SelectedTask.Describe = newDescription;
                _context.SaveChanges();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}