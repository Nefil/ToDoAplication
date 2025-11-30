using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace ToDoAplication.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }
        
        private string _text = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        private bool _isDone;
        public bool IsDone
        {
            get => _isDone;
            set { _isDone = value; OnPropertyChanged(); }
        }

        private string _describe = string.Empty;
        public string Describe
        {
            get => _describe;
            set { _describe = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
    }
}