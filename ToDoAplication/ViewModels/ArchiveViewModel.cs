using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ToDoAplication.Commands;
using ToDoAplication.Data;
using ToDoAplication.Models;
using ToDoAplication.Services;

namespace ToDoAplication.ViewModels
{
    public class ArchiveViewModel : INotifyPropertyChanged
    {
        private readonly TodoDbContext _context;
        private ArchivedTask? _selectedArchivedTask;
        private readonly IDialogService _dialogService;

        public ObservableCollection<ArchivedTask> ArchivedTasks { get; } = new ObservableCollection<ArchivedTask>();

        public ArchivedTask? SelectedArchivedTask
        {
            get => _selectedArchivedTask;
            set 
            { 
                _selectedArchivedTask = value; 
                OnPropertyChanged();
                ((RelayCommand)ShowDescriptionCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand RestoreCommand { get; }
        public ICommand DeletePermanentlyCommand { get; }
        public ICommand ShowDescriptionCommand { get; }

        public ArchiveViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _context = new TodoDbContext(new DbContextOptions<TodoDbContext>());
            _context.Database.EnsureCreated();

            LoadArchivedTasksFromDatabase();

            RestoreCommand = new RelayCommand(RestoreMarkedTasks, CanRestoreMarkedTasks);
            DeletePermanentlyCommand = new RelayCommand(DeletePermanently, CanDeletePermanently);
            ShowDescriptionCommand = new RelayCommand(ShowDescription, CanShowDescription);
        }

        private void LoadArchivedTasksFromDatabase()
        {
            var tasksFromDb = _context.ArchivedTasks.ToList();
            foreach (var task in tasksFromDb)
            {
                ArchivedTasks.Add(task);
                task.PropertyChanged += Task_PropertyChanged;
            }
        }

        private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ArchivedTask.IsMarkedForRestore))
            {
                ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeletePermanentlyCommand).RaiseCanExecuteChanged();
            }
        }

        private bool CanShowDescription() => SelectedArchivedTask != null;

        private void ShowDescription()
        {
            if (SelectedArchivedTask == null)
                return;

            var description = string.IsNullOrWhiteSpace(SelectedArchivedTask.Description)
                ? "No description available"
                : SelectedArchivedTask.Description;

            MessageBox.Show(
                description,
                $"Description: {SelectedArchivedTask.Text}",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private bool CanRestoreMarkedTasks() => ArchivedTasks.Any(t => t.IsMarkedForRestore);

        private void RestoreMarkedTasks()
        {
            var tasksToRestore = ArchivedTasks.Where(t => t.IsMarkedForRestore).ToList();
            
            foreach (var archivedTask in tasksToRestore)
            {
                var restoredTask = new TodoItem
                {
                    Text = archivedTask.Text,
                    IsDone = archivedTask.IsCompleted,
                    Describe = archivedTask.Description  
                };
                
                _context.TodoItems.Add(restoredTask);
                
                archivedTask.PropertyChanged -= Task_PropertyChanged;
                _context.ArchivedTasks.Remove(archivedTask);
                ArchivedTasks.Remove(archivedTask);
            }
            
            _context.SaveChanges();
            ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeletePermanentlyCommand).RaiseCanExecuteChanged();
        }

        private bool CanDeletePermanently() => ArchivedTasks.Any(t => t.IsMarkedForRestore);

        private void DeletePermanently()
        {
            var tasksToDelete = ArchivedTasks.Where(t => t.IsMarkedForRestore).ToList();
            
            foreach (var task in tasksToDelete)
            {
                task.PropertyChanged -= Task_PropertyChanged;
                _context.ArchivedTasks.Remove(task);
                ArchivedTasks.Remove(task);
            }
            
            _context.SaveChanges();
            ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeletePermanentlyCommand).RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}