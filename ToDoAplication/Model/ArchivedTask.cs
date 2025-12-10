using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoAplication.Models 
{
    public class ArchivedTask : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public string Description { get; set; } = string.Empty;

        private bool _isMarkedForRestore;
        public bool IsMarkedForRestore
        {
            get => _isMarkedForRestore;
            set { _isMarkedForRestore = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}