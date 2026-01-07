using ITTitans.ToDoManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.ToDoManager.Data;

internal class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
