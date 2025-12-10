using Microsoft.EntityFrameworkCore;
using ToDoAplication.Models;  

namespace ToDoAplication.Data
{
    public class TodoDbContext : DbContext
    {
        // Active tasks
        public DbSet<TodoItem> TodoItems { get; set; } = null!;

        // Archived tasks
        public DbSet<ArchivedTask> ArchivedTasks { get; set; } = null!;

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

            // Active tasks configuration
            modelBuilder.Entity<TodoItem>()
                .ToTable("TodoItems");
                
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.Text)
                .HasMaxLength(100)
                .IsRequired();

            // Archive configuration
            modelBuilder.Entity<ArchivedTask>()
                .ToTable("ArchivedTasks");
                
            modelBuilder.Entity<ArchivedTask>()
                .Property(t => t.Text)
                .HasMaxLength(100)
                .IsRequired();
                
            // Description column configuration
            modelBuilder.Entity<ArchivedTask>()
                .Property(t => t.Description)
                .HasMaxLength(500);
        }
    }
}
