using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ToDoAplication.Commands;
using ToDoAplication.Data;
using ToDoAplication.Models;

namespace ToDoAplication.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoDbContext _context;
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

        public MainViewModel()
        {
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

            // Create input dialog for description
            var inputDialog = new Window
            {
                Title = "Add Description",
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(10)
            };

            var titleLabel = new System.Windows.Controls.TextBlock
            {
                Text = $"Task: {SelectedTask.Text}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var descriptionLabel = new System.Windows.Controls.TextBlock
            {
                Text = "Description:",
                Margin = new Thickness(0, 0, 0, 5)
            };

            var descriptionTextBox = new System.Windows.Controls.TextBox
            {
                Text = SelectedTask.Describe ?? string.Empty,
                MinHeight = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 30
            };

            okButton.Click += (s, e) =>
            {
                // Update description
                SelectedTask.Describe = descriptionTextBox.Text;
                _context.SaveChanges();
                inputDialog.DialogResult = true;
                inputDialog.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                inputDialog.DialogResult = false;
                inputDialog.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(titleLabel);
            stackPanel.Children.Add(descriptionLabel);
            stackPanel.Children.Add(descriptionTextBox);
            stackPanel.Children.Add(buttonPanel);

            inputDialog.Content = stackPanel;
            inputDialog.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}