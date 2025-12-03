using System.Windows;
using Microsoft.EntityFrameworkCore;
using ToDoAplication.Data;

namespace ToDoAplication
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new TodoDbContext(new DbContextOptions<TodoDbContext>()))
            {
                context.Database.EnsureDeleted();
                // Create new database if it doesn't exist
                context.Database.EnsureCreated();
            }
        }
    }
}
