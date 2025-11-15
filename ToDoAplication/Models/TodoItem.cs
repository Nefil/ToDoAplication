using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoAplication.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private bool _isDone;

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public bool IsDone
        {
            get => _isDone;
            set { _isDone = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}