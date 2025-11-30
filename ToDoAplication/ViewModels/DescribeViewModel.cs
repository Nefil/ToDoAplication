using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToDoAplication.Commands;

namespace ToDoAplication.ViewModels
{
    public class DescribeViewModel : INotifyPropertyChanged
    {
        private string _taskText = string.Empty;
        private string _description = string.Empty;

        public string TaskText
        {
            get => _taskText;
            set { _taskText = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        // Event to notify View to close dialog
        public event EventHandler<bool>? RequestClose;

        public DescribeViewModel(string taskText, string initialDescription)
        {
            TaskText = taskText;
            Description = initialDescription ?? string.Empty;
            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnOk()
        {
            // Notify View to close with DialogResult = true
            RequestClose?.Invoke(this, true);
        }

        private void OnCancel()
        {
            // Notify View to close with DialogResult = false
            RequestClose?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}