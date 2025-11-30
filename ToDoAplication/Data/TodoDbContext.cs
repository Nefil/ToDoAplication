using Microsoft.EntityFrameworkCore;
using ToDoAplication.Models;

namespace ToDoAplication.Data
{
    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; } = null!;

        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=todos.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TodoItem>(entity =>
            {
                // Map only Text property
                entity.Property(t => t.Text)
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasColumnName("Text"); // Force column name

                // Ignore _text field (should not be needed, but just in case)
                entity.Ignore("_text");
            });
        }
    }
}
