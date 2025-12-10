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
                ((RelayCommand)DescribeCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DescribeCommand { get; }
        public ICommand DeleteMarkedCommand { get; }
        public ICommand ArchiveCommand { get; }

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            // Initialize database context
            _context = new TodoDbContext(new DbContextOptions<TodoDbContext>());
            _context.Database.EnsureCreated();

            // Load existing tasks from database
            LoadTasksFromDatabase();

            AddCommand = new RelayCommand(AddTask, CanAddTask);
            DescribeCommand = new RelayCommand(DescribeTask, CanDescribeTask);
            DeleteMarkedCommand = new RelayCommand(DeleteMarkedTasks, CanDeleteMarkedTasks);
            ArchiveCommand = new RelayCommand(ArchiveMarkedTasks, CanArchiveMarkedTasks);
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

        public void RefreshTasks()
        {
            // Unsubscribe from all tasks
            foreach (var task in Tasks.ToList())
            {
                task.PropertyChanged -= Task_PropertyChanged;
            }
            
            // Clear the collection
            Tasks.Clear();
            
            // Use a fresh context to reload data
            using (var freshContext = new TodoDbContext(new DbContextOptions<TodoDbContext>()))
            {
                var tasksFromDb = freshContext.TodoItems.ToList();
                foreach (var task in tasksFromDb)
                {
                    // Attach task to the main context
                    _context.Entry(task).State = EntityState.Detached;
                    _context.Attach(task);
                    
                    Tasks.Add(task);
                    task.PropertyChanged += Task_PropertyChanged;
                }
            }
            
            // Refresh button states
            ((RelayCommand)ArchiveCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteMarkedCommand).RaiseCanExecuteChanged();
        }

        private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // If IsDone, Describe or IsMarkedForDeletion changed, save to database
            if (e.PropertyName == nameof(TodoItem.IsDone) || 
                e.PropertyName == nameof(TodoItem.Describe) ||
                e.PropertyName == nameof(TodoItem.IsMarkedForDeletion))
            {
                _context.SaveChanges();
                
                // Update commands when IsDone or IsMarkedForDeletion changes
                if (e.PropertyName == nameof(TodoItem.IsMarkedForDeletion) || 
                    e.PropertyName == nameof(TodoItem.IsDone))
                {
                    ((RelayCommand)DeleteMarkedCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ArchiveCommand).RaiseCanExecuteChanged();
                }
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

        // DELETE MARKED - permanently deletes tasks with "Delete" checkbox
        private bool CanDeleteMarkedTasks() => Tasks.Any(t => t.IsMarkedForDeletion);

        private void DeleteMarkedTasks()
        {
            var tasksToDelete = Tasks.Where(t => t.IsMarkedForDeletion).ToList();
            
            foreach (var task in tasksToDelete)
            {
                // Unsubscribe PropertyChanged
                task.PropertyChanged -= Task_PropertyChanged;
                
                // Remove from database
                _context.TodoItems.Remove(task);
                
                // Remove from UI collection
                Tasks.Remove(task);
            }
            
            _context.SaveChanges();
            
            // Update button state after deletion
            ((RelayCommand)DeleteMarkedCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ArchiveCommand).RaiseCanExecuteChanged();
        }

        // ARCHIVE - archives only tasks with IsDone = true
        private bool CanArchiveMarkedTasks() => Tasks.Any(t => t.IsDone);

        private void ArchiveMarkedTasks()
        {
            var tasksToArchive = Tasks.Where(t => t.IsDone).ToList();
            
            foreach (var task in tasksToArchive)
            {
                // Create archived task with description
                var archivedTask = new ArchivedTask
                {
                    Text = task.Text,
                    IsCompleted = task.IsDone,
                    Description = task.Describe,
                    ArchivedDate = DateTime.Now
                };
                
                // Add to archive
                _context.ArchivedTasks.Add(archivedTask);
                
                // Remove from active tasks
                task.PropertyChanged -= Task_PropertyChanged;
                _context.TodoItems.Remove(task);
                Tasks.Remove(task);
            }
            
            _context.SaveChanges();
            
            // Update button states
            ((RelayCommand)ArchiveCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteMarkedCommand).RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}